using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.Profile
{
    /// <summary>
    /// Service for managing education entries
    /// </summary>
    public class EducationService : IEducationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EducationService> _logger;

        public EducationService(AppDbContext context, ILogger<EducationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EducationListResponseDto> GetEducationAsync(int userId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker == null)
            {
                return new EducationListResponseDto();
            }

            var educationList = await _context.Educations
                .Where(e => e.JobSeekerId == jobSeeker.Id && !e.IsDeleted)
                .OrderBy(e => e.DisplayOrder)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();

            return new EducationListResponseDto
            {
                EducationList = educationList.Select(MapToResponseDto).ToList(),
                TotalCount = educationList.Count
            };
        }

        public async Task<EducationResponseDto?> GetEducationByIdAsync(int userId, int educationId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker == null) return null;

            var education = await _context.Educations
                .FirstOrDefaultAsync(e => e.Id == educationId && e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);

            return education != null ? MapToResponseDto(education) : null;
        }

        public async Task<EducationResponseDto?> AddEducationAsync(int userId, EducationRequestDto dto)
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

                var education = new Education
                {
                    JobSeekerId = jobSeeker.Id,
                    Institution = dto.Institution.Trim(),
                    Degree = dto.Degree.Trim(),
                    Major = dto.FieldOfStudy.Trim(), // FieldOfStudy maps to Major in model
                    GradeOrGPA = dto.GradeOrGPA?.Trim(),
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsCurrent = dto.IsCurrent,
                    DisplayOrder = dto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Educations.Add(education);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Education added for user {UserId}: {Degree} at {Institution}", 
                    userId, education.Degree, education.Institution);

                return MapToResponseDto(education);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding education for user {UserId}", userId);
                return null;
            }
        }

        public async Task<EducationResponseDto?> UpdateEducationAsync(int userId, int educationId, EducationRequestDto dto)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null) return null;

                var education = await _context.Educations
                    .FirstOrDefaultAsync(e => e.Id == educationId && e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);

                if (education == null) return null;

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

                education.Institution = dto.Institution.Trim();
                education.Degree = dto.Degree.Trim();
                education.Major = dto.FieldOfStudy.Trim();
                education.GradeOrGPA = dto.GradeOrGPA?.Trim();
                education.StartDate = dto.StartDate;
                education.EndDate = dto.EndDate;
                education.IsCurrent = dto.IsCurrent;
                education.DisplayOrder = dto.DisplayOrder;
                education.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Education {EducationId} updated for user {UserId}", educationId, userId);

                return MapToResponseDto(education);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating education {EducationId} for user {UserId}", educationId, userId);
                return null;
            }
        }

        public async Task<bool> DeleteEducationAsync(int userId, int educationId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null) return false;

                var education = await _context.Educations
                    .FirstOrDefaultAsync(e => e.Id == educationId && e.JobSeekerId == jobSeeker.Id && !e.IsDeleted);

                if (education == null) return false;

                // Soft delete
                education.IsDeleted = true;
                education.DeletedAt = DateTime.UtcNow;
                education.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Education {EducationId} deleted for user {UserId}", educationId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting education {EducationId} for user {UserId}", educationId, userId);
                return false;
            }
        }

        public async Task<bool> ReorderEducationAsync(int userId, List<int> orderedIds)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null) return false;

                var educationList = await _context.Educations
                    .Where(e => e.JobSeekerId == jobSeeker.Id && !e.IsDeleted && orderedIds.Contains(e.Id))
                    .ToListAsync();

                for (int i = 0; i < orderedIds.Count; i++)
                {
                    var education = educationList.FirstOrDefault(e => e.Id == orderedIds[i]);
                    if (education != null)
                    {
                        education.DisplayOrder = i;
                        education.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering education for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> HasEducationAsync(int userId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker == null) return false;

            return await _context.Educations
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

        private static EducationResponseDto MapToResponseDto(Education education)
        {
            return new EducationResponseDto
            {
                Id = education.Id,
                Institution = education.Institution,
                Degree = education.Degree,
                FieldOfStudy = education.Major, // Major maps to FieldOfStudy in DTO
                GradeOrGPA = education.GradeOrGPA,
                StartDate = education.StartDate,
                EndDate = education.EndDate,
                IsCurrent = education.IsCurrent,
                DisplayOrder = education.DisplayOrder,
                DateRange = FormatDateRange(education.StartDate, education.EndDate, education.IsCurrent),
                CreatedAt = education.CreatedAt,
                UpdatedAt = education.UpdatedAt
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
