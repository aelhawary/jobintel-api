using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public interface IJobSeekerService
    {
        Task<ProfileResponseDto> SavePersonalInfoAsync(int userId, PersonalInfoRequestDto dto);
        Task<PersonalInfoDto?> GetPersonalInfoAsync(int userId, string language = "en");
        Task<WizardStatusDto> GetWizardStatusAsync(int userId);
        Task<ProfileResponseDto> AdvanceWizardStepAsync(int userId, int targetStep);
        Task<List<JobTitleDto>> GetJobTitlesAsync();
    }
}
