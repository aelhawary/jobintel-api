using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Recruiter;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    public interface IRecruiterService
    {
        Task<ProfileResponseDto> SaveCompanyInfoAsync(int userId, RecruiterCompanyInfoRequestDto dto);
        Task<RecruiterCompanyInfoDto?> GetCompanyInfoAsync(int userId);
        Task<WizardStatusDto> GetWizardStatusAsync(int userId);
        Task<ProfileResponseDto> AdvanceWizardStepAsync(int userId, int targetStep);
        List<IndustryDto> GetIndustries();
        List<CompanySizeDto> GetCompanySizes();
    }
}
