using Microsoft.EntityFrameworkCore;
using System.Globalization;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.JobSeeker;
using RecruitmentPlatformAPI.Models.Reference;
using JobSeekerEntity = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public class JobSeekerService : IJobSeekerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<JobSeekerService> _logger;
        
        // Wizard step constants — Job Seeker (4 steps)
        private const int TotalWizardSteps = 4;
        private const int PersonalInfoStep = 1;       // Step 1: Personal Info + Picture + CV
        private const int ExperienceEducationStep = 2; // Step 2: Work Experience + Education
        private const int ProjectsStep = 3;            // Step 3: Projects
        private const int SkillsSocialCertsStep = 4;   // Step 4: Skills + Social Links + Certificates

        public JobSeekerService(AppDbContext context, ILogger<JobSeekerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProfileResponseDto> SavePersonalInfoAsync(int userId, PersonalInfoRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Starting SavePersonalInfoAsync for user {UserId}", userId);
                
                // Get the user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Only job seekers have profile wizard
                if (user.AccountType != AccountType.JobSeeker)
                {
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "Profile completion wizard is only available for job seekers"
                    };
                }



                // Validate duplicate languages
                if (dto.SecondLanguageId.HasValue && dto.SecondLanguageId.Value == dto.FirstLanguageId)
                {
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "Second language must be different from first language"
                    };
                }

                // Batch validate all foreign key references for efficiency
                var languageIds = new List<int> { dto.FirstLanguageId };
                if (dto.SecondLanguageId.HasValue)
                {
                    languageIds.Add(dto.SecondLanguageId.Value);
                }

                var jobTitleExists = await _context.JobTitles.AnyAsync(jt => jt.Id == dto.JobTitleId && jt.IsActive);
                var countryExists = await _context.Countries.AnyAsync(c => c.Id == dto.CountryId && c.IsActive);
                var validLanguagesCount = await _context.Languages.CountAsync(l => languageIds.Contains(l.Id) && l.IsActive);

                if (!jobTitleExists)
                {
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "Invalid job title. Please select from the provided list"
                    };
                }

                if (!countryExists)
                {
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "Invalid country. Please select from the provided list"
                    };
                }

                if (validLanguagesCount != languageIds.Count)
                {
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "Invalid language selection. Please select from the provided list"
                    };
                }

                // Get or create JobSeeker record
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    // Create JobSeeker record if it doesn't exist (for users registered before the fix)
                    _logger.LogInformation("JobSeeker record not found for user {UserId}, creating new one", userId);
                    jobSeeker = new JobSeekerEntity
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.JobSeekers.Add(jobSeeker);
                    await _context.SaveChangesAsync(); // Save to get the ID
                    _logger.LogInformation("Created JobSeeker record for user {UserId}", userId);
                }

                // Update job seeker fields
                jobSeeker.JobTitleId = dto.JobTitleId;
                jobSeeker.YearsOfExperience = dto.YearsOfExperience;
                jobSeeker.CountryId = dto.CountryId;
                jobSeeker.City = NormalizeCity(dto.City);
                jobSeeker.PhoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
                jobSeeker.FirstLanguageId = dto.FirstLanguageId;
                jobSeeker.FirstLanguageProficiency = dto.FirstLanguageProficiency;
                jobSeeker.SecondLanguageId = dto.SecondLanguageId;
                jobSeeker.SecondLanguageProficiency = dto.SecondLanguageProficiency;
                jobSeeker.Bio = dto.Bio?.Trim();
                jobSeeker.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updated JobSeeker fields for user {UserId}", userId);

                // Mark entity as modified
                _context.JobSeekers.Update(jobSeeker);

                // Update wizard progress
                if (user.ProfileCompletionStep < PersonalInfoStep)
                {
                    user.ProfileCompletionStep = PersonalInfoStep;
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Users.Update(user);
                    _logger.LogInformation("Updated ProfileCompletionStep to 1 for user {UserId}", userId);
                }

                _logger.LogInformation("Calling SaveChangesAsync for user {UserId}", userId);
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync completed. {Changes} changes saved for user {UserId}", changes, userId);

                return new ProfileResponseDto
                {
                    Success = true,
                    Message = "Personal information saved successfully",
                    ProfileCompletionStep = user.ProfileCompletionStep
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving personal information for user {UserId}", userId);
                return new ProfileResponseDto
                {
                    Success = false,
                    Message = "An error occurred while saving personal information. Please try again."
                };
            }
        }

        public async Task<PersonalInfoDto?> GetPersonalInfoAsync(int userId, string language = "en")
        {
            var jobSeeker = await _context.JobSeekers
                .AsNoTracking()
                .Include(js => js.JobTitle)
                .Include(js => js.Country)
                .Include(js => js.FirstLanguage)
                .Include(js => js.SecondLanguage)
                .FirstOrDefaultAsync(js => js.UserId == userId);

            if (jobSeeker == null)
            {
                return null;
            }

            // If no data has been saved yet, return null
            if (jobSeeker.JobTitleId == null)
            {
                return null;
            }

            var isArabic = language.ToLower() == "ar";

            return new PersonalInfoDto
            {
                JobTitleId = jobSeeker.JobTitleId.Value,
                JobTitle = jobSeeker.JobTitle?.Title, // JobTitle doesn't have localization yet
                YearsOfExperience = jobSeeker.YearsOfExperience ?? 0,
                CountryId = jobSeeker.CountryId ?? 0,
                Country = isArabic ? jobSeeker.Country?.NameAr : jobSeeker.Country?.NameEn,
                City = jobSeeker.City ?? string.Empty,
                PhoneNumber = jobSeeker.PhoneNumber,
                FirstLanguageId = jobSeeker.FirstLanguageId ?? 0,
                FirstLanguage = isArabic ? jobSeeker.FirstLanguage?.NameAr : jobSeeker.FirstLanguage?.NameEn,
                FirstLanguageProficiency = jobSeeker.FirstLanguageProficiency ?? LanguageProficiency.Beginner,
                SecondLanguageId = jobSeeker.SecondLanguageId,
                SecondLanguage = jobSeeker.SecondLanguage != null 
                    ? (isArabic ? jobSeeker.SecondLanguage.NameAr : jobSeeker.SecondLanguage.NameEn)
                    : null,
                SecondLanguageProficiency = jobSeeker.SecondLanguageProficiency,
                Bio = jobSeeker.Bio
            };
        }

        public async Task<ProfileResponseDto> AdvanceWizardStepAsync(int userId, int targetStep)
        {
            if (targetStep < 1 || targetStep > TotalWizardSteps)
            {
                return new ProfileResponseDto { Success = false, Message = $"Invalid step number. Must be between 1 and {TotalWizardSteps}." };
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ProfileResponseDto { Success = false, Message = "User not found" };
            }

            if (user.AccountType != AccountType.JobSeeker)
            {
                return new ProfileResponseDto
                {
                    Success = false,
                    Message = "Wizard advancement is only available for job seeker accounts"
                };
            }

            if (user.ProfileCompletionStep < targetStep)
            {
                user.ProfileCompletionStep = targetStep;
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Advanced wizard for user {UserId} to step {Step}", userId, targetStep);
            }

            return new ProfileResponseDto
            {
                Success = true,
                Message = "Wizard advanced successfully",
                ProfileCompletionStep = user.ProfileCompletionStep
            };
        }

        public async Task<WizardStatusDto> GetWizardStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.AccountType != AccountType.JobSeeker)
            {
                return new WizardStatusDto
                {
                    CurrentStep = 0,
                    IsComplete = false,
                    StepName = "Not Started",
                    CompletedSteps = Array.Empty<string>()
                };
            }

            var stepNames = new[]
            {
                "Not Started",                        // 0
                "Personal Info & CV",                  // 1
                "Experience & Education",              // 2
                "Projects",                            // 3
                "Skills, Social Links & Certificates"  // 4
            };

            var isComplete = user.ProfileCompletionStep >= TotalWizardSteps;
            var currentStepName = isComplete ? "Complete" : stepNames[user.ProfileCompletionStep];
            
            var completedSteps = new List<string>();
            for (int i = 1; i <= user.ProfileCompletionStep && i < stepNames.Length; i++)
            {
                completedSteps.Add(stepNames[i]);
            }

            return new WizardStatusDto
            {
                CurrentStep = user.ProfileCompletionStep,
                IsComplete = isComplete,
                StepName = currentStepName,
                CompletedSteps = completedSteps.ToArray()
            };
        }

        public async Task<List<JobTitleDto>> GetJobTitlesAsync()
        {
            return await _context.JobTitles
                .AsNoTracking()
                .Where(jt => jt.IsActive)
                .OrderBy(jt => jt.Category)
                .ThenBy(jt => jt.Title)
                .Select(jt => new JobTitleDto
                {
                    Id = jt.Id,
                    Title = jt.Title,
                    Category = jt.Category
                })
                .ToListAsync();
        }

        // Helper methods for data normalization
        private string NormalizeCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return city;

            // Trim and convert to Title Case
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(city.Trim().ToLower());
        }

        private string? NormalizePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            // Remove spaces, dashes, parentheses
            return phoneNumber
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();
        }
    }
}
