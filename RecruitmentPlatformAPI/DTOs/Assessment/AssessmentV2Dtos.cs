using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Assessment
{
    /// <summary>
    /// Extended eligibility response for skill-based assessment v2.
    /// </summary>
    public class EligibilityV2ResponseDto : EligibilityResponseDto
    {
        /// <summary>
        /// Whether the user has selected at least one claimed skill.
        /// </summary>
        public bool HasClaimedSkills { get; set; }

        /// <summary>
        /// Number of claimed skills selected by the job seeker.
        /// </summary>
        public int ClaimedSkillsCount { get; set; }

        /// <summary>
        /// Claimed skills used as the source for question targeting.
        /// </summary>
        public List<AssessmentSkillLiteDto> ClaimedSkills { get; set; } = new();
    }

    /// <summary>
    /// Extended start response for skill-based assessment v2.
    /// </summary>
    public class StartAssessmentV2ResponseDto : StartAssessmentResponseDto
    {
        /// <summary>
        /// Number of claimed skills considered for this attempt.
        /// </summary>
        public int ClaimedSkillsCount { get; set; }

        /// <summary>
        /// Question allocation summary per skill.
        /// </summary>
        public List<SkillAllocationDto> SkillAllocations { get; set; } = new();
    }

    /// <summary>
    /// Minimal skill projection used in v2 responses.
    /// </summary>
    public class AssessmentSkillLiteDto
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Per-skill question distribution summary.
    /// </summary>
    public class SkillAllocationDto
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; } = string.Empty;

        public int TechnicalQuestions { get; set; }

        public int SoftSkillQuestions { get; set; }

        public int TotalQuestions => TechnicalQuestions + SoftSkillQuestions;
    }

    /// <summary>
    /// Skill-based assessment completion result (v2).
    /// </summary>
    public class AssessmentResultV2ResponseDto
    {
        public int AttemptId { get; set; }

        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Overall score across all answered questions.
        /// </summary>
        public decimal OverallScore { get; set; }

        /// <summary>
        /// Technical score across all technical questions.
        /// </summary>
        public decimal TechnicalSkillsTotalScore { get; set; }

        /// <summary>
        /// Soft-skills score across all soft-skill questions.
        /// </summary>
        public decimal SoftSkillsScore { get; set; }

        public int TotalQuestions { get; set; }

        public int CorrectAnswers { get; set; }

        public int TechnicalCorrect { get; set; }

        public int TechnicalTotal { get; set; }

        public int SoftSkillCorrect { get; set; }

        public int SoftSkillTotal { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int TimeTakenMinutes { get; set; }

        public DateTime? ScoreExpiresAt { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string PerformanceLevel { get; set; } = string.Empty;

        public bool IsPassing { get; set; }

        /// <summary>
        /// Per-skill verification breakdown.
        /// </summary>
        public List<SkillScoreDto> SkillScores { get; set; } = new();

        /// <summary>
        /// Detailed per-question breakdown.
        /// </summary>
        public List<QuestionResultV2Dto>? QuestionResults { get; set; }
    }

    /// <summary>
    /// Per-skill score details for v2 results.
    /// </summary>
    public class SkillScoreDto
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; } = string.Empty;

        /// <summary>
        /// Technical or SoftSkill.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        public int CorrectAnswers { get; set; }

        public int TotalQuestions { get; set; }

        public decimal Score { get; set; }

        /// <summary>
        /// Indicates whether this skill was explicitly claimed by the user at assessment start.
        /// </summary>
        public bool IsClaimedSkill { get; set; }
    }

    /// <summary>
    /// Question-level result with skill attribution for v2.
    /// </summary>
    public class QuestionResultV2Dto
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Difficulty { get; set; } = string.Empty;

        public List<string> Options { get; set; } = new();

        public int SelectedAnswerIndex { get; set; }

        public int CorrectAnswerIndex { get; set; }

        public bool IsCorrect { get; set; }

        public string? Explanation { get; set; }

        public int TimeSpentSeconds { get; set; }

        public int SkillId { get; set; }

        public string SkillName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optional request to start v2 with explicit client-side claim snapshot override.
    /// If omitted, server snapshot from profile skills is used.
    /// </summary>
    public class StartAssessmentV2RequestDto
    {
        /// <summary>
        /// Optional explicit skill IDs from client. Server validates ownership and existence.
        /// </summary>
        [MaxLength(50)]
        public List<int>? SkillIds { get; set; }
    }
}
