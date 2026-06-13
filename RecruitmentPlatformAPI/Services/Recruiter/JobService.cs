using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Jobs;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        private readonly IAIMatchingService _aiMatchingService;
        private readonly ILogger<JobService> _logger;

        public JobService(AppDbContext context, IAIMatchingService aiMatchingService, ILogger<JobService> logger)
        {
            _context = context;
            _aiMatchingService = aiMatchingService;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════
        //  JOB CRUD
        // ═══════════════════════════════════════════════════════════

        public async Task<JobServiceResult<JobResponseDto>> CreateJobAsync(int userId, JobRequestDto dto)
        {
            try
            {
                var validation = await ValidateRecruiterAsync<JobResponseDto>(userId);
                if (!validation.IsValid) return validation.Error!;

                var recruiter = validation.Recruiter!;

                // Validate location
                var locationValidation = await ValidateLocationAsync(dto.CountryId, dto.CityId, dto.WorkModel);
                if (!locationValidation.Success)
                    return JobServiceResult<JobResponseDto>.Fail(locationValidation.Message, locationValidation.ErrorCode);

                // Validate JobTitleId
                var titleValidation = await ValidateJobTitleIdAsync(dto.JobTitleId);
                if (!titleValidation.Success)
                    return JobServiceResult<JobResponseDto>.Fail(titleValidation.Message, titleValidation.ErrorCode);

                // Validate skill IDs if provided
                var skillValidation = await ValidateSkillIdsAsync(dto.SkillIds);
                if (!skillValidation.Success)
                    return JobServiceResult<JobResponseDto>.Fail(skillValidation.Message, skillValidation.ErrorCode);

                var job = new Job
                {
                    RecruiterId = recruiter.Id,
                    Title = dto.Title.Trim(),
                    JobTitleId = dto.JobTitleId,
                    Description = dto.Description.Trim(),
                    Requirements = dto.Requirements.Trim(),
                    EmploymentType = dto.EmploymentType,
                    WorkModel = dto.WorkModel,
                    MinYearsOfExperience = dto.MinYearsOfExperience,
                    CountryId = dto.CountryId,
                    CityId = dto.CityId,
                    PostedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                // Add skills if provided
                if (skillValidation.Data != null && skillValidation.Data.Count > 0)
                {
                    var jobSkills = skillValidation.Data.Select(skillId => new JobSkill
                    {
                        JobId = job.Id,
                        SkillId = skillId
                    }).ToList();

                    _context.JobSkills.AddRange(jobSkills);
                    await _context.SaveChangesAsync();
                }

                // Reload skills for accurate DTO mapping
                await _context.Entry(job).Collection(j => j.JobSkills).Query().Include(js => js.Skill).LoadAsync();

                _logger.LogInformation("Job {JobId} created by user {UserId}", job.Id, userId);
                return JobServiceResult<JobResponseDto>.Ok(
                    BuildJobResponseDto(job),
                    "Job created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job for user {UserId}", userId);
                return JobServiceResult<JobResponseDto>.Fail(
                    "Failed to create job. Please try again later.",
                    JobServiceErrorCode.ServerError);
            }
        }

        public async Task<JobServiceResult<JobResponseDto>> UpdateJobAsync(int userId, int jobId, JobRequestDto dto)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null)
                {
                    return JobServiceResult<JobResponseDto>.Fail(
                        "Job not found.",
                        JobServiceErrorCode.NotFound);
                }

                // Validate location
                var locationValidation = await ValidateLocationAsync(dto.CountryId, dto.CityId, dto.WorkModel);
                if (!locationValidation.Success)
                    return JobServiceResult<JobResponseDto>.Fail(locationValidation.Message, locationValidation.ErrorCode);

                // Validate JobTitleId
                var titleValidation = await ValidateJobTitleIdAsync(dto.JobTitleId);
                if (!titleValidation.Success)
                    return JobServiceResult<JobResponseDto>.Fail(titleValidation.Message, titleValidation.ErrorCode);

                // Validate skill IDs if provided
                var skillValidation = await ValidateSkillIdsAsync(dto.SkillIds);
                if (!skillValidation.Success)
                    return JobServiceResult<JobResponseDto>.Fail(skillValidation.Message, skillValidation.ErrorCode);

                // Detect core matching criteria changes for AI invalidation
                bool criteriaChanged = job.Title != dto.Title.Trim() ||
                                       job.JobTitleId != dto.JobTitleId ||
                                       job.Description != dto.Description.Trim() ||
                                       job.Requirements != dto.Requirements.Trim() ||
                                       job.EmploymentType != dto.EmploymentType ||
                                       job.WorkModel != dto.WorkModel ||
                                       job.MinYearsOfExperience != dto.MinYearsOfExperience ||
                                       job.CountryId != dto.CountryId ||
                                       job.CityId != dto.CityId;

                // Detect Skill changes
                if (!criteriaChanged)
                {
                    var existingSkillIds = job.JobSkills.Select(js => js.SkillId).OrderBy(id => id).ToList();
                    var incomingSkillIds = (skillValidation.Data ?? new List<int>()).OrderBy(id => id).ToList();
                    criteriaChanged = !existingSkillIds.SequenceEqual(incomingSkillIds);
                }

                if (criteriaChanged)
                {
                    _logger.LogInformation("Core matching criteria changed for Job {JobId}. Invalidating existing AI recommendations and cache.", jobId);
                    var existingRecommendations = await _context.Recommendations.Where(r => r.JobId == jobId).ToListAsync();
                    if (existingRecommendations.Any())
                    {
                        _context.Recommendations.RemoveRange(existingRecommendations);
                    }

                    // Invalidate the AI match cache so the next candidate fetch re-calls the external API
                    await _aiMatchingService.InvalidateCacheAsync(jobId);
                }

                // Update job fields
                job.Title = dto.Title.Trim();
                job.JobTitleId = dto.JobTitleId;
                job.Description = dto.Description.Trim();
                job.Requirements = dto.Requirements.Trim();
                job.EmploymentType = dto.EmploymentType;
                job.WorkModel = dto.WorkModel;
                job.MinYearsOfExperience = dto.MinYearsOfExperience;
                job.CountryId = dto.CountryId;
                job.CityId = dto.CityId;
                job.UpdatedAt = DateTime.UtcNow;

                // Replace skills: remove existing, add new
                job.JobSkills.Clear();

                if (skillValidation.Data != null && skillValidation.Data.Count > 0)
                {
                    foreach (var skillId in skillValidation.Data)
                    {
                        job.JobSkills.Add(new JobSkill
                        {
                            JobId = job.Id,
                            SkillId = skillId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Reload skills for accurate DTO mapping
                await _context.Entry(job).Collection(j => j.JobSkills).Query().Include(js => js.Skill).LoadAsync();

                // If criteria changed, recommendations were cleared; otherwise count existing
                var candidateCount = criteriaChanged
                    ? 0
                    : await _context.Recommendations.CountAsync(r => r.JobId == jobId);

                _logger.LogInformation("Job {JobId} updated by user {UserId}", jobId, userId);
                return JobServiceResult<JobResponseDto>.Ok(
                    BuildJobResponseDto(job, candidateCount),
                    "Job updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job {JobId} for user {UserId}", jobId, userId);
                return JobServiceResult<JobResponseDto>.Fail(
                    "Failed to update job. Please try again later.",
                    JobServiceErrorCode.ServerError);
            }
        }

        public async Task<JobServiceResult<bool>> DeactivateJobAsync(int userId, int jobId)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null)
                {
                    return JobServiceResult<bool>.Fail(
                        "Job not found.",
                        JobServiceErrorCode.NotFound);
                }

                job.IsActive = false;
                job.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} deactivated by user {UserId}", jobId, userId);
                return JobServiceResult<bool>.Ok(true, "Job deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating job {JobId} for user {UserId}", jobId, userId);
                return JobServiceResult<bool>.Fail(
                    "Failed to deactivate job. Please try again later.",
                    JobServiceErrorCode.ServerError);
            }
        }

        public async Task<JobServiceResult<bool>> ReactivateJobAsync(int userId, int jobId)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null)
                {
                    return JobServiceResult<bool>.Fail(
                        "Job not found.",
                        JobServiceErrorCode.NotFound);
                }

                job.IsActive = true;
                job.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} reactivated by user {UserId}", jobId, userId);
                return JobServiceResult<bool>.Ok(true, "Job reactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating job {JobId} for user {UserId}", jobId, userId);
                return JobServiceResult<bool>.Fail(
                    "Failed to reactivate job. Please try again later.",
                    JobServiceErrorCode.ServerError);
            }
        }

        public async Task<JobServiceResult<bool>> DeleteJobAsync(int userId, int jobId)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null)
                {
                    return JobServiceResult<bool>.Fail(
                        "Job not found.",
                        JobServiceErrorCode.NotFound);
                }

                // Hard delete — cascade removes JobSkills and Recommendations automatically
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} permanently deleted by user {UserId}", jobId, userId);
                return JobServiceResult<bool>.Ok(true, "Job deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job {JobId} for user {UserId}", jobId, userId);
                return JobServiceResult<bool>.Fail(
                    "Failed to delete job. Please try again later.",
                    JobServiceErrorCode.ServerError);
            }
        }

        public async Task<JobServiceResult<JobListResponseDto>> GetMyJobsAsync(int userId, int page = 1, int pageSize = 10, bool? isActive = null, string? search = null)
        {
            try
            {
                var validation = await ValidateRecruiterAsync<JobListResponseDto>(userId);
                if (!validation.IsValid) return validation.Error!;

                var recruiter = validation.Recruiter!;

                var query = _context.Jobs.Where(j => j.RecruiterId == recruiter.Id);
                if (isActive.HasValue) query = query.Where(j => j.IsActive == isActive.Value);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();
                    if (int.TryParse(search, out int searchId))
                    {
                        query = query.Where(j => j.Id == searchId);
                    }
                    else
                    {
                        query = query.Where(j => j.Title.Contains(search) || (j.JobTitle != null && j.JobTitle.TitleEn.Contains(search)));
                    }
                }

                var totalCount = await query.CountAsync();
                var jobs = await query
                    .AsNoTracking()
                    .Include(j => j.JobSkills)
                        .ThenInclude(js => js.Skill)
                    .Include(j => j.JobTitle)
                    .Include(j => j.Country)
                    .Include(j => j.City)
                    .AsSplitQuery()
                    .OrderByDescending(j => j.PostedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Batch-fetch recommendation counts for all jobs on this page
                var jobIds = jobs.Select(j => j.Id).ToList();
                var recCounts = await _context.Recommendations
                    .Where(r => jobIds.Contains(r.JobId))
                    .GroupBy(r => r.JobId)
                    .Select(g => new { JobId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.JobId, x => x.Count);

                var dtos = jobs.Select(j => BuildJobResponseDto(j, recCounts.GetValueOrDefault(j.Id, 0))).ToList();

                return JobServiceResult<JobListResponseDto>.Ok(new JobListResponseDto
                {
                    Jobs = dtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }, "Jobs retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs for user {UserId}", userId);
                return JobServiceResult<JobListResponseDto>.Fail(
                    "Failed to retrieve jobs. Please try again later.",
                    JobServiceErrorCode.ServerError);
            }
        }

        public async Task<JobServiceResult<JobResponseDto>> GetJobByIdAsync(int userId, int jobId)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId, track: false);
                if (job == null)
                {
                    return JobServiceResult<JobResponseDto>.Fail(
                        "Job not found.",
                        JobServiceErrorCode.NotFound);
                }

                var candidateCount = await _context.Recommendations.CountAsync(r => r.JobId == jobId);

                return JobServiceResult<JobResponseDto>.Ok(
                    BuildJobResponseDto(job, candidateCount),
                    "Job retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job {JobId} for user {UserId}", jobId, userId);
                return JobServiceResult<JobResponseDto>.Fail(
                    "Failed to retrieve job. Please try again later.",
                    JobServiceErrorCode.ServerError);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  REFERENCE DATA
        // ═══════════════════════════════════════════════════════════

        public async Task<List<SkillOptionDto>> GetSkillsAsync(string? search = null)
        {
            try
            {
                var query = _context.Skills.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.Trim().ToLower();
                    query = query.Where(s => s.Name.ToLower().Contains(searchLower));
                }

                return await query
                    .OrderBy(s => s.Name)
                    .Select(s => new SkillOptionDto { Id = s.Id, Name = s.Name })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skills list");
                return new List<SkillOptionDto>();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Returns a job only if it belongs to the recruiter account linked to userId.
        /// Returns null if not found or if ownership check fails.
        /// </summary>
        private async Task<Job?> GetOwnedJobAsync(int userId, int jobId, bool track = true)
        {
            var query = _context.Jobs
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Include(j => j.JobTitle)
                .Include(j => j.Recruiter)
                    .ThenInclude(r => r.User)
                .Include(j => j.Country)
                .Include(j => j.City)
                .AsQueryable();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(j => 
                j.Id == jobId && 
                j.Recruiter.UserId == userId && 
                j.Recruiter.User.AccountType == AccountType.Recruiter);
        }

        /// <summary>
        /// Builds a full JobResponseDto including skills from eagerly loaded JobSkills.
        /// </summary>
        private static JobResponseDto BuildJobResponseDto(Job job, int candidateCount = 0)
        {
            var skills = job.JobSkills?.Select(js => new JobSkillDto
            {
                Id = js.Skill.Id,
                Name = js.Skill.Name
            }).ToList() ?? new List<JobSkillDto>();

            return MapJobResponse(job, skills, candidateCount);
        }

        private static JobResponseDto MapJobResponse(Job job, List<JobSkillDto> skills, int candidateCount = 0)
        {
            return new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                JobTitleId = job.JobTitleId,
                JobTitleName = job.JobTitle?.TitleEn,
                Description = job.Description,
                Requirements = job.Requirements,
                EmploymentType = job.EmploymentType,
                WorkModel = job.WorkModel,
                MinYearsOfExperience = job.MinYearsOfExperience,
                CountryId = job.CountryId,
                CountryName = job.Country?.NameEn,
                CityId = job.CityId,
                CityName = job.City?.NameEn,
                PostedAt = job.PostedAt,
                UpdatedAt = job.UpdatedAt,
                IsActive = job.IsActive,
                CandidateCount = candidateCount,
                Skills = skills
            };
        }

        private async Task<JobServiceResult<bool>> ValidateJobTitleIdAsync(int jobTitleId)
        {
            var exists = await _context.JobTitles.AnyAsync(jt => jt.Id == jobTitleId && jt.IsActive);
            if (!exists)
            {
                return JobServiceResult<bool>.Fail(
                    "Invalid job title ID or the job title is not active.",
                    JobServiceErrorCode.Validation);
            }

            return JobServiceResult<bool>.Ok(true);
        }

        private async Task<JobServiceResult<List<int>>> ValidateSkillIdsAsync(List<int>? skillIds)
        {
            if (skillIds == null || skillIds.Count == 0)
            {
                return JobServiceResult<List<int>>.Ok(new List<int>());
            }

            var requestedIds = skillIds.Distinct().ToList();
            if (requestedIds.Count > 25)
            {
                return JobServiceResult<List<int>>.Fail(
                    "Maximum 25 skills allowed per job.",
                    JobServiceErrorCode.Validation);
            }

            var validSkillIds = await _context.Skills
                .Where(s => requestedIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            if (validSkillIds.Count != requestedIds.Count)
            {
                var invalidIds = requestedIds.Except(validSkillIds).ToList();
                return JobServiceResult<List<int>>.Fail(
                    $"Invalid skill IDs: {string.Join(", ", invalidIds)}",
                    JobServiceErrorCode.Validation);
            }

            return JobServiceResult<List<int>>.Ok(requestedIds);
        }

        /// <summary>
        /// Validates if the user exists and is a recruiter with a completed profile.
        /// Performs a single DB query via projection.
        /// </summary>
        private async Task<(bool IsValid, Models.Recruiter.Recruiter? Recruiter, JobServiceResult<T>? Error)> ValidateRecruiterAsync<T>(int userId)
        {
            var data = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new {
                    User = u,
                    Recruiter = _context.Recruiters.FirstOrDefault(r => r.UserId == u.Id)
                })
                .FirstOrDefaultAsync();

            if (data?.User == null || data.User.AccountType != AccountType.Recruiter)
            {
                _logger.LogWarning("Validation failed: User {UserId} is not a recruiter", userId);
                return (false, null, JobServiceResult<T>.Fail(
                    "Recruiter access is required to perform this action.",
                    JobServiceErrorCode.Forbidden));
            }

            if (data.Recruiter == null)
            {
                _logger.LogWarning("Validation failed: No recruiter profile for user {UserId}", userId);
                return (false, null, JobServiceResult<T>.Fail(
                    "Recruiter profile not found. Please complete company information first.",
                    JobServiceErrorCode.ProfileMissing));
            }

            return (true, data.Recruiter, null);
        }

        private async Task<JobServiceResult<bool>> ValidateLocationAsync(int? countryId, int? cityId, WorkModel workModel)
        {
            // If OnSite or Hybrid, country and city are required
            if (workModel == WorkModel.OnSite || workModel == WorkModel.Hybrid)
            {
                if (!countryId.HasValue)
                {
                    return JobServiceResult<bool>.Fail("Country is required for On-Site or Hybrid jobs.", JobServiceErrorCode.Validation);
                }
                if (!cityId.HasValue)
                {
                    return JobServiceResult<bool>.Fail("City is required for On-Site or Hybrid jobs.", JobServiceErrorCode.Validation);
                }
            }

            // If a country is provided, validate it
            if (countryId.HasValue)
            {
                var countryExists = await _context.Countries.AnyAsync(c => c.Id == countryId.Value);
                if (!countryExists)
                {
                    return JobServiceResult<bool>.Fail("Invalid Country ID.", JobServiceErrorCode.Validation);
                }
            }

            // If a city is provided, validate it and ensure it belongs to the country
            if (cityId.HasValue)
            {
                if (!countryId.HasValue)
                {
                    return JobServiceResult<bool>.Fail("Country ID must be provided when City ID is specified.", JobServiceErrorCode.Validation);
                }

                var city = await _context.Cities.FindAsync(cityId.Value);
                if (city == null)
                {
                    return JobServiceResult<bool>.Fail("Invalid City ID.", JobServiceErrorCode.Validation);
                }

                if (city.CountryId != countryId.Value)
                {
                    return JobServiceResult<bool>.Fail("The selected city does not belong to the selected country.", JobServiceErrorCode.Validation);
                }
            }

            return JobServiceResult<bool>.Ok(true);
        }
    }
}
