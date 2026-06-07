using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Assessment.V2
{
    // ═══════════════════════════════════════════════════════════════════════════════
    // ELIGIBILITY
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Response for the V2 eligibility check — determines whether the user can start
    /// an LLM-powered assessment and surfaces the claimed-skills snapshot.
    /// </summary>
    public class EligibilityResponseDto
    {
        /// <summary>Whether the user is eligible to start an assessment.</summary>
        public bool IsEligible { get; set; }

        /// <summary>Human-readable reason when the user is not eligible.</summary>
        public string? Reason { get; set; }

        /// <summary>Whether the user has completed their profile (step 4).</summary>
        public bool HasCompletedProfile { get; set; }

        /// <summary>Whether the user has set their job title.</summary>
        public bool HasJobTitle { get; set; }

        /// <summary>Whether the user has selected at least one claimed skill.</summary>
        public bool HasClaimedSkills { get; set; }

        /// <summary>Number of claimed skills on the job-seeker profile.</summary>
        public int ClaimedSkillsCount { get; set; }

        /// <summary>Claimed skills that will be used as the source for question targeting.</summary>
        public List<AssessmentSkillLiteDto> ClaimedSkills { get; set; } = new();

        /// <summary>Whether the user already has an in-progress assessment.</summary>
        public bool HasInProgressAssessment { get; set; }

        /// <summary>Whether the user is within the cooldown period after a previous attempt.</summary>
        public bool IsInCooldownPeriod { get; set; }

        /// <summary>When the cooldown period ends (populated when in cooldown).</summary>
        public DateTime? CooldownEndsAt { get; set; }

        /// <summary>Days remaining until eligible again (populated when in cooldown).</summary>
        public int? DaysUntilEligible { get; set; }

        /// <summary>Number of previous assessment attempts.</summary>
        public int PreviousAttempts { get; set; }

        /// <summary>Current active assessment score, if one exists.</summary>
        public decimal? CurrentScore { get; set; }

        /// <summary>When the current active score expires.</summary>
        public DateTime? ScoreExpiresAt { get; set; }
    }

    /// <summary>Minimal skill projection used in assessment responses.</summary>
    public class AssessmentSkillLiteDto
    {
        /// <summary>The skill ID.</summary>
        public int SkillId { get; set; }

        /// <summary>The skill display name.</summary>
        public string SkillName { get; set; } = string.Empty;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // START ASSESSMENT
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Optional request body when starting a V2 assessment.
    /// When omitted the server snapshots skills from the job-seeker profile.
    /// </summary>
    public class StartAssessmentRequestDto
    {
        /// <summary>
        /// Optional explicit skill IDs supplied by the client.
        /// The server validates ownership and existence before using them.
        /// </summary>
        [MaxLength(50)]
        public List<int>? SkillIds { get; set; }
    }

    /// <summary>Response returned when a V2 assessment is successfully started.</summary>
    public class StartAssessmentResponseDto
    {
        /// <summary>ID of the created assessment attempt.</summary>
        public int AttemptId { get; set; }

        /// <summary>Total number of questions in this assessment.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>Number of technical questions.</summary>
        public int TechnicalQuestions { get; set; }

        /// <summary>Number of soft-skill questions.</summary>
        public int SoftSkillQuestions { get; set; }

        /// <summary>Time limit in minutes for the entire assessment.</summary>
        public int TimeLimitMinutes { get; set; }

        /// <summary>When the assessment started.</summary>
        public DateTime StartedAt { get; set; }

        /// <summary>When the assessment expires.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>Job title at the time of the assessment.</summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>Role family used for question targeting.</summary>
        public string RoleFamily { get; set; } = string.Empty;

        /// <summary>Seniority level derived from years of experience.</summary>
        public string SeniorityLevel { get; set; } = string.Empty;

        /// <summary>Which attempt number this is (1 = first, 2 = first retake, …).</summary>
        public int RetakeNumber { get; set; }

        /// <summary>Number of claimed skills snapshotted for this attempt.</summary>
        public int ClaimedSkillsCount { get; set; }

        /// <summary>Per-skill question distribution summary.</summary>
        public List<SkillAllocationDto> SkillAllocations { get; set; } = new();
    }

    /// <summary>Per-skill question distribution summary included in the start response.</summary>
    public class SkillAllocationDto
    {
        /// <summary>The skill ID.</summary>
        public int SkillId { get; set; }

        /// <summary>The skill display name.</summary>
        public string SkillName { get; set; } = string.Empty;

        /// <summary>Number of technical questions for this skill.</summary>
        public int TechnicalQuestions { get; set; }

        /// <summary>Number of soft-skill questions for this skill.</summary>
        public int SoftSkillQuestions { get; set; }

        /// <summary>Total questions for this skill (technical + soft-skill).</summary>
        public int TotalQuestions => TechnicalQuestions + SoftSkillQuestions;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // CURRENT STATUS
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>Current V2 assessment attempt status and progress.</summary>
    public class AssessmentStatusResponseDto
    {
        /// <summary>The assessment attempt ID.</summary>
        public int AttemptId { get; set; }

        /// <summary>Current status string (InProgress, Completed, Abandoned, Expired).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Total questions in the assessment.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>Number of questions answered so far.</summary>
        public int QuestionsAnswered { get; set; }

        /// <summary>Number of questions still unanswered.</summary>
        public int QuestionsRemaining { get; set; }

        /// <summary>When the assessment started.</summary>
        public DateTime StartedAt { get; set; }

        /// <summary>When the assessment expires.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>Seconds remaining until the assessment expires.</summary>
        public int TimeRemainingSeconds { get; set; }

        /// <summary>Completion percentage (0–100).</summary>
        public decimal ProgressPercentage { get; set; }

        /// <summary>Whether the assessment has already expired.</summary>
        public bool IsExpired { get; set; }

        /// <summary>Whether the assessment was automatically submitted due to rules (like anti-cheat).</summary>
        public bool IsAutoSubmitted { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // QUESTION OVERVIEW
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>Lightweight status entry for the question-navigation overview panel.</summary>
    public class AssessmentQuestionStatusDto
    {
        /// <summary>1-based question number within the attempt.</summary>
        public int QuestionNumber { get; set; }

        /// <summary>Whether a saved answer exists for this question.</summary>
        public bool IsAnswered { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // QUESTION
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>A single question presented to the user during a V2 assessment.</summary>
    public class QuestionResponseDto
    {
        /// <summary>The question ID.</summary>
        public int QuestionId { get; set; }

        /// <summary>1-based position within the attempt.</summary>
        public int QuestionNumber { get; set; }

        /// <summary>Total questions in the assessment.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>The question text.</summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>Category — Technical or SoftSkill.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Difficulty — Easy, Medium, or Hard.</summary>
        public string Difficulty { get; set; } = string.Empty;

        /// <summary>The four answer options.</summary>
        public List<string> Options { get; set; } = new();

        /// <summary>Previously selected answer index, or null if not yet answered.</summary>
        public int? SelectedAnswerIndex { get; set; }

        /// <summary>Time allowed for this question in seconds.</summary>
        public int TimeAllowedSeconds { get; set; }

        /// <summary>Seconds remaining in the overall assessment.</summary>
        public int TimeRemainingInAssessmentSeconds { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // SUBMIT ANSWER
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Request body for submitting or overwriting an answer.
    /// Submitting to an already-answered question overwrites the saved answer without
    /// incrementing the answered count.
    /// </summary>
    public class SubmitAnswerRequestDto
    {
        /// <summary>The question ID being answered.</summary>
        [Required(ErrorMessage = "Question ID is required")]
        public int QuestionId { get; set; }

        /// <summary>Selected answer index (0–3).</summary>
        [Required(ErrorMessage = "Answer selection is required")]
        [Range(0, 3, ErrorMessage = "Answer index must be between 0 and 3")]
        public int SelectedAnswerIndex { get; set; }

        /// <summary>Time spent on this question in seconds (used for analytics).</summary>
        [Range(0, 3600, ErrorMessage = "Time spent must be between 0 and 3600 seconds")]
        public int TimeSpentSeconds { get; set; }
    }

    /// <summary>
    /// Response after recording an answer.
    /// No correctness information is exposed while the assessment is in progress.
    /// </summary>
    public class SubmitAnswerResponseDto
    {
        /// <summary>Whether the answer was recorded successfully.</summary>
        public bool Success { get; set; }

        /// <summary>Total questions with a saved answer (overwrite does not increase this).</summary>
        public int QuestionsAnswered { get; set; }

        /// <summary>Questions still without a saved answer.</summary>
        public int QuestionsRemaining { get; set; }

        /// <summary>Whether all questions now have a saved answer.</summary>
        public bool IsAssessmentComplete { get; set; }

        /// <summary>Seconds remaining in the assessment.</summary>
        public int TimeRemainingSeconds { get; set; }

        /// <summary>Completion percentage (0–100).</summary>
        public decimal ProgressPercentage { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // COMPLETION / RESULT
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// V2 assessment completion result with skill-based scoring.
    /// Returned by the complete endpoint and by the per-attempt result lookup.
    /// Unanswered questions count as incorrect when computing scores.
    /// </summary>
    public class AssessmentResultResponseDto
    {
        /// <summary>The assessment attempt ID.</summary>
        public int AttemptId { get; set; }

        /// <summary>Final status string (Completed, Abandoned).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Overall score across all questions (0–100).</summary>
        public decimal OverallScore { get; set; }

        /// <summary>Summary statistics: answered, correct, wrong, skipped.</summary>
        public AssessmentStatsDto? Stats { get; set; }

        /// <summary>Score across technical questions only (0–100).</summary>
        public decimal TechnicalScore { get; set; }

        /// <summary>Score across soft-skill questions only (0–100).</summary>
        public decimal SoftSkillsScore { get; set; }

        /// <summary>Total questions in the assessment.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>Total correct answers.</summary>
        public int CorrectAnswers { get; set; }

        /// <summary>Technical questions answered correctly.</summary>
        public int TechnicalCorrect { get; set; }

        /// <summary>Total technical questions.</summary>
        public int TechnicalTotal { get; set; }

        /// <summary>Soft-skill questions answered correctly.</summary>
        public int SoftSkillCorrect { get; set; }

        /// <summary>Total soft-skill questions.</summary>
        public int SoftSkillTotal { get; set; }

        /// <summary>When the assessment started.</summary>
        public DateTime StartedAt { get; set; }

        /// <summary>When the assessment was completed.</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Time taken in minutes.</summary>
        public int TimeTakenMinutes { get; set; }

        /// <summary>When this score expires.</summary>
        public DateTime? ScoreExpiresAt { get; set; }

        /// <summary>Job title at time of assessment.</summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>Performance band derived from the overall score (Novice → Expert).</summary>
        public string PerformanceLevel { get; set; } = string.Empty;

        /// <summary>Whether the score meets the minimum passing threshold.</summary>
        public bool IsPassing { get; set; }

        /// <summary>Per-skill verification breakdown.</summary>
        public List<SkillScoreDto> SkillScores { get; set; } = new();

        /// <summary>
        /// Full per-question breakdown including correct answers and explanations.
        /// Populated by the result-lookup endpoint; null on the complete endpoint.
        /// </summary>
        public List<QuestionResultDto>? QuestionResults { get; set; }
    }

    /// <summary>Summary statistics for a completed assessment attempt.</summary>
    public class AssessmentStatsDto
    {
        /// <summary>Total questions in the assessment.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>Number of questions that received an answer.</summary>
        public int AnsweredQuestions { get; set; }

        /// <summary>Number of correct answers.</summary>
        public int CorrectAnswers { get; set; }

        /// <summary>Number of wrong answers.</summary>
        public int WrongAnswers { get; set; }

        /// <summary>Number of unanswered questions (counted as incorrect).</summary>
        public int SkippedQuestions { get; set; }
    }

    /// <summary>Per-skill score details included in the assessment result.</summary>
    public class SkillScoreDto
    {
        /// <summary>The skill ID.</summary>
        public int SkillId { get; set; }

        /// <summary>The skill display name.</summary>
        public string SkillName { get; set; } = string.Empty;

        /// <summary>Technical or SoftSkill.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Number of correct answers for this skill.</summary>
        public int CorrectAnswers { get; set; }

        /// <summary>Total questions for this skill.</summary>
        public int TotalQuestions { get; set; }

        /// <summary>Score percentage for this skill (0–100).</summary>
        public decimal Score { get; set; }

        /// <summary>Whether this skill was explicitly claimed by the user at assessment start.</summary>
        public bool IsClaimedSkill { get; set; }
    }

    /// <summary>
    /// Question-level result included in the full review response.
    /// Includes the correct answer index, optional explanation, and skill attribution.
    /// </summary>
    public class QuestionResultDto
    {
        /// <summary>The question ID.</summary>
        public int QuestionId { get; set; }

        /// <summary>The question text.</summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>Technical or SoftSkill.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Difficulty — Easy, Medium, or Hard.</summary>
        public string Difficulty { get; set; } = string.Empty;

        /// <summary>The four answer options.</summary>
        public List<string> Options { get; set; } = new();

        /// <summary>The user's selected answer index, or null if the question was left unanswered.</summary>
        public int? SelectedAnswerIndex { get; set; }

        /// <summary>The correct answer index (0–3).</summary>
        public int CorrectAnswerIndex { get; set; }

        /// <summary>Whether the user answered correctly.</summary>
        public bool IsCorrect { get; set; }

        /// <summary>Explanation of why the correct answer is correct.</summary>
        public string? Explanation { get; set; }

        /// <summary>Time spent on this question in seconds.</summary>
        public int TimeSpentSeconds { get; set; }

        /// <summary>Skill this question is attributed to.</summary>
        public int SkillId { get; set; }

        /// <summary>Skill display name.</summary>
        public string SkillName { get; set; } = string.Empty;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // HISTORY
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>Summary of a single historical V2 assessment attempt.</summary>
    public class AssessmentHistoryItemDto
    {
        /// <summary>The assessment attempt ID.</summary>
        public int AttemptId { get; set; }

        /// <summary>Status string (Completed, Abandoned, Expired).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Overall score (0–100), null if not yet scored.</summary>
        public decimal? OverallScore { get; set; }

        /// <summary>Job title at time of assessment.</summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>When the assessment started.</summary>
        public DateTime StartedAt { get; set; }

        /// <summary>When the assessment was completed.</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Which attempt number this is.</summary>
        public int RetakeNumber { get; set; }

        /// <summary>Whether this is the current active scored attempt.</summary>
        public bool IsActive { get; set; }

        /// <summary>Whether the score has expired (beyond validity period).</summary>
        public bool IsScoreExpired { get; set; }

        /// <summary>Performance band derived from the overall score.</summary>
        public string? PerformanceLevel { get; set; }
    }

    /// <summary>V2 assessment history list response.</summary>
    public class AssessmentHistoryResponseDto
    {
        /// <summary>All historical assessment attempts, newest first.</summary>
        public List<AssessmentHistoryItemDto> Attempts { get; set; } = new();

        /// <summary>Total number of assessment attempts.</summary>
        public int TotalAttempts { get; set; }

        /// <summary>Best score achieved across all completed attempts.</summary>
        public decimal? BestScore { get; set; }

        /// <summary>Currently active (non-expired) score.</summary>
        public decimal? CurrentActiveScore { get; set; }
    }
}
