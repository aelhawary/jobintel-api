using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.Profile
{
    public class SocialAccountService : ISocialAccountService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SocialAccountService> _logger;

        // Wizard step constant
        private const int SocialLinksStep = 6;

        public SocialAccountService(AppDbContext context, ILogger<SocialAccountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SocialAccountResponseDto> UpdateSocialAccountAsync(int userId, UpdateSocialAccountDto dto)
        {
            try
            {
                _logger.LogInformation("Updating social accounts for user {UserId}", userId);

                // Get user and verify they're a job seeker
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return new SocialAccountResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (user.AccountType != AccountType.JobSeeker)
                {
                    _logger.LogWarning("User {UserId} is not a job seeker", userId);
                    return new SocialAccountResponseDto
                    {
                        Success = false,
                        Message = "Only job seekers can manage social accounts"
                    };
                }

                // Get or create JobSeeker record (backward compatibility)
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogInformation("Creating JobSeeker record for user {UserId}", userId);
                    jobSeeker = new JobSeeker
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.JobSeekers.Add(jobSeeker);
                    await _context.SaveChangesAsync();
                }

                // Check if at least one link is provided (any non-null, non-empty value)
                bool hasAnyLink = !string.IsNullOrWhiteSpace(dto.LinkedIn) ||
                                 !string.IsNullOrWhiteSpace(dto.Github) ||
                                 !string.IsNullOrWhiteSpace(dto.Behance) ||
                                 !string.IsNullOrWhiteSpace(dto.Dribbble) ||
                                 !string.IsNullOrWhiteSpace(dto.PersonalWebsite);

                // Get existing social account
                var socialAccount = await _context.SocialAccounts
                    .FirstOrDefaultAsync(sa => sa.JobSeekerId == jobSeeker.Id);

                if (socialAccount == null)
                {
                    // If no links provided and no existing record, just return success (user skipped step)
                    if (!hasAnyLink)
                    {
                        _logger.LogInformation("User {UserId} skipped social links (Step 6) - no links provided", userId);
                        return new SocialAccountResponseDto
                        {
                            Success = true,
                            Message = "No social links added",
                            SocialAccounts = null
                        };
                    }

                    // Create new social account record
                    _logger.LogInformation("Creating new social account record for JobSeeker {JobSeekerId}", jobSeeker.Id);
                    socialAccount = new SocialAccount
                    {
                        JobSeekerId = jobSeeker.Id,
                        LinkedIn = dto.LinkedIn?.Trim(),
                        Github = dto.Github?.Trim(),
                        Behance = dto.Behance?.Trim(),
                        Dribbble = dto.Dribbble?.Trim(),
                        PersonalWebsite = dto.PersonalWebsite?.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.SocialAccounts.Add(socialAccount);
                }
                else
                {
                    // Update existing record (allows clearing individual links by setting to null/empty)
                    _logger.LogInformation("Updating existing social account for JobSeeker {JobSeekerId}", jobSeeker.Id);
                    socialAccount.LinkedIn = dto.LinkedIn?.Trim();
                    socialAccount.Github = dto.Github?.Trim();
                    socialAccount.Behance = dto.Behance?.Trim();
                    socialAccount.Dribbble = dto.Dribbble?.Trim();
                    socialAccount.PersonalWebsite = dto.PersonalWebsite?.Trim();
                    socialAccount.UpdatedAt = DateTime.UtcNow;
                }

                // Advance wizard step to 6 if user has at least one link and is at step < 6
                if (hasAnyLink && user.ProfileCompletionStep < SocialLinksStep)
                {
                    _logger.LogInformation("Advancing user {UserId} to wizard step {Step}", userId, SocialLinksStep);
                    user.ProfileCompletionStep = SocialLinksStep;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Social accounts updated successfully for user {UserId}", userId);

                return new SocialAccountResponseDto
                {
                    Success = true,
                    Message = "Social accounts updated successfully",
                    SocialAccounts = new SocialAccountDto
                    {
                        LinkedIn = socialAccount.LinkedIn,
                        Github = socialAccount.Github,
                        Behance = socialAccount.Behance,
                        Dribbble = socialAccount.Dribbble,
                        PersonalWebsite = socialAccount.PersonalWebsite,
                        CreatedAt = socialAccount.CreatedAt,
                        UpdatedAt = socialAccount.UpdatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating social accounts for user {UserId}", userId);
                return new SocialAccountResponseDto
                {
                    Success = false,
                    Message = "An error occurred while updating social accounts"
                };
            }
        }

        public async Task<SocialAccountResponseDto> GetSocialAccountAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting social accounts for user {UserId}", userId);

                // Get user and verify they're a job seeker
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return new SocialAccountResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (user.AccountType != AccountType.JobSeeker)
                {
                    _logger.LogWarning("User {UserId} is not a job seeker", userId);
                    return new SocialAccountResponseDto
                    {
                        Success = false,
                        Message = "Only job seekers can view social accounts"
                    };
                }

                // Get JobSeeker record
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogInformation("JobSeeker record not found for user {UserId}", userId);
                    return new SocialAccountResponseDto
                    {
                        Success = true,
                        SocialAccounts = null
                    };
                }

                // Get social account
                var socialAccount = await _context.SocialAccounts
                    .FirstOrDefaultAsync(sa => sa.JobSeekerId == jobSeeker.Id);

                if (socialAccount == null)
                {
                    _logger.LogInformation("No social accounts found for user {UserId}", userId);
                    return new SocialAccountResponseDto
                    {
                        Success = true,
                        SocialAccounts = null
                    };
                }

                return new SocialAccountResponseDto
                {
                    Success = true,
                    SocialAccounts = new SocialAccountDto
                    {
                        LinkedIn = socialAccount.LinkedIn,
                        Github = socialAccount.Github,
                        Behance = socialAccount.Behance,
                        Dribbble = socialAccount.Dribbble,
                        PersonalWebsite = socialAccount.PersonalWebsite,
                        CreatedAt = socialAccount.CreatedAt,
                        UpdatedAt = socialAccount.UpdatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting social accounts for user {UserId}", userId);
                return new SocialAccountResponseDto
                {
                    Success = false,
                    Message = "An error occurred while retrieving social accounts"
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteSocialAccountAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Deleting social accounts for user {UserId}", userId);

                // Get user and verify they're a job seeker
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (user.AccountType != AccountType.JobSeeker)
                {
                    _logger.LogWarning("User {UserId} is not a job seeker", userId);
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Only job seekers can delete social accounts"
                    };
                }

                // Get JobSeeker record
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogInformation("JobSeeker record not found for user {UserId}", userId);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No social accounts to delete"
                    };
                }

                // Find and delete social account
                var socialAccount = await _context.SocialAccounts
                    .FirstOrDefaultAsync(sa => sa.JobSeekerId == jobSeeker.Id);

                if (socialAccount == null)
                {
                    _logger.LogInformation("No social accounts found for user {UserId}", userId);
                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No social accounts to delete"
                    };
                }

                _context.SocialAccounts.Remove(socialAccount);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Social accounts deleted successfully for user {UserId}", userId);

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Social accounts deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting social accounts for user {UserId}", userId);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting social accounts"
                };
            }
        }
    }
}
