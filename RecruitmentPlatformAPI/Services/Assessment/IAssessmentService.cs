using RecruitmentPlatformAPI.DTOs.Assessment;

namespace RecruitmentPlatformAPI.Services.Assessment
{
    /// <summary>
    /// Service for managing job seeker skill assessments
    /// </summary>
    public interface IAssessmentService
    {
        /// <summary>
        /// Check if job seeker is eligible to start an assessment
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Eligibility status with details</returns>
        Task<EligibilityResponseDto> CheckEligibilityAsync(int userId);

        /// <summary>
        /// Start a new assessment attempt
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Assessment start details or null if not eligible</returns>
        Task<StartAssessmentResponseDto?> StartAssessmentAsync(int userId);

        /// <summary>
        /// Get current in-progress assessment status
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Current assessment status or null if none in progress</returns>
        Task<AssessmentStatusResponseDto?> GetCurrentStatusAsync(int userId);

        /// <summary>
        /// Get the next unanswered question
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Next question or null if assessment complete/not found</returns>
        Task<QuestionResponseDto?> GetNextQuestionAsync(int userId);

        /// <summary>
        /// Submit an answer for a question
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="dto">Answer submission details</param>
        /// <returns>Submission result or null if failed</returns>
        Task<SubmitAnswerResponseDto?> SubmitAnswerAsync(int userId, SubmitAnswerRequestDto dto);

        /// <summary>
        /// Complete the assessment and calculate scores
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Assessment result with scores or null if failed</returns>
        Task<AssessmentResultResponseDto?> CompleteAssessmentAsync(int userId);

        /// <summary>
        /// Abandon the current assessment
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if abandoned successfully</returns>
        Task<bool> AbandonAssessmentAsync(int userId);

        /// <summary>
        /// Get assessment history for a job seeker
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Assessment history with all attempts</returns>
        Task<AssessmentHistoryResponseDto> GetHistoryAsync(int userId);

        /// <summary>
        /// Get detailed result for a specific attempt
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="attemptId">The assessment attempt ID</param>
        /// <returns>Detailed result or null if not found/not owned</returns>
        Task<AssessmentResultResponseDto?> GetResultAsync(int userId, int attemptId);
    }
}
