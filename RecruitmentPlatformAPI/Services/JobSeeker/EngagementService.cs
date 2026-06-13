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
            // Use a serializable transaction to prevent race conditions when two recruiters
            // load the same job's candidates simultaneously (delete-then-insert must be atomic).
            using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // Remove existing recommendations for this job (idempotent re-run)
                var existing = await _context.Recommendations
                    .Where(r => r.JobId == jobId)
                    .ToListAsync();

                if (existing.Any())
                    _context.Recommendations.RemoveRange(existing);

                // Insert fresh recommendations from AI results
                var recommendations = candidates.Select(c => new Recommendation
                {
                    JobId = jobId,
                    JobSeekerId = c.JobSeekerId,
                    MatchScore = c.MatchScore,
                    AiReasoning = c.AiReasoning,
                    MatchedSkillsJson = JsonSerializer.Serialize(c.MatchedSkills),
                    MissingSkillsJson = JsonSerializer.Serialize(c.MissingSkills),
                    IsViewed = false,
                    GeneratedAt = DateTime.UtcNow
                }).ToList();

                _context.Recommendations.AddRange(recommendations);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                _logger.LogInformation(
                    "Stored {Count} recommendations for Job {JobId}",
                    recommendations.Count, jobId);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Failed to store recommendations for Job {JobId}", jobId);
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
