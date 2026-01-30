using Microsoft.AspNetCore.Http;
using RecruitmentPlatformAPI.DTOs.Profile;

namespace RecruitmentPlatformAPI.Services.Profile
{
    /// <summary>
    /// Service interface for managing resume/CV operations
    /// </summary>
    public interface IResumeService
    {
        /// <summary>
        /// Upload a new resume or replace existing one
        /// </summary>
        /// <param name="userId">The authenticated user's ID</param>
        /// <param name="file">The uploaded file</param>
        /// <returns>Resume response with file information</returns>
        Task<ResumeResponseDto> UploadResumeAsync(int userId, IFormFile file);

        /// <summary>
        /// Get the current user's resume information
        /// </summary>
        /// <param name="userId">The authenticated user's ID</param>
        /// <returns>Resume information or null if no resume exists</returns>
        Task<ResumeResponseDto> GetResumeAsync(int userId);

        /// <summary>
        /// Get the file stream for downloading the resume
        /// </summary>
        /// <param name="userId">The authenticated user's ID</param>
        /// <returns>Tuple of (fileStream, contentType, fileName) or null if not found</returns>
        Task<(Stream? FileStream, string ContentType, string FileName)?> GetResumeFileAsync(int userId);

        /// <summary>
        /// Delete the user's resume (soft delete)
        /// </summary>
        /// <param name="userId">The authenticated user's ID</param>
        /// <returns>Success/failure result</returns>
        Task<ResumeResponseDto> DeleteResumeAsync(int userId);

        /// <summary>
        /// Validate the uploaded file
        /// </summary>
        /// <param name="file">The uploaded file to validate</param>
        /// <returns>Validation result with error message if invalid</returns>
        FileValidationResult ValidateFile(IFormFile file);

        /// <summary>
        /// Check if the user has an active (non-deleted) resume
        /// </summary>
        /// <param name="userId">The authenticated user's ID</param>
        /// <returns>True if resume exists</returns>
        Task<bool> HasResumeAsync(int userId);
    }
}
