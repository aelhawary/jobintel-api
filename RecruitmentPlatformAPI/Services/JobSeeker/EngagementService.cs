using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Models.JobSeeker;
using RecruitmentPlatformAPI.Models.Jobs;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public class EngagementService : IEngagementService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EngagementService> _logger;

        private const int DEDUP_WINDOW_HOURS = 1;

        public EngagementService(AppDbContext context, ILogger<EngagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RecordSearchAppearancesAsync(IEnumerable<int> jobSeekerIds, int? recruiterId, int? jobId)
        {
            try
            {
                var ids = jobSeekerIds.ToList();
                if (!ids.Any()) return;

                var cutoff = DateTime.UtcNow.AddHours(-DEDUP_WINDOW_HOURS);

                // Deduplicate: for each jobseeker, skip if this recruiter already triggered a Search view within the window
                var existingQuery = _context.ProfileViews
                    .Where(pv =>
                        pv.ViewType == "Search" &&
                        pv.ViewedAt > cutoff);

                if (recruiterId.HasValue)
                {
                    existingQuery = existingQuery.Where(pv => pv.ViewerRecruiterId == recruiterId.Value);
                }

                if (jobId.HasValue)
                {
                    existingQuery = existingQuery.Where(pv => pv.JobId == jobId.Value);
                }

                var existingJobSeekerIds = await existingQuery
                    .Select(pv => pv.JobSeekerId)
                    .Distinct()
                    .ToListAsync();

                var existingSet = new HashSet<int>(existingJobSeekerIds);
                var newIds = ids.Where(id => !existingSet.Contains(id)).ToList();

                if (!newIds.Any()) return;

                var views = newIds.Select(id => new ProfileView
                {
                    JobSeekerId = id,
                    ViewerRecruiterId = recruiterId,
                    JobId = jobId,
                    ViewType = "Search",
                    ViewedAt = DateTime.UtcNow
                });

                _context.ProfileViews.AddRange(views);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Non-critical — don't let analytics tracking break the main flow
                _logger.LogWarning(ex, "Failed to record search appearances for {Count} job seekers", jobSeekerIds.Count());
            }
        }

        public async Task RecordProfileViewAsync(int jobSeekerId, int recruiterId, int? jobId)
        {
            try
            {
                // Deduplicate: don't record if this recruiter already viewed this profile in the last hour
                var cutoff = DateTime.UtcNow.AddHours(-DEDUP_WINDOW_HOURS);
                var alreadyViewed = await _context.ProfileViews.AnyAsync(pv =>
                    pv.JobSeekerId == jobSeekerId &&
                    pv.ViewerRecruiterId == recruiterId &&
                    pv.ViewType == "ProfileClick" &&
                    pv.ViewedAt > cutoff);

                if (alreadyViewed) return;

                _context.ProfileViews.Add(new ProfileView
                {
                    JobSeekerId = jobSeekerId,
                    ViewerRecruiterId = recruiterId,
                    JobId = jobId,
                    ViewType = "ProfileClick",
                    ViewedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record profile view for job seeker {JobSeekerId}", jobSeekerId);
            }
        }

        public async Task StoreRecommendationsAsync(int jobId, List<MatchedCandidateDto> candidates)
        {
            const int maxRetries = 1;
            int attempt = 0;

            while (true)
            {
                try
                {
                    var existing = await _context.Recommendations
                        .Where(r => r.JobId == jobId)
                        .ToListAsync();

                    var existingMap = existing.ToDictionary(r => r.JobSeekerId);
                    var incomingIds = new HashSet<int>(candidates.Select(c => c.JobSeekerId));
                    var toInsert = new List<Recommendation>();

                    foreach (var c in candidates)
                    {
                        if (existingMap.TryGetValue(c.JobSeekerId, out var existingRec))
                        {
                            existingRec.MatchScore = c.MatchScore;
                            existingRec.AiReasoning = c.AiReasoning;
                            existingRec.MatchedSkillsJson = JsonSerializer.Serialize(c.MatchedSkills);
                            existingRec.MissingSkillsJson = JsonSerializer.Serialize(c.MissingSkills);
                            existingRec.GeneratedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            toInsert.Add(new Recommendation
                            {
                                JobId = jobId,
                                JobSeekerId = c.JobSeekerId,
                                MatchScore = c.MatchScore,
                                AiReasoning = c.AiReasoning,
                                MatchedSkillsJson = JsonSerializer.Serialize(c.MatchedSkills),
                                MissingSkillsJson = JsonSerializer.Serialize(c.MissingSkills),
                                GeneratedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // Delete stale recommendations (candidates no longer in the AI result)
                    var stale = existing.Where(r => !incomingIds.Contains(r.JobSeekerId)).ToList();
                    if (stale.Any())
                        _context.Recommendations.RemoveRange(stale);

                    if (toInsert.Any())
                        _context.Recommendations.AddRange(toInsert);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Stored/Updated {Count} recommendations for Job {JobId} (inserted {Inserted}, updated {Updated}, removed {Removed})",
                        candidates.Count, jobId, toInsert.Count, existing.Count - stale.Count, stale.Count);

                    return;
                }
                catch (Exception ex) when (attempt < maxRetries && ex is DbUpdateException)
                {
                    attempt++;
                    _logger.LogWarning(ex,
                        "Race condition storing recommendations for Job {JobId}, retrying (attempt {Attempt}/{MaxRetries})",
                        jobId, attempt, maxRetries);

                    // Detach all tracked entities to get a clean state for retry
                    foreach (var entry in _context.ChangeTracker.Entries().ToList())
                        entry.State = EntityState.Detached;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to store recommendations for Job {JobId}", jobId);
                    throw;
                }
            }
        }

        public async Task<EngagementStatsDto> GetEngagementStatsAsync(int jobSeekerId)
        {
            var now = DateTime.UtcNow;
            var thisWeekStart = now.AddDays(-7);
            var lastWeekStart = now.AddDays(-14);

            // ─── Profile Views & Search Appearances (optimized SQL aggregation) ───
            var weeklyStats = await _context.ProfileViews
                .Where(pv => pv.JobSeekerId == jobSeekerId && pv.ViewedAt >= lastWeekStart)
                .GroupBy(pv => new { pv.ViewType, IsThisWeek = pv.ViewedAt >= thisWeekStart })
                .Select(g => new
                {
                    g.Key.ViewType,
                    g.Key.IsThisWeek,
                    Count = g.Count()
                })
                .ToListAsync();

            var searchThisWeek = weeklyStats
                .Where(s => s.ViewType == "Search" && s.IsThisWeek)
                .Sum(s => s.Count);
            var searchLastWeek = weeklyStats
                .Where(s => s.ViewType == "Search" && !s.IsThisWeek)
                .Sum(s => s.Count);
            var profileThisWeek = weeklyStats
                .Where(s => s.ViewType == "ProfileClick" && s.IsThisWeek)
                .Sum(s => s.Count);
            var profileLastWeek = weeklyStats
                .Where(s => s.ViewType == "ProfileClick" && !s.IsThisWeek)
                .Sum(s => s.Count);

            // All-time totals (single query each)
            var totalSearch = await _context.ProfileViews
                .CountAsync(pv => pv.JobSeekerId == jobSeekerId && pv.ViewType == "Search");
            var totalProfile = await _context.ProfileViews
                .CountAsync(pv => pv.JobSeekerId == jobSeekerId && pv.ViewType == "ProfileClick");

            // ─── Recommendations ───
            var recStats = await _context.Recommendations
                .Where(r => r.JobSeekerId == jobSeekerId && r.GeneratedAt >= lastWeekStart)
                .GroupBy(r => new { IsThisWeek = r.GeneratedAt >= thisWeekStart })
                .Select(g => new
                {
                    g.Key.IsThisWeek,
                    Count = g.Count()
                })
                .ToListAsync();

            var recThisWeek = recStats.Where(s => s.IsThisWeek).Sum(s => s.Count);
            var recLastWeek = recStats.Where(s => !s.IsThisWeek).Sum(s => s.Count);
            var totalRec = await _context.Recommendations
                .CountAsync(r => r.JobSeekerId == jobSeekerId);

            return new EngagementStatsDto
            {
                SearchAppearancesThisWeek = searchThisWeek,
                ProfileViewsThisWeek = profileThisWeek,
                RecommendationsThisWeek = recThisWeek,
                SearchAppearancesLastWeek = searchLastWeek,
                ProfileViewsLastWeek = profileLastWeek,
                RecommendationsLastWeek = recLastWeek,
                TotalSearchAppearances = totalSearch,
                TotalProfileViews = totalProfile,
                TotalRecommendations = totalRec,
                SearchAppearancesTrend = searchLastWeek > 0
                    ? Math.Round((double)(searchThisWeek - searchLastWeek) / searchLastWeek * 100, 1)
                    : null,
                ProfileViewsTrend = profileLastWeek > 0
                    ? Math.Round((double)(profileThisWeek - profileLastWeek) / profileLastWeek * 100, 1)
                    : null,
                RecommendationsTrend = recLastWeek > 0
                    ? Math.Round((double)(recThisWeek - recLastWeek) / recLastWeek * 100, 1)
                    : null,
            };
        }
    }
}
