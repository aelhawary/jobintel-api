using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.Services.Auth;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    /// <summary>
    /// Background service that runs daily to send weekly engagement digests to job seekers.
    /// It checks if it's Monday and sends the email if the user had any activity in the past week.
    /// </summary>
    public class WeeklyEngagementDigestService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeeklyEngagementDigestService> _logger;

        public WeeklyEngagementDigestService(IServiceProvider serviceProvider, ILogger<WeeklyEngagementDigestService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly Engagement Digest Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var today = DateTime.UtcNow.DayOfWeek;
                    if (today == DayOfWeek.Monday)
                    {
                        _logger.LogInformation("It's Monday! Processing weekly engagement digests.");
                        await ProcessWeeklyDigestsAsync(stoppingToken);
                    }

                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Expected when shutting down
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Weekly Engagement Digest Service. Retrying in 1 hour.");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Weekly Engagement Digest Service is stopping.");
        }

        private async Task ProcessWeeklyDigestsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

            // Get all job seekers who had at least 1 view, search appearance, or recommendation in the last 7 days
            var activeFromViews = context.ProfileViews
                .Where(pv => pv.ViewedAt >= oneWeekAgo)
                .Select(pv => pv.JobSeekerId);

            var activeFromRecs = context.Recommendations
                .Where(r => r.GeneratedAt >= oneWeekAgo)
                .Select(r => r.JobSeekerId);

            var activeSeekerIds = await activeFromViews
                .Union(activeFromRecs)
                .Distinct()
                .ToListAsync(stoppingToken);

            if (!activeSeekerIds.Any())
            {
                _logger.LogInformation("No active profile views this week. Skipping digest emails.");
                return;
            }

            foreach (var seekerId in activeSeekerIds)
            {
                if (stoppingToken.IsCancellationRequested) break;

                var jobSeeker = await context.JobSeekers
                    .Include(js => js.User)
                    .FirstOrDefaultAsync(js => js.Id == seekerId, stoppingToken);

                if (jobSeeker == null || !jobSeeker.User.IsActive || string.IsNullOrEmpty(jobSeeker.User.Email))
                    continue;

                var searchAppearances = await context.ProfileViews
                    .CountAsync(pv => pv.JobSeekerId == seekerId && pv.ViewType == "Search" && pv.ViewedAt >= oneWeekAgo, stoppingToken);

                var profileViews = await context.ProfileViews
                    .CountAsync(pv => pv.JobSeekerId == seekerId && pv.ViewType == "ProfileClick" && pv.ViewedAt >= oneWeekAgo, stoppingToken);

                await emailService.SendWeeklyDigestAsync(
                    jobSeeker.User.Email,
                    jobSeeker.User.FirstName,
                    searchAppearances,
                    profileViews);

                await Task.Delay(500, stoppingToken);
            }

            _logger.LogInformation("Finished processing weekly digests for {Count} job seekers.", activeSeekerIds.Count);
        }
    }
}
