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
        private readonly ILogger<JobService> _logger;
        private readonly IAiRecommendationService _aiService;




        public JobService(AppDbContext context, ILogger<JobService> logger, IAiRecommendationService aiService)
        {
            _context = context;
            _logger = logger;
            _aiService = aiService;
        }

        // ═══════════════════════════════════════════════════════════
        //  JOB CRUD
        // ═══════════════════════════════════════════════════════════

        public async Task<JobResponseDto?> CreateJobAsync(int userId, JobRequestDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.AccountType != AccountType.Recruiter)
                {
                    _logger.LogWarning("CreateJob failed: User {UserId} is not a recruiter", userId);
                    return null;
                }

                var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
                if (recruiter == null)
                {
                    _logger.LogWarning("CreateJob failed: No recruiter profile for user {UserId}", userId);
                    return null;
                }

                // Validate skill IDs if provided
                if (dto.SkillIds != null && dto.SkillIds.Count > 0)
                {
                    var validSkillIds = await _context.Skills
                        .Where(s => dto.SkillIds.Contains(s.Id))
                        .Select(s => s.Id)
                        .ToListAsync();

                    if (validSkillIds.Count != dto.SkillIds.Distinct().Count())
                    {
                        _logger.LogWarning("CreateJob failed: Invalid skill IDs provided by user {UserId}", userId);
                        return null;
                    }
                }

                var job = new Job
                {
                    RecruiterId = recruiter.Id,
                    Title = dto.Title.Trim(),
                    Description = dto.Description.Trim(),
                    Requirements = dto.Requirements.Trim(),
                    EmploymentType = dto.EmploymentType,
                    MinYearsOfExperience = dto.MinYearsOfExperience,
                    Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim(),
                    PostedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                // Add skills if provided
                if (dto.SkillIds != null && dto.SkillIds.Count > 0)
                {
                    var jobSkills = dto.SkillIds.Distinct().Select(skillId => new JobSkill
                    {
                        JobId = job.Id,
                        SkillId = skillId
                    }).ToList();

                    _context.JobSkills.AddRange(jobSkills);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Job {JobId} created by user {UserId}", job.Id, userId);
                return await BuildJobResponseDto(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job for user {UserId}", userId);
                return null;
            }
        }

        public async Task<JobResponseDto?> UpdateJobAsync(int userId, int jobId, JobRequestDto dto)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null) return null;

                // Validate skill IDs if provided
                if (dto.SkillIds != null && dto.SkillIds.Count > 0)
                {
                    var validSkillIds = await _context.Skills
                        .Where(s => dto.SkillIds.Contains(s.Id))
                        .Select(s => s.Id)
                        .ToListAsync();

                    if (validSkillIds.Count != dto.SkillIds.Distinct().Count())
                    {
                        _logger.LogWarning("UpdateJob failed: Invalid skill IDs for job {JobId} by user {UserId}", jobId, userId);
                        return null;
                    }
                }

                // Update job fields
                job.Title = dto.Title.Trim();
                job.Description = dto.Description.Trim();
                job.Requirements = dto.Requirements.Trim();
                job.EmploymentType = dto.EmploymentType;
                job.MinYearsOfExperience = dto.MinYearsOfExperience;
                job.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim();
                job.UpdatedAt = DateTime.UtcNow;

                // Replace skills: remove existing, add new
                var existingSkills = await _context.JobSkills
                    .Where(js => js.JobId == job.Id)
                    .ToListAsync();
                _context.JobSkills.RemoveRange(existingSkills);

                if (dto.SkillIds != null && dto.SkillIds.Count > 0)
                {
                    var jobSkills = dto.SkillIds.Distinct().Select(skillId => new JobSkill
                    {
                        JobId = job.Id,
                        SkillId = skillId
                    }).ToList();

                    _context.JobSkills.AddRange(jobSkills);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} updated by user {UserId}", jobId, userId);
                return await BuildJobResponseDto(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job {JobId} for user {UserId}", jobId, userId);
                return null;
            }
        }

        public async Task<bool> DeactivateJobAsync(int userId, int jobId)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null) return false;

                job.IsActive = false;
                job.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} deactivated by user {UserId}", jobId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating job {JobId} for user {UserId}", jobId, userId);
                return false;
            }
        }

        public async Task<bool> ReactivateJobAsync(int userId, int jobId)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null) return false;

                job.IsActive = true;
                job.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} reactivated by user {UserId}", jobId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating job {JobId} for user {UserId}", jobId, userId);
                return false;
            }
        }

        public async Task<bool> DeleteJobAsync(int userId, int jobId)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null) return false;

                // Hard delete — cascade removes JobSkills and Recommendations automatically
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Job {JobId} permanently deleted by user {UserId}", jobId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job {JobId} for user {UserId}", jobId, userId);
                return false;
            }
        }

        public async Task<JobListResponseDto?> GetMyJobsAsync(int userId, int page = 1, int pageSize = 10, bool? isActive = null)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.AccountType != AccountType.Recruiter)
                {
                    _logger.LogWarning("GetMyJobs denied: User {UserId} is not a recruiter", userId);
                    return null;
                }

                var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
                if (recruiter == null)
                    return new JobListResponseDto { Page = page, PageSize = pageSize };

                var query = _context.Jobs.Where(j => j.RecruiterId == recruiter.Id);
                if (isActive.HasValue) query = query.Where(j => j.IsActive == isActive.Value);

                var totalCount = await query.CountAsync();
                var jobs = await query
                    .OrderByDescending(j => j.PostedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = new List<JobResponseDto>();
                foreach (var job in jobs)
                    dtos.Add(await BuildJobResponseDto(job));

                return new JobListResponseDto
                {
                    Jobs = dtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs for user {UserId}", userId);
                return new JobListResponseDto { Page = page, PageSize = pageSize };
            }
        }

        public async Task<JobResponseDto?> GetJobByIdAsync(int userId, int jobId)
        {
            try
            {
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null) return null;
                return await BuildJobResponseDto(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job {JobId} for user {UserId}", jobId, userId);
                return null;
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
        private async Task<Job?> GetOwnedJobAsync(int userId, int jobId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.AccountType != AccountType.Recruiter)
                return null;

            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return null;

            return await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == recruiter.Id);
        }

        /// <summary>
        /// Builds a full JobResponseDto including skills.
        /// </summary>
        private async Task<JobResponseDto> BuildJobResponseDto(Job job)
        {
            var skills = await _context.JobSkills
                .Where(js => js.JobId == job.Id)
                .Join(_context.Skills, js => js.SkillId, s => s.Id,
                    (js, s) => new JobSkillDto { Id = s.Id, Name = s.Name })
                .ToListAsync();

            return new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements,
                EmploymentType = job.EmploymentType,
                MinYearsOfExperience = job.MinYearsOfExperience,
                Location = job.Location,
                PostedAt = job.PostedAt,
                UpdatedAt = job.UpdatedAt,
                IsActive = job.IsActive,
                CandidateCount = 0, // Reserved for future AI matching module
                Skills = skills
            };
        }

        public async Task<CandidateProfileDto?> GetCandidateProfileAsync(
    int userId, int jobId, int jobSeekerId)
        {
            try
            {

                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null) return null;


                var recommendation = await _context.Recommendations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.JobId == jobId && r.JobSeekerId == jobSeekerId);

                if (recommendation == null) return null;


                var jobSeeker = await _context.JobSeekers
                    .AsNoTracking()
                    .Include(js => js.User)
                    .Include(js => js.JobTitle)
                    .Include(js => js.Country)
                    .Include(js => js.FirstLanguage)
                    .Include(js => js.SecondLanguage)
                    .FirstOrDefaultAsync(js => js.Id == jobSeekerId);

                if (jobSeeker == null) return null;


                var skills = await _context.JobSeekerSkills
                    .AsNoTracking()
                    .Where(s => s.JobSeekerId == jobSeekerId)
                    .Join(_context.Skills,
                        jss => jss.SkillId,
                        s => s.Id,
                        (jss, s) => new CandidateSkillDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Source = jss.Source
                        })
                    .ToListAsync();

                var experiences = await _context.Experiences
                    .AsNoTracking()
                    .Where(e => e.JobSeekerId == jobSeekerId && !e.IsDeleted)
                    .OrderBy(e => e.DisplayOrder)
                    .Select(e => new CandidateExperienceDto
                    {
                        Id = e.Id,
                        JobTitle = e.JobTitle,
                        CompanyName = e.CompanyName,
                        Location = e.Location,
                        EmploymentType = e.EmploymentType.ToString(),
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        IsCurrent = e.IsCurrent,
                        Responsibilities = e.Responsibilities
                    })
                    .ToListAsync();

                var educations = await _context.Educations
                    .AsNoTracking()
                    .Where(e => e.JobSeekerId == jobSeekerId && !e.IsDeleted)
                    .OrderBy(e => e.DisplayOrder)
                    .Select(e => new CandidateEducationDto
                    {
                        Id = e.Id,
                        Institution = e.Institution,
                        Degree = e.Degree.ToString(),
                        Major = e.Major,
                        GradeOrGPA = e.GradeOrGPA,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        IsCurrent = e.IsCurrent
                    })
                    .ToListAsync();

                var projects = await _context.Projects
                    .AsNoTracking()
                    .Where(p => p.JobSeekerId == jobSeekerId && !p.IsDeleted)
                    .OrderBy(p => p.DisplayOrder)
                    .Select(p => new CandidateProjectDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        TechnologiesUsed = p.TechnologiesUsed,
                        ProjectLink = p.ProjectLink
                    })
                    .ToListAsync();

                var certificates = await _context.Certificates
                    .AsNoTracking()
                    .Where(c => c.JobSeekerId == jobSeekerId && !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new CandidateCertificateDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        IssuingOrganization = c.IssuingOrganization,
                        IssueDate = c.IssueDate,
                        ExpirationDate = c.ExpirationDate
                    })
                    .ToListAsync();

                var social = await _context.SocialAccounts
                    .AsNoTracking()
                    .Where(s => s.JobSeekerId == jobSeekerId)
                    .Select(s => new CandidateSocialDto
                    {
                        LinkedIn = s.LinkedIn,
                        Github = s.Github,
                        PersonalWebsite = s.PersonalWebsite,
                        Behance = s.Behance,
                        Dribbble = s.Dribbble
                    })
                    .FirstOrDefaultAsync();

                var resume = await _context.Resumes
                    .AsNoTracking()
                    .Where(r => r.JobSeekerId == jobSeekerId && !r.IsDeleted)
                    .Select(r => new CandidateResumeDto
                    {
                        FileName = r.FileName,
                        FilePath = r.FilePath,
                        FileSizeBytes = r.FileSizeBytes,
                        ContentType = r.ContentType
                    })
                    .FirstOrDefaultAsync();

                return new CandidateProfileDto
                {
                    // AI match info
                    MatchScore = recommendation.MatchScore,
                    RecommendedAt = recommendation.GeneratedAt,

                    // Identity
                    JobSeekerId = jobSeeker.Id,
                    FirstName = jobSeeker.User.FirstName,
                    LastName = jobSeeker.User.LastName,
                    Email = jobSeeker.User.Email,
                    ProfilePictureUrl = jobSeeker.User.ProfilePictureUrl,
                    PhoneNumber = jobSeeker.PhoneNumber,
                    Bio = jobSeeker.Bio,
                    City = jobSeeker.City,
                    Country = jobSeeker.Country?.NameEn,
                    JobTitle = jobSeeker.JobTitle?.Title,
                    YearsOfExperience = jobSeeker.YearsOfExperience,

                    // Assessment
                    AssessmentScore = jobSeeker.CurrentAssessmentScore,
                    LastAssessmentDate = jobSeeker.LastAssessmentDate,

                    // Languages
                    FirstLanguage = jobSeeker.FirstLanguage == null ? null : new CandidateLanguageDto
                    {
                        Name = jobSeeker.FirstLanguage.NameEn,
                        Proficiency = jobSeeker.FirstLanguageProficiency.ToString()
                    },
                    SecondLanguage = jobSeeker.SecondLanguage == null ? null : new CandidateLanguageDto
                    {
                        Name = jobSeeker.SecondLanguage.NameEn,
                        Proficiency = jobSeeker.SecondLanguageProficiency.ToString()
                    },

                    // Collections
                    Skills = skills,
                    Experiences = experiences,
                    Educations = educations,
                    Projects = projects,
                    Certificates = certificates,
                    SocialAccounts = social,
                    Resume = resume
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting candidate profile. UserId={UserId} JobId={JobId} JobSeekerId={JobSeekerId}",
                    userId, jobId, jobSeekerId);
                return null;
            }
        }
        // ═══════════════════════════════════════════════════════════
        //  أضف الـ method دي جوّا JobService class
        //  بعد GetCandidateProfileAsync مباشرةً
        //
        //  لازم تضيف في constructor:
        //    private readonly IAiRecommendationService _aiService;
        //  وتضيفه في الـ constructor parameters بردو
        // ═══════════════════════════════════════════════════════════

        public async Task<JobRecommendationsDto?> GetAiRecommendationsAsync(
            int userId, int jobId, int maxResults = 10)
        {
            try
            {
                // 1️⃣ الوظيفة موجودة وبتاعة الـ Recruiter ده؟
                var job = await GetOwnedJobAsync(userId, jobId);
                if (job == null) return null;

                // 2️⃣ جيب الـ skills بتاعة الوظيفة
                var jobSkillNames = await _context.JobSkills
                    .AsNoTracking()
                    .Where(js => js.JobId == jobId)
                    .Join(_context.Skills,
                        js => js.SkillId,
                        s => s.Id,
                        (js, s) => s.Name)
                    .ToListAsync();

                // 3️⃣ Pre-filter: JobSeekers اللي عندهم skill واحدة على الأقل مشتركة
                var skillIds = await _context.JobSkills
                    .AsNoTracking()
                    .Where(js => js.JobId == jobId)
                    .Select(js => js.SkillId)
                    .ToListAsync();

                var matchingJobSeekerIds = await _context.JobSeekerSkills
                    .AsNoTracking()
                    .Where(jss => skillIds.Contains(jss.SkillId))
                    .Select(jss => jss.JobSeekerId)
                    .Distinct()
                    .ToListAsync();

                if (!matchingJobSeekerIds.Any())
                {
                    // مفيش candidates مناسبين — رجّع response فاضي
                    return new JobRecommendationsDto
                    {
                        JobId = job.Id,
                        JobTitle = job.Title,
                        TotalCandidatesEvaluated = 0,
                        Recommendations = new()
                    };
                }

                // 4️⃣ جيب بيانات الـ candidates المفلترين
                var candidates = await _context.JobSeekers
                    .AsNoTracking()
                    .Where(js => matchingJobSeekerIds.Contains(js.Id))
                    .Include(js => js.User)
                    .Include(js => js.JobTitle)
                    .ToListAsync();

                // 5️⃣ جيب skills لكل candidate في query واحدة
                var allCandidateSkills = await _context.JobSeekerSkills
                    .AsNoTracking()
                    .Where(jss => matchingJobSeekerIds.Contains(jss.JobSeekerId))
                    .Join(_context.Skills,
                        jss => jss.SkillId,
                        s => s.Id,
                        (jss, s) => new { jss.JobSeekerId, SkillName = s.Name })
                    .ToListAsync();

                // جيب experience details لكل candidate
                var allExperiences = await _context.Experiences
                    .AsNoTracking()
                    .Where(e => matchingJobSeekerIds.Contains(e.JobSeekerId) && !e.IsDeleted)
                    .ToListAsync();

                // جيب education لكل candidate
                var allEducations = await _context.Educations
                    .AsNoTracking()
                    .Where(e => matchingJobSeekerIds.Contains(e.JobSeekerId) && !e.IsDeleted)
                    .ToListAsync();

                // 6️⃣ ركّب الـ AI request
                var aiCandidates = candidates.Select(js =>
                {
                    var skills = allCandidateSkills
                        .Where(s => s.JobSeekerId == js.Id)
                        .Select(s => s.SkillName)
                        .ToList();

                    var experienceDetails = allExperiences
                        .Where(e => e.JobSeekerId == js.Id)
                        .OrderByDescending(e => e.StartDate)
                        .Select(e => $"{e.JobTitle} at {e.CompanyName}")
                        .ToList();

                    var educationDetails = allEducations
                        .Where(e => e.JobSeekerId == js.Id)
                        .OrderByDescending(e => e.StartDate)
                        .Select(e => $"{e.Degree} in {e.Major} from {e.Institution}")
                        .ToList();

                    return new AiCandidateInputDto
                    {
                        CandidateId = js.Id.ToString(),
                        FullName = $"{js.User.FirstName} {js.User.LastName}",
                        TotalYearsExp = js.YearsOfExperience ?? 0,
                        Bio = js.Bio ?? string.Empty,
                        ExperienceDetails = string.Join(". ", experienceDetails),
                        Skills = string.Join(", ", skills),
                        Education = string.Join(". ", educationDetails),
                        TestScore = (double)(js.CurrentAssessmentScore ?? 0)
                    };
                }).ToList();

                var aiRequest = new AiRecommendationRequestDto
                {
                    Job = new AiJobDto
                    {
                        Id = job.Id,
                        Title = job.Title,
                        Description = job.Description,
                        MinYearsOfExperience = job.MinYearsOfExperience,
                        RequiredSkills = jobSkillNames
                    },
                    MaxResults = maxResults,
                    Candidates = aiCandidates
                };

                // 7️⃣ ابعت للـ AI API
                var aiResponse = await _aiService.GetRecommendationsAsync(aiRequest);

                if (aiResponse == null)
                {
                    _logger.LogWarning("AI API returned null for JobId={JobId}", jobId);
                    return null;
                }

                // 8️⃣ ركّب الـ final response للـ Recruiter
                return new JobRecommendationsDto
                {
                    JobId = job.Id,
                    JobTitle = job.Title,
                    TotalCandidatesEvaluated = aiCandidates.Count,
                    Recommendations = aiResponse.Results.Select(r => new CandidateRecommendationDto
                    {
                        JobSeekerId = int.Parse(r.CandidateId),
                        FullName = r.FullName,
                        FinalScore = r.FinalScore,
                        MatchedSkills = r.MatchedSkills,
                        MissingSkills = r.MissingSkills,
                        Reason = r.Reason
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting AI recommendations for JobId={JobId}", jobId);
                return null;
            }
        }
    }
}