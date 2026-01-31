using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.Profile
{
    /// <summary>
    /// Service for managing work experience entries
    /// </summary>
    public class ExperienceService : IExperienceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExperienceService> _logger;

        public ExperienceService(AppDbContext context, ILogger<ExperienceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ExperienceListResponseDto> GetExperiencesAsync(int userId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker == null)
            {
                return new ExperienceListResponseDto();
            }

            var experiences = await _context.Experiences
                .Where(e => e.JobSeekerId == jobSeeker.Id && !e.IsDeleted)
                .OrderBy(e => e.DisplayOrder)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();

            return new ExperienceListResponseDto
            {
                Experiences = experiences.Select(MapToResponseDto).ToList(),
                TotalCount = experiences.Count
            };
        }

        public async Task<ExperienceResponseDto?> GetExperienceByIdAsync(int userId, int experienceId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker == null) return null;

            var experience = await _context.Experiences
                .FirstOrDefaultAsync(e => e.Id == experienceId && e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);

            return experience != null ? MapToResponseDto(experience) : null;
        }

        public async Task<ExperienceResponseDto?> AddExperienceAsync(int userId, ExperienceRequestDto dto)
        {
            try
            {
                var jobSeeker = await GetOrCreateJobSeekerAsync(userId);
                if (jobSeeker == null) return null;

                // Validate dates
                if (dto.IsCurrent)
                {
                    dto.EndDate = null;
                }
                else if (dto.EndDate.HasValue && dto.EndDate < dto.StartDate)
                {
                    _logger.LogWarning("Invalid date range: EndDate {EndDate} is before StartDate {StartDate}", dto.EndDate, dto.StartDate);
                    return null;
                }

                var experience = new Experience
                {
                    JobSeekerId = jobSeeker.Id,
                    JobTitle = dto.JobTitle.Trim(),
                    CompanyName = dto.CompanyName.Trim(),
                    Location = dto.Location?.Trim(),
                    EmploymentType = dto.EmploymentType,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsCurrent = dto.IsCurrent,
                    Responsibilities = dto.Responsibilities?.Trim(),
                    DisplayOrder = dto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Experiences.Add(experience);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Experience added for user {UserId}: {JobTitle} at {Company}", 
                    userId, experience.JobTitle, experience.CompanyName);

                return MapToResponseDto(experience);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding experience for user {UserId}", userId);
                return null;
            }
        }

        public async Task<ExperienceResponseDto?> UpdateExperienceAsync(int userId, int experienceId, ExperienceRequestDto dto)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null) return null;

                var experience = await _context.Experiences
                    .FirstOrDefaultAsync(e => e.Id == experienceId && e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);

                if (experience == null) return null;

                // Validate dates
                if (dto.IsCurrent)
                {
                    dto.EndDate = null;
                }
                else if (dto.EndDate.HasValue && dto.EndDate < dto.StartDate)
                {
                    _logger.LogWarning("Invalid date range: EndDate {EndDate} is before StartDate {StartDate}", dto.EndDate, dto.StartDate);
                    return null;
                }

                experience.JobTitle = dto.JobTitle.Trim();
                experience.CompanyName = dto.CompanyName.Trim();
                experience.Location = dto.Location?.Trim();
                experience.EmploymentType = dto.EmploymentType;
                experience.StartDate = dto.StartDate;
                experience.EndDate = dto.EndDate;
                experience.IsCurrent = dto.IsCurrent;
                experience.Responsibilities = dto.Responsibilities?.Trim();
                experience.DisplayOrder = dto.DisplayOrder;
                experience.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Experience {ExperienceId} updated for user {UserId}", experienceId, userId);

                return MapToResponseDto(experience);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating experience {ExperienceId} for user {UserId}", experienceId, userId);
                return null;
            }
        }

        public async Task<bool> DeleteExperienceAsync(int userId, int experienceId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null) return false;

                var experience = await _context.Experiences
                    .FirstOrDefaultAsync(e => e.Id == experienceId && e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);

                if (experience == null) return false;

                // Soft delete
                experience.IsDeleted = true;
                experience.DeletedAt = DateTime.UtcNow;
                experience.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Experience {ExperienceId} deleted for user {UserId}", experienceId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting experience {ExperienceId} for user {UserId}", experienceId, userId);
                return false;
            }
        }

        public async Task<bool> ReorderExperiencesAsync(int userId, List<int> orderedIds)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null) return false;

                var experiences = await _context.Experiences
                    .Where(e => e.JobSeekerId == jobSeeker.Id && !e.IsDeleted && orderedIds.Contains(e.Id))
                    .ToListAsync();

                for (int i = 0; i < orderedIds.Count; i++)
                {
                    var experience = experiences.FirstOrDefault(e => e.Id == orderedIds[i]);
                    if (experience != null)
                    {
                        experience.DisplayOrder = i;
                        experience.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering experiences for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> HasExperienceAsync(int userId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker == null) return false;

            return await _context.Experiences
                .AnyAsync(e => e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);
        }

        #region Private Methods

        private async Task<Models.JobSeeker.JobSeeker?> GetJobSeekerAsync(int userId)
        {
            return await _context.JobSeekers.FirstOrDefaultAsync(js => js.UserId == userId);
        }

        private async Task<Models.JobSeeker.JobSeeker?> GetOrCreateJobSeekerAsync(int userId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker != null) return jobSeeker;

            // Create new JobSeeker record
            jobSeeker = new Models.JobSeeker.JobSeeker
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.JobSeekers.Add(jobSeeker);
            await _context.SaveChangesAsync();

            return jobSeeker;
        }

        private static ExperienceResponseDto MapToResponseDto(Experience experience)
        {
            return new ExperienceResponseDto
            {
                Id = experience.Id,
                JobTitle = experience.JobTitle,
                CompanyName = experience.CompanyName,
                Location = experience.Location,
                EmploymentType = experience.EmploymentType,
                EmploymentTypeName = GetEmploymentTypeName(experience.EmploymentType),
                StartDate = experience.StartDate,
                EndDate = experience.EndDate,
                IsCurrent = experience.IsCurrent,
                Responsibilities = experience.Responsibilities,
                DisplayOrder = experience.DisplayOrder,
                DateRange = FormatDateRange(experience.StartDate, experience.EndDate, experience.IsCurrent),
                CreatedAt = experience.CreatedAt,
                UpdatedAt = experience.UpdatedAt
            };
        }

        private static string GetEmploymentTypeName(Enums.EmploymentType type)
        {
            return type switch
            {
                Enums.EmploymentType.FullTime => "Full-time",
                Enums.EmploymentType.PartTime => "Part-time",
                Enums.EmploymentType.Freelance => "Freelance/Contract",
                Enums.EmploymentType.Internship => "Internship",
                _ => type.ToString()
            };
        }

        private static string FormatDateRange(DateTime startDate, DateTime? endDate, bool isCurrent)
        {
            var start = startDate.ToString("MMM yyyy");
            var end = isCurrent ? "Present" : endDate?.ToString("MMM yyyy") ?? "Present";
            return $"{start} - {end}";
        }

        #endregion
    }
}
