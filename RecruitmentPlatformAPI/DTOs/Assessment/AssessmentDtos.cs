using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Assessment
{
    // ═══════════════════════════════════════════════════════════════════════════════
    // ELIGIBILITY
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Response for eligibility check - determines if user can start an assessment
    /// </summary>
    public class EligibilityResponseDto
    {
        /// <summary>
        /// Whether the user is eligible to start an assessment
        /// </summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Reason why user is not eligible (if applicable)
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Whether the user has completed their profile (Step 4)
        /// </summary>
        public bool HasCompletedProfile { get; set; }

        /// <summary>
        /// Whether the user has set their job title
        /// </summary>
        public bool HasJobTitle { get; set; }

        /// <summary>
        /// Whether the user has an in-progress assessment
        /// </summary>
        public bool HasInProgressAssessment { get; set; }

        /// <summary>
        /// Whether the user is within the 60-day cooldown period
        /// </summary>
        public bool IsInCooldownPeriod { get; set; }

        /// <summary>
        /// When the cooldown period ends (if in cooldown)
        /// </summary>
        public DateTime? CooldownEndsAt { get; set; }

        /// <summary>
        /// Days remaining until eligible (if in cooldown)
        /// </summary>
        public int? DaysUntilEligible { get; set; }

        /// <summary>
        /// Number of previous assessment attempts
        /// </summary>
        public int PreviousAttempts { get; set; }

        /// <summary>
        /// Current active assessment score (if any)
        /// </summary>
        public decimal? CurrentScore { get; set; }

        /// <summary>
        /// When the current score expires
        /// </summary>
        public DateTime? ScoreExpiresAt { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // START ASSESSMENT
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Response when starting a new assessment
    /// </summary>
    public class StartAssessmentResponseDto
    {
        /// <summary>
        /// The ID of the created assessment attempt
        /// </summary>
        public int AttemptId { get; set; }

        /// <summary>
        /// Total number of questions in this assessment
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Number of technical questions
        /// </summary>
        public int TechnicalQuestions { get; set; }

        /// <summary>
        /// Number of soft skill questions
        /// </summary>
        public int SoftSkillQuestions { get; set; }

        /// <summary>
        /// Time limit in minutes for the entire assessment
        /// </summary>
        public int TimeLimitMinutes { get; set; }

        /// <summary>
        /// When the assessment started
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// When the assessment expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Job title at time of assessment
        /// </summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// Role family for question targeting
        /// </summary>
        public string RoleFamily { get; set; } = string.Empty;

        /// <summary>
        /// Seniority level for question targeting
        /// </summary>
        public string SeniorityLevel { get; set; } = string.Empty;

        /// <summary>
        /// Which attempt number this is (1st, 2nd, etc.)
        /// </summary>
        public int RetakeNumber { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // CURRENT STATUS
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Current assessment attempt status
    /// </summary>
    public class AssessmentStatusResponseDto
    {
        /// <summary>
        /// The assessment attempt ID
        /// </summary>
        public int AttemptId { get; set; }

        /// <summary>
        /// Current status (InProgress, Completed, Abandoned, Expired)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Total questions in the assessment
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Number of questions answered
        /// </summary>
        public int QuestionsAnswered { get; set; }

        /// <summary>
        /// Number of questions remaining
        /// </summary>
        public int QuestionsRemaining { get; set; }

        /// <summary>
        /// When the assessment started
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// When the assessment expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Seconds remaining until expiration
        /// </summary>
        public int TimeRemainingSeconds { get; set; }

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public decimal ProgressPercentage { get; set; }

        /// <summary>
        /// Whether the assessment has expired
        /// </summary>
        public bool IsExpired { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // QUESTION
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A single question to present to the user
    /// </summary>
    public class QuestionResponseDto
    {
        /// <summary>
        /// The question ID
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Question number in sequence (1-30)
        /// </summary>
        public int QuestionNumber { get; set; }

        /// <summary>
        /// Total questions in assessment
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// The question text
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Category (Technical or SoftSkill)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Difficulty level (Easy, Medium, Hard)
        /// </summary>
        public string Difficulty { get; set; } = string.Empty;

        /// <summary>
        /// The 4 answer options
        /// </summary>
        public List<string> Options { get; set; } = new();

        /// <summary>
        /// Time allowed for this question in seconds
        /// </summary>
        public int TimeAllowedSeconds { get; set; }

        /// <summary>
        /// Seconds remaining in the entire assessment
        /// </summary>
        public int TimeRemainingInAssessmentSeconds { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // SUBMIT ANSWER
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Request to submit an answer
    /// </summary>
    public class SubmitAnswerRequestDto
    {
        /// <summary>
        /// The question ID being answered
        /// </summary>
        [Required(ErrorMessage = "Question ID is required")]
        public int QuestionId { get; set; }

        /// <summary>
        /// Selected answer index (0-3)
        /// </summary>
        [Required(ErrorMessage = "Answer selection is required")]
        [Range(0, 3, ErrorMessage = "Answer index must be between 0 and 3")]
        public int SelectedAnswerIndex { get; set; }

        /// <summary>
        /// Time spent on this question in seconds (for analytics)
        /// </summary>
        [Range(0, 3600, ErrorMessage = "Time spent must be between 0 and 3600 seconds")]
        public int TimeSpentSeconds { get; set; }
    }

    /// <summary>
    /// Response after submitting an answer (no correctness info in exam mode)
    /// </summary>
    public class SubmitAnswerResponseDto
    {
        /// <summary>
        /// Whether the answer was recorded successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of questions answered so far
        /// </summary>
        public int QuestionsAnswered { get; set; }

        /// <summary>
        /// Number of questions remaining
        /// </summary>
        public int QuestionsRemaining { get; set; }

        /// <summary>
        /// Whether all questions have been answered
        /// </summary>
        public bool IsAssessmentComplete { get; set; }

        /// <summary>
        /// Seconds remaining in the assessment
        /// </summary>
        public int TimeRemainingSeconds { get; set; }

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public decimal ProgressPercentage { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // COMPLETION / RESULT
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Assessment completion result with full details
    /// </summary>
    public class AssessmentResultResponseDto
    {
        /// <summary>
        /// The assessment attempt ID
        /// </summary>
        public int AttemptId { get; set; }

        /// <summary>
        /// Status (Completed, Abandoned, Expired)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Overall weighted score (0-100)
        /// </summary>
        public decimal OverallScore { get; set; }

        /// <summary>
        /// Technical questions score (0-100)
        /// </summary>
        public decimal TechnicalScore { get; set; }

        /// <summary>
        /// Soft skills score (0-100)
        /// </summary>
        public decimal SoftSkillsScore { get; set; }

        /// <summary>
        /// Total questions in assessment
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Number of correct answers
        /// </summary>
        public int CorrectAnswers { get; set; }

        /// <summary>
        /// Technical questions answered correctly
        /// </summary>
        public int TechnicalCorrect { get; set; }

        /// <summary>
        /// Total technical questions
        /// </summary>
        public int TechnicalTotal { get; set; }

        /// <summary>
        /// Soft skill questions answered correctly
        /// </summary>
        public int SoftSkillCorrect { get; set; }

        /// <summary>
        /// Total soft skill questions
        /// </summary>
        public int SoftSkillTotal { get; set; }

        /// <summary>
        /// When the assessment started
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// When the assessment was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Time taken in minutes
        /// </summary>
        public int TimeTakenMinutes { get; set; }

        /// <summary>
        /// When this score expires
        /// </summary>
        public DateTime? ScoreExpiresAt { get; set; }

        /// <summary>
        /// Job title at time of assessment
        /// </summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// Performance level based on score
        /// </summary>
        public string PerformanceLevel { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user passed (score >= 50%)
        /// </summary>
        public bool IsPassing { get; set; }

        /// <summary>
        /// Detailed breakdown by question (only in detailed result view)
        /// </summary>
        public List<QuestionResultDto>? QuestionResults { get; set; }
    }

    /// <summary>
    /// Individual question result for detailed breakdown
    /// </summary>
    public class QuestionResultDto
    {
        /// <summary>
        /// Question ID
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// The question text
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Category (Technical or SoftSkill)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Difficulty level
        /// </summary>
        public string Difficulty { get; set; } = string.Empty;

        /// <summary>
        /// The 4 answer options
        /// </summary>
        public List<string> Options { get; set; } = new();

        /// <summary>
        /// Index of the user's selected answer
        /// </summary>
        public int SelectedAnswerIndex { get; set; }

        /// <summary>
        /// Index of the correct answer
        /// </summary>
        public int CorrectAnswerIndex { get; set; }

        /// <summary>
        /// Whether the user's answer was correct
        /// </summary>
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Explanation for the correct answer
        /// </summary>
        public string? Explanation { get; set; }

        /// <summary>
        /// Time spent on this question in seconds
        /// </summary>
        public int TimeSpentSeconds { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // HISTORY
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Historical assessment attempt summary
    /// </summary>
    public class AssessmentHistoryItemDto
    {
        /// <summary>
        /// The assessment attempt ID
        /// </summary>
        public int AttemptId { get; set; }

        /// <summary>
        /// Status (Completed, Abandoned, Expired)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Overall score (null if not completed)
        /// </summary>
        public decimal? OverallScore { get; set; }

        /// <summary>
        /// Job title at time of assessment
        /// </summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// When the assessment started
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// When the assessment was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Which attempt number (1st, 2nd, etc.)
        /// </summary>
        public int RetakeNumber { get; set; }

        /// <summary>
        /// Whether this is the current active score
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether the score has expired
        /// </summary>
        public bool IsScoreExpired { get; set; }

        /// <summary>
        /// Performance level
        /// </summary>
        public string? PerformanceLevel { get; set; }
    }

    /// <summary>
    /// Assessment history list response
    /// </summary>
    public class AssessmentHistoryResponseDto
    {
        /// <summary>
        /// List of assessment attempts
        /// </summary>
        public List<AssessmentHistoryItemDto> Attempts { get; set; } = new();

        /// <summary>
        /// Total number of attempts
        /// </summary>
        public int TotalAttempts { get; set; }

        /// <summary>
        /// Best score achieved
        /// </summary>
        public decimal? BestScore { get; set; }

        /// <summary>
        /// Current active score (most recent completed, non-expired)
        /// </summary>
        public decimal? CurrentActiveScore { get; set; }
    }
}
