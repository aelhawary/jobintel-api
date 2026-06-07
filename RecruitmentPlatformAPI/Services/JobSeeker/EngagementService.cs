using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public class EngagementService : IEngagementService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EngagementService> _logger;

        public EngagementService(AppDbContext context, ILogger<EngagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RecordSearchAppearancesAsync(IEnumerable<int> jobSeekerIds, int? recruiterId)
        {
            try
            {
                var views = jobSeekerIds.Select(id => new ProfileView
                {
                    JobSeekerId = id,
                    ViewerRecruiterId = recruiterId,
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

        public async Task RecordProfileViewAsync(int jobSeekerId, int recruiterId)
        {
            try
            {
                // Deduplicate: don't record if this recruiter already viewed this profile in the last hour
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                var alreadyViewed = await _context.ProfileViews.AnyAsync(pv =>
                    pv.JobSeekerId == jobSeekerId &&
                    pv.ViewerRecruiterId == recruiterId &&
                    pv.ViewType == "ProfileClick" &&
                    pv.ViewedAt > oneHourAgo);

                if (alreadyViewed) return;

                _context.ProfileViews.Add(new ProfileView
                {
                    JobSeekerId = jobSeekerId,
                    ViewerRecruiterId = recruiterId,
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

        public async Task<EngagementStatsDto> GetEngagementStatsAsync(int jobSeekerId)
        {
            var now = DateTime.UtcNow;
            var thisWeekStart = now.AddDays(-7);
            var lastWeekStart = now.AddDays(-14);

            var allViews = await _context.ProfileViews
                .Where(pv => pv.JobSeekerId == jobSeekerId && pv.ViewedAt >= lastWeekStart)
                .Select(pv => new { pv.ViewType, pv.ViewedAt })
                .ToListAsync();

            var thisWeekViews = allViews.Where(v => v.ViewedAt >= thisWeekStart).ToList();
            var lastWeekViews = allViews.Where(v => v.ViewedAt >= lastWeekStart && v.ViewedAt < thisWeekStart).ToList();

            var searchThisWeek = thisWeekViews.Count(v => v.ViewType == "Search");
            var searchLastWeek = lastWeekViews.Count(v => v.ViewType == "Search");
            var profileThisWeek = thisWeekViews.Count(v => v.ViewType == "ProfileClick");
            var profileLastWeek = lastWeekViews.Count(v => v.ViewType == "ProfileClick");

            // Get all-time totals
            var totalSearch = await _context.ProfileViews.CountAsync(pv => pv.JobSeekerId == jobSeekerId && pv.ViewType == "Search");
            var totalProfile = await _context.ProfileViews.CountAsync(pv => pv.JobSeekerId == jobSeekerId && pv.ViewType == "ProfileClick");

            return new EngagementStatsDto
            {
                SearchAppearancesThisWeek = searchThisWeek,
                ProfileViewsThisWeek = profileThisWeek,
                SearchAppearancesLastWeek = searchLastWeek,
                ProfileViewsLastWeek = profileLastWeek,
                TotalSearchAppearances = totalSearch,
                TotalProfileViews = totalProfile,
                SearchAppearancesTrend = searchLastWeek > 0 ? Math.Round((double)(searchThisWeek - searchLastWeek) / searchLastWeek * 100, 1) : null,
                ProfileViewsTrend = profileLastWeek > 0 ? Math.Round((double)(profileThisWeek - profileLastWeek) / profileLastWeek * 100, 1) : null,
            };
        }
    }
}
