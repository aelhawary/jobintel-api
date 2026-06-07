namespace RecruitmentPlatformAPI.Configuration
{
    /// <summary>
    /// Constants and configuration for the assessment module
    /// </summary>
    public static class AssessmentSettings
    {
        /// <summary>
        /// Minimum days between assessment attempts (cooldown period)
        /// </summary>
        public const int CooldownDays = 60;
        
        /// <summary>
        /// How long assessment scores remain valid (in months)
        /// </summary>
        public const int ScoreValidityMonths = 18;
        
        /// <summary>
        /// Default time limit for completing an assessment (in minutes)
        /// </summary>
        public const int DefaultTimeLimitMinutes = 20;
        
        /// <summary>
        /// Default time allowed per question (in seconds)
        /// </summary>
        public const int DefaultTimePerQuestionSeconds = 60;
        
        /// <summary>
        /// Total number of questions in an assessment
        /// </summary>
        public const int TotalQuestionsPerAssessment = 20;
        
        /// <summary>
        /// Number of technical questions (80% of total)
        /// </summary>
        public const int TechnicalQuestionsCount = 16;
        
        /// <summary>
        /// Number of soft skill questions (20% of total)
        /// </summary>
        public const int SoftSkillQuestionsCount = 4;
        
        /// <summary>
        /// Minimum passing score percentage
        /// </summary>
        public const decimal MinimumPassingScore = 50.0m;

        /// <summary>
        /// Standard soft skill areas for evaluation
        /// </summary>
        public static readonly string[] SoftSkillAreas = { "Communication", "Problem Solving", "Agile & Teamwork" };
    }
}
