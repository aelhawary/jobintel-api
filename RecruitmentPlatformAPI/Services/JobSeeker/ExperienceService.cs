using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
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

        public async Task<ExperienceListResponseDto> GetExperiencesAsync(int userId, string lang = "en")
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker == null)
            {
                return new ExperienceListResponseDto();
            }

            var experiences = await _context.Experiences
                .Include(e => e.Country)
                .Include(e => e.City)
                .Where(e => e.JobSeekerId == jobSeeker.Id && !e.IsDeleted)
                .OrderBy(e => e.DisplayOrder)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();

            return new ExperienceListResponseDto
            {
                Experiences = experiences.Select(e => MapToResponseDto(e, lang)).ToList(),
                TotalCount = experiences.Count
            };
        }

        public async Task<ExperienceResponseDto?> GetExperienceByIdAsync(int userId, int experienceId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker == null) return null;

            var experience = await _context.Experiences
                .Include(e => e.Country)
                .Include(e => e.City)
                .FirstOrDefaultAsync(e => e.Id == experienceId && e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);

            return experience != null ? MapToResponseDto(experience, "en") : null;
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
                else if (!dto.EndDate.HasValue)
                {
                    _logger.LogWarning("Missing EndDate for non-current experience: User {UserId}", userId);
                    return null;
                }
                else if (dto.EndDate.HasValue && dto.EndDate < dto.StartDate)
                {
                    _logger.LogWarning("Invalid date range: EndDate {EndDate} is before StartDate {StartDate}", dto.EndDate, dto.StartDate);
                    return null;
                }

                // Smart Location Fallback
                int finalCountryId = (dto.CountryId == null || dto.CountryId <= 0) ? jobSeeker.CountryId ?? 0 : dto.CountryId.Value;
                int finalCityId = (dto.CityId == null || dto.CityId <= 0) ? jobSeeker.CityId ?? 0 : dto.CityId.Value;

                if (finalCountryId <= 0 || finalCityId <= 0)
                {
                    _logger.LogWarning("Missing valid Country/City for experience and no default on JobSeeker profile: User {UserId}", userId);
                    return null;
                }

                var experience = new Experience
                {
                    JobSeekerId = jobSeeker.Id,
                    JobTitle = dto.JobTitle.Trim(),
                    CompanyName = dto.CompanyName.Trim(),
                    CountryId = finalCountryId,
                    CityId = finalCityId,
                    EmploymentType = dto.EmploymentType,
                    Responsibilities = dto.Responsibilities?.Trim(),
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsCurrent = dto.IsCurrent,
                    DisplayOrder = dto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Experiences.Add(experience);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Experience added for user {UserId}: {JobTitle} at {Company}", 
                    userId, experience.JobTitle, experience.CompanyName);

                await _context.Entry(experience).Reference(e => e.Country).LoadAsync();
                await _context.Entry(experience).Reference(e => e.City).LoadAsync();

                return MapToResponseDto(experience, "en");
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
                    .Include(e => e.Country)
                    .Include(e => e.City)
                    .FirstOrDefaultAsync(e => e.Id == experienceId && e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);

                if (experience == null) return null;

                // Validate dates
                if (dto.IsCurrent)
                {
                    dto.EndDate = null;
                }
                else if (!dto.EndDate.HasValue)
                {
                    _logger.LogWarning("Missing EndDate for non-current experience: User {UserId}, Experience {ExperienceId}", userId, experienceId);
                    return null;
                }
                else if (dto.EndDate.HasValue && dto.EndDate < dto.StartDate)
                {
                    _logger.LogWarning("Invalid date range: EndDate {EndDate} is before StartDate {StartDate}", dto.EndDate, dto.StartDate);
                    return null;
                }

                // Smart Location Fallback
                int finalCountryId = (dto.CountryId == null || dto.CountryId <= 0) ? jobSeeker.CountryId ?? 0 : dto.CountryId.Value;
                int finalCityId = (dto.CityId == null || dto.CityId <= 0) ? jobSeeker.CityId ?? 0 : dto.CityId.Value;

                if (finalCountryId <= 0 || finalCityId <= 0)
                {
                    _logger.LogWarning("Missing valid Country/City for experience and no default on JobSeeker profile: User {UserId}, Experience {ExperienceId}", userId, experienceId);
                    return null;
                }

                experience.JobTitle = dto.JobTitle.Trim();
                experience.CompanyName = dto.CompanyName.Trim();
                experience.CountryId = finalCountryId;
                experience.CityId = finalCityId;
                experience.EmploymentType = dto.EmploymentType;
                experience.Responsibilities = dto.Responsibilities?.Trim();
                experience.StartDate = dto.StartDate;
                experience.EndDate = dto.EndDate;
                experience.IsCurrent = dto.IsCurrent;
                experience.DisplayOrder = dto.DisplayOrder;
                experience.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Experience {ExperienceId} updated for user {UserId}", experienceId, userId);

                await _context.Entry(experience).Reference(e => e.Country).LoadAsync();
                await _context.Entry(experience).Reference(e => e.City).LoadAsync();

                return MapToResponseDto(experience, "en");
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

        private static ExperienceResponseDto MapToResponseDto(Experience experience, string lang = "en")
        {
            bool isAr = lang.ToLower() == "ar";
            return new ExperienceResponseDto
            {
                Id = experience.Id,
                JobTitle = experience.JobTitle,
                CompanyName = experience.CompanyName,
                CountryId = experience.CountryId,
                Country = (isAr && !string.IsNullOrEmpty(experience.Country?.NameAr)) ? experience.Country.NameAr : (experience.Country?.NameEn ?? string.Empty),
                CityId = experience.CityId,
                City = (isAr && !string.IsNullOrEmpty(experience.City?.NameAr)) ? experience.City.NameAr : (experience.City?.NameEn ?? string.Empty),
                EmploymentType = experience.EmploymentType,
                Responsibilities = experience.Responsibilities,
                StartDate = experience.StartDate,
                EndDate = experience.EndDate,
                IsCurrent = experience.IsCurrent,
                DisplayOrder = experience.DisplayOrder,
                DateRange = FormatDateRange(experience.StartDate, experience.EndDate, experience.IsCurrent, isAr),
                CreatedAt = experience.CreatedAt,
                UpdatedAt = experience.UpdatedAt
            };
        }

        private static string FormatDateRange(DateTime startDate, DateTime? endDate, bool isCurrent, bool isAr)
        {
            var culture = isAr ? new System.Globalization.CultureInfo("ar-EG") : new System.Globalization.CultureInfo("en-US");
            var start = startDate.ToString("MMM yyyy", culture);
            var present = isAr ? "الحاضر" : "Present";
            var end = isCurrent ? present : endDate?.ToString("MMM yyyy", culture) ?? present;
            return $"{start} - {end}";
        }

        #endregion
    }
}
