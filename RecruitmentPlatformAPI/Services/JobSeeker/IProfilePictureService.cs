using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    /// <summary>
    /// Service interface for managing user profile pictures
    /// </summary>
    public interface IProfilePictureService
    {
        /// <summary>
        /// Uploads a profile picture for the user
        /// Replaces existing picture if one exists
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="fileStream">The file stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="contentType">MIME type</param>
        /// <returns>Upload result with URL</returns>
        Task<ProfilePictureUploadResultDto> UploadProfilePictureAsync(int userId, Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Gets the profile picture information for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Profile picture response with URL and metadata</returns>
        Task<ProfilePictureResponseDto> GetProfilePictureAsync(int userId);

        /// <summary>
        /// Gets the profile picture file for download/display
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Tuple of (stream, contentType, fileName) or null if not found</returns>
        Task<(Stream? stream, string? contentType, string? fileName)?> GetProfilePictureFileAsync(int userId);

        /// <summary>
        /// Deletes the profile picture for a user
        /// Does not affect OAuth profile pictures (those will remain in database but file is deleted)
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteProfilePictureAsync(int userId);

        /// <summary>
        /// Checks if a user has an uploaded profile picture (not OAuth)
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if user has an uploaded picture</returns>
        Task<bool> HasUploadedProfilePictureAsync(int userId);

        /// <summary>
        /// Validates an image file before upload
        /// </summary>
        /// <param name="fileStream">The file stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="contentType">MIME type</param>
        /// <param name="fileSize">File size in bytes</param>
        /// <returns>Validation result</returns>
        Task<ImageValidationResult> ValidateImageFileAsync(Stream fileStream, string fileName, string contentType, long fileSize);
    }
}
