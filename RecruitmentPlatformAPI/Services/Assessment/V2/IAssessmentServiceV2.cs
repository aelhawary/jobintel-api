using System.Collections.Generic;
using System.Threading.Tasks;
using RecruitmentPlatformAPI.DTOs.Assessment.V2;

namespace RecruitmentPlatformAPI.Services.Assessment.V2
{
    public interface IAssessmentServiceV2
    {
        Task<EligibilityResponseDto> CheckEligibilityAsync(int userId);
        Task<StartAssessmentResponseDto?> StartAssessmentAsync(int userId, StartAssessmentRequestDto? request = null);
        Task<AssessmentStatusResponseDto?> ResumeAssessmentAsync(int userId);
        Task<AssessmentStatusResponseDto?> GetCurrentStatusAsync(int userId);
        Task<List<AssessmentQuestionStatusDto>?> GetQuestionStatusesAsync(int userId);
        Task<QuestionResponseDto?> GetQuestionByNumberAsync(int userId, int questionNumber);
        Task<QuestionResponseDto?> GetNextQuestionAsync(int userId);
        Task<SubmitAnswerResponseDto?> SubmitAnswerAsync(int userId, SubmitAnswerRequestDto dto);
        Task<AssessmentResultResponseDto?> CompleteAssessmentAsync(int userId);
        Task<bool> AbandonAssessmentAsync(int userId);
        Task<AssessmentHistoryResponseDto> GetHistoryAsync(int userId);
        Task<AssessmentResultResponseDto?> GetResultAsync(int userId, int attemptId);
    }
}
