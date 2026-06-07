using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public interface ICvParserService
    {
        Task<ParsedResumeDataDto?> ParseResumeTextAsync(string text);
    }
}
