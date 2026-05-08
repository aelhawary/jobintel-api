using RecruitmentPlatformAPI.DTOs.Assessment;

namespace RecruitmentPlatformAPI.Services.Assessment
{
    /// <summary>
    /// Service contract for job-seeker skill assessments.
    /// Supports flexible navigation, draft-answer overwrite, overview panel,
    /// partial submission, auto-submit on expiry, and full review results.
    /// </summary>
    public interface IAssessmentService
    {
        /// <summary>Check whether the user is eligible to start an assessment.</summary>
        Task<EligibilityResponseDto> CheckEligibilityAsync(int userId);

        /// <summary>
        /// Start a new assessment attempt. Returns null when the user is not eligible
        /// or when a concurrent start is detected.
        /// </summary>
        Task<StartAssessmentResponseDto?> StartAssessmentAsync(int userId, StartAssessmentRequestDto? request = null);

        /// <summary>
        /// Get the current in-progress assessment status. Auto-submits and returns the
        /// completed status when the attempt has expired.
        /// </summary>
        Task<AssessmentStatusResponseDto?> GetCurrentStatusAsync(int userId);

        /// <summary>Return the answered/unanswered status for every question in the active attempt.</summary>
        Task<List<AssessmentQuestionStatusDto>?> GetQuestionStatusesAsync(int userId);

        /// <summary>Return the next unanswered question, or null when all questions are answered.</summary>
        Task<QuestionResponseDto?> GetNextQuestionAsync(int userId);

        /// <summary>Return the question at the given 1-based position. Supports non-linear navigation.</summary>
        Task<QuestionResponseDto?> GetQuestionByNumberAsync(int userId, int questionNumber);

        /// <summary>
        /// Record or overwrite an answer for a question.
        /// Overwriting an existing answer does not change the answered count.
        /// </summary>
        Task<SubmitAnswerResponseDto?> SubmitAnswerAsync(int userId, SubmitAnswerRequestDto dto);

        /// <summary>
        /// Finalise the assessment and compute skill-based scores.
        /// Unanswered questions count as incorrect.
        /// </summary>
        Task<AssessmentResultResponseDto?> CompleteAssessmentAsync(int userId);

        /// <summary>Abandon the current in-progress assessment.</summary>
        Task<bool> AbandonAssessmentAsync(int userId);

        /// <summary>Return the assessment history for the job seeker.</summary>
        Task<AssessmentHistoryResponseDto> GetHistoryAsync(int userId);

        /// <summary>Return the full review result for a completed attempt, including correct answers and explanations.</summary>
        Task<AssessmentResultResponseDto?> GetResultAsync(int userId, int attemptId);
    }
}
