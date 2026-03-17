using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Recruiter;
using RecruiterEntity = RecruitmentPlatformAPI.Models.Recruiter.Recruiter;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    public class RecruiterService : IRecruiterService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecruiterService> _logger;

        // Wizard step constants — Recruiter (1 step)
        private const int TotalRecruiterSteps = 1;
        private const int CompanyInfoStep = 1;

        // ─── Static Data ─────────────────────────────────────
        private static readonly List<IndustryDto> Industries = new()
        {
            new() { Name = "Technology" },
            new() { Name = "Finance & Banking" },
            new() { Name = "Healthcare" },
            new() { Name = "Education" },
            new() { Name = "Manufacturing" },
            new() { Name = "Retail" },
            new() { Name = "Real Estate" },
            new() { Name = "Telecommunications" },
            new() { Name = "Energy" },
            new() { Name = "Transportation & Logistics" },
            new() { Name = "Media & Entertainment" },
            new() { Name = "Consulting" },
            new() { Name = "Legal" },
            new() { Name = "Government" },
            new() { Name = "Non-Profit" },
            new() { Name = "Hospitality & Tourism" },
            new() { Name = "Agriculture" },
            new() { Name = "Construction" },
            new() { Name = "Automotive" },
            new() { Name = "Pharmaceutical" },
        };

        private static readonly List<CompanySizeDto> CompanySizes = new()
        {
            new() { Value = "1-10",      Label = "1-10 employees" },
            new() { Value = "11-50",     Label = "11-50 employees" },
            new() { Value = "51-200",    Label = "51-200 employees" },
            new() { Value = "201-500",   Label = "201-500 employees" },
            new() { Value = "501-1000",  Label = "501-1000 employees" },
            new() { Value = "1000+",     Label = "1000+ employees" },
        };

        private static readonly HashSet<string> ValidIndustries =
            new(Industries.Select(i => i.Name), StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> ValidCompanySizes =
            new(CompanySizes.Select(s => s.Value));

        public RecruiterService(AppDbContext context, ILogger<RecruiterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProfileResponseDto> SaveCompanyInfoAsync(int userId, RecruiterCompanyInfoRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Starting SaveCompanyInfoAsync for user {UserId}", userId);

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ProfileResponseDto { Success = false, Message = "User not found" };
                }

                if (user.AccountType != AccountType.Recruiter)
                {
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "Company information is only available for recruiter accounts"
                    };
                }

                // Validate industry against predefined list
                if (!ValidIndustries.Contains(dto.Industry))
                {
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "Invalid industry. Please select from the provided list (GET /api/recruiter/industries)"
                    };
                }

                // Validate company size against allowed values
                if (!ValidCompanySizes.Contains(dto.CompanySize))
                {
                    return new ProfileResponseDto
                    {
                        Success = false,
                        Message = "Invalid company size. Allowed values: 1-10, 11-50, 51-200, 201-500, 501-1000, 1000+"
                    };
                }

                // Get or create Recruiter record
                var recruiter = await _context.Recruiters
                    .FirstOrDefaultAsync(r => r.UserId == userId);

                if (recruiter == null)
                {
                    _logger.LogInformation("Recruiter record not found for user {UserId}, creating new one", userId);
                    recruiter = new RecruiterEntity
                    {
                        UserId = userId,
                        CompanyName = dto.CompanyName.Trim(),
                        CompanySize = dto.CompanySize,
                        Industry = dto.Industry,
                        Location = dto.Location.Trim(),
                        Website = NormalizeUrl(dto.Website),
                        LinkedIn = NormalizeUrl(dto.LinkedIn),
                        CompanyDescription = dto.CompanyDescription?.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Recruiters.Add(recruiter);
                }
                else
                {
                    recruiter.CompanyName = dto.CompanyName.Trim();
                    recruiter.CompanySize = dto.CompanySize;
                    recruiter.Industry = dto.Industry;
                    recruiter.Location = dto.Location.Trim();
                    recruiter.Website = NormalizeUrl(dto.Website);
                    recruiter.LinkedIn = NormalizeUrl(dto.LinkedIn);
                    recruiter.CompanyDescription = dto.CompanyDescription?.Trim();
                    recruiter.UpdatedAt = DateTime.UtcNow;
                    _context.Recruiters.Update(recruiter);
                }

                // Update wizard progress (single step for recruiters)
                if (user.ProfileCompletionStep < CompanyInfoStep)
                {
                    user.ProfileCompletionStep = CompanyInfoStep;
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Users.Update(user);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Company info saved for user {UserId}", userId);

                return new ProfileResponseDto
                {
                    Success = true,
                    Message = "Company information saved successfully",
                    ProfileCompletionStep = user.ProfileCompletionStep
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving company info for user {UserId}", userId);
                return new ProfileResponseDto
                {
                    Success = false,
                    Message = "An error occurred while saving company information. Please try again."
                };
            }
        }

        public async Task<RecruiterCompanyInfoDto?> GetCompanyInfoAsync(int userId)
        {
            var recruiter = await _context.Recruiters
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter == null)
            {
                return null;
            }

            // Don't return placeholder data from registration
            if (recruiter.CompanyName == "Not Specified")
            {
                return null;
            }

            return new RecruiterCompanyInfoDto
            {
                CompanyName = recruiter.CompanyName,
                CompanySize = recruiter.CompanySize,
                Industry = recruiter.Industry,
                Location = recruiter.Location,
                Website = recruiter.Website,
                LinkedIn = recruiter.LinkedIn,
                CompanyDescription = recruiter.CompanyDescription,
                LogoUrl = recruiter.LogoUrl,
                CreatedAt = recruiter.CreatedAt,
                UpdatedAt = recruiter.UpdatedAt
            };
        }

        public async Task<ProfileResponseDto> AdvanceWizardStepAsync(int userId, int targetStep)
        {
            if (targetStep < 1 || targetStep > TotalRecruiterSteps)
            {
                return new ProfileResponseDto { Success = false, Message = $"Invalid step number. Must be between 1 and {TotalRecruiterSteps}." };
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ProfileResponseDto { Success = false, Message = "User not found" };
            }

            if (user.AccountType != AccountType.Recruiter)
            {
                return new ProfileResponseDto
                {
                    Success = false,
                    Message = "This operation is only available for recruiter accounts"
                };
            }

            if (user.ProfileCompletionStep < targetStep)
            {
                user.ProfileCompletionStep = targetStep;
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Advanced wizard for recruiter {UserId} to step {Step}", userId, targetStep);
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
            if (user == null || user.AccountType != AccountType.Recruiter)
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
                "Not Started",        // 0
                "Company Information"  // 1
            };

            var isComplete = user.ProfileCompletionStep >= TotalRecruiterSteps;
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

        public List<IndustryDto> GetIndustries() => Industries;

        public List<CompanySizeDto> GetCompanySizes() => CompanySizes;

        // ─── Helpers ─────────────────────────────────────────
        private static string? NormalizeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            return url.Trim();
        }
    }
}
