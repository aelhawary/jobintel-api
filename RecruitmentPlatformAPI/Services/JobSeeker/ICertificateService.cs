using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public interface ICertificateService
    {
        Task<CertificateListResponseDto> GetCertificatesAsync(int userId);
        Task<CertificateResponseDto?> GetCertificateByIdAsync(int userId, int certificateId);
        Task<CertificateResponseDto?> AddCertificateAsync(int userId, CertificateRequestDto dto, IFormFile? file);
        Task<CertificateResponseDto?> UpdateCertificateAsync(int userId, int certificateId, CertificateRequestDto dto, IFormFile? file);
        Task<bool> DeleteCertificateAsync(int userId, int certificateId);
        Task<(Stream? FileStream, string ContentType, string FileName)?> DownloadCertificateFileAsync(int userId, int certificateId);
    }
}
