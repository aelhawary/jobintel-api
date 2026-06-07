namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Engagement statistics for a job seeker's dashboard widget.
    /// </summary>
    public class EngagementStatsDto
    {
        /// <summary>Number of times the profile appeared in recruiter search results this week.</summary>
        public int SearchAppearancesThisWeek { get; set; }

        /// <summary>Number of times a recruiter clicked into the full profile this week.</summary>
        public int ProfileViewsThisWeek { get; set; }

        /// <summary>Search appearances last week (for trend comparison).</summary>
        public int SearchAppearancesLastWeek { get; set; }

        /// <summary>Profile views last week (for trend comparison).</summary>
        public int ProfileViewsLastWeek { get; set; }

        /// <summary>All-time total search appearances.</summary>
        public int TotalSearchAppearances { get; set; }

        /// <summary>All-time total profile views.</summary>
        public int TotalProfileViews { get; set; }

        /// <summary>Percentage change in search appearances vs last week. Null if last week was 0.</summary>
        public double? SearchAppearancesTrend { get; set; }

        /// <summary>Percentage change in profile views vs last week. Null if last week was 0.</summary>
        public double? ProfileViewsTrend { get; set; }
    }
}
