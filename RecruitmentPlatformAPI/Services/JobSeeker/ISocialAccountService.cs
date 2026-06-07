using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public interface ISocialAccountService
    {
        /// <summary>
        /// Create or update social account links for a job seeker
        /// All fields are optional - users can provide any combination of links
        /// </summary>
        Task<SocialAccountResponseDto> UpdateSocialAccountAsync(int userId, UpdateSocialAccountDto dto);

        /// <summary>
        /// Get social account information for a job seeker
        /// Returns null if no social account exists
        /// </summary>
        Task<SocialAccountResponseDto> GetSocialAccountAsync(int userId);

        /// <summary>
        /// Delete all social account links for a job seeker
        /// </summary>
        Task<ApiResponse<object>> DeleteSocialAccountAsync(int userId);
    }
}
