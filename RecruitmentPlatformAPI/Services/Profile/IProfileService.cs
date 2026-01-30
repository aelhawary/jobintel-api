using RecruitmentPlatformAPI.DTOs.Profile;

namespace RecruitmentPlatformAPI.Services.Profile
{
    public interface IProfileService
    {
        Task<ProfileResponseDto> SavePersonalInfoAsync(int userId, PersonalInfoRequestDto dto);
        Task<PersonalInfoDto?> GetPersonalInfoAsync(int userId, string language = "en");
        Task<WizardStatusDto> GetWizardStatusAsync(int userId);
        Task<List<JobTitleDto>> GetJobTitlesAsync();
    }
}
