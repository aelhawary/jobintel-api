using RecruitmentPlatformAPI.DTOs.Recruiter;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    public interface IAiRecommendationService
    {
        /// <summary>
        /// Sends job and pre-filtered candidates to the AI API
        /// and returns ranked recommendations.
        /// Returns null if the AI API call fails.
        /// </summary>
        Task<AiRecommendationResponseDto?> GetRecommendationsAsync(
            AiRecommendationRequestDto request);
    }
}
