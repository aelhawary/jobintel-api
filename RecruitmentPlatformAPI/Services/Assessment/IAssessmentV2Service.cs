using RecruitmentPlatformAPI.DTOs.Assessment;

namespace RecruitmentPlatformAPI.Services.Assessment
{
    /// <summary>
    /// Service for skill-based assessment v2 (claimed-skills validation mode).
    /// </summary>
    public interface IAssessmentV2Service
    {
        Task<EligibilityV2ResponseDto> CheckEligibilityAsync(int userId);

        Task<StartAssessmentV2ResponseDto?> StartAssessmentAsync(int userId, StartAssessmentV2RequestDto? request = null);

        Task<AssessmentStatusResponseDto?> GetCurrentStatusAsync(int userId);

        Task<QuestionResponseDto?> GetNextQuestionAsync(int userId);

        Task<SubmitAnswerResponseDto?> SubmitAnswerAsync(int userId, SubmitAnswerRequestDto dto);

        Task<AssessmentResultV2ResponseDto?> CompleteAssessmentAsync(int userId);

        Task<bool> AbandonAssessmentAsync(int userId);

        Task<AssessmentHistoryResponseDto> GetHistoryAsync(int userId);

        Task<AssessmentResultV2ResponseDto?> GetResultAsync(int userId, int attemptId);
    }
}
