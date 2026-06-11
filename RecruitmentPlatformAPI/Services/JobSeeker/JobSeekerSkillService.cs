using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public class JobSeekerSkillService : IJobSeekerSkillService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<JobSeekerSkillService> _logger;

        public JobSeekerSkillService(AppDbContext context, ILogger<JobSeekerSkillService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SkillsResponseDto> GetSkillsAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.AccountType != AccountType.JobSeeker)
                    return SkillsResponseDto.FailureResult("Only job seekers can access skills");

                var jobSeeker = await _context.JobSeekers.FirstOrDefaultAsync(j => j.UserId == userId);
                if (jobSeeker == null)
                {
                    return SkillsResponseDto.SuccessResult(new List<SkillDto>(), "No skills assigned yet");
                }

                var skills = await _context.JobSeekerSkills
                    .Where(js => js.JobSeekerId == jobSeeker.Id)
                    .Include(js => js.Skill)
                    .OrderBy(js => js.Skill.Name)
                    .Select(js => new SkillDto
                    {
                        Id = js.Skill.Id,
                        Name = js.Skill.Name
                    })
                    .ToListAsync();

                return SkillsResponseDto.SuccessResult(skills,
                    skills.Count > 0 ? $"Found {skills.Count} skill(s)" : "No skills assigned");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skills for user {UserId}", userId);
                return SkillsResponseDto.FailureResult("An error occurred");
            }
        }

        public async Task<SkillsResponseDto> UpdateSkillsAsync(int userId, UpdateSkillsRequestDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.AccountType != AccountType.JobSeeker)
                    return SkillsResponseDto.FailureResult("Only job seekers can update skills");

                if (dto.SkillIds == null || dto.SkillIds.Count == 0)
                    return SkillsResponseDto.FailureResult("At least one skill must be selected");

                var jobSeeker = await GetOrCreateJobSeekerAsync(userId);
                if (jobSeeker == null)
                    return SkillsResponseDto.FailureResult("Job seeker profile not found. Please try again.");

                // Deduplicate
                var requestedIds = dto.SkillIds.Distinct().ToList();
                if (requestedIds.Count == 0)
                    return SkillsResponseDto.FailureResult("At least one skill must be selected");
                    
                if (requestedIds.Count > 25)
                    return SkillsResponseDto.FailureResult("You can select a maximum of 25 skills.");

                // Validate all skill IDs exist
                var validSkills = await _context.Skills
                    .Where(s => requestedIds.Contains(s.Id))
                    .ToListAsync();

                if (validSkills.Count != requestedIds.Count)
                {
                    var invalidIds = requestedIds.Except(validSkills.Select(s => s.Id));
                    return SkillsResponseDto.FailureResult($"Invalid skill IDs: {string.Join(", ", invalidIds)}");
                }

                // Remove all existing skills
                var existing = await _context.JobSeekerSkills
                    .Where(js => js.JobSeekerId == jobSeeker.Id)
                    .ToListAsync();

                // ── AI State Invalidation: Dirty Check ──────────────────
                // If skills changed, invalidate AI recommendations
                var existingSkillIds = existing.Select(e => e.SkillId).OrderBy(id => id).ToList();
                var incomingSkillIds = requestedIds.OrderBy(id => id).ToList();

                if (!existingSkillIds.SequenceEqual(incomingSkillIds))
                {
                    _logger.LogInformation(
                        "Skills changed for JobSeeker {JobSeekerId}. Invalidating existing AI recommendations.",
                        jobSeeker.Id);
                    var existingRecommendations = await _context.Recommendations
                        .Where(r => r.JobSeekerId == jobSeeker.Id)
                        .ToListAsync();
                    if (existingRecommendations.Any())
                    {
                        _context.Recommendations.RemoveRange(existingRecommendations);
                    }
                }

                _context.JobSeekerSkills.RemoveRange(existing);

                // Add new skills
                var newSkills = requestedIds.Select(skillId => new JobSeekerSkill
                {
                    JobSeekerId = jobSeeker.Id,
                    SkillId = skillId,
                    Source = "Self"
                }).ToList();

                _context.JobSeekerSkills.AddRange(newSkills);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated {Count} skills for user {UserId}", requestedIds.Count, userId);

                // Return the updated skill list
                var result = validSkills
                    .OrderBy(s => s.Name)
                    .Select(s => new SkillDto { Id = s.Id, Name = s.Name })
                    .ToList();

                return SkillsResponseDto.SuccessResult(result, $"Skills updated successfully ({result.Count} skill(s))");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skills for user {UserId}", userId);
                return SkillsResponseDto.FailureResult("An error occurred while updating skills");
            }
        }

        public async Task<SkillsResponseDto> ClearSkillsAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.AccountType != AccountType.JobSeeker)
                    return SkillsResponseDto.FailureResult("Only job seekers can manage skills");

                return SkillsResponseDto.FailureResult("At least one skill is required; clearing skills is not allowed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing skills for user {UserId}", userId);
                return SkillsResponseDto.FailureResult("An error occurred while clearing skills");
            }
        }

        public async Task<List<SkillDto>> GetAvailableSkillsAsync()
        {
            return await _context.Skills
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SkillDto { Id = s.Id, Name = s.Name })
                .ToListAsync();
        }

        private async Task<Models.JobSeeker.JobSeeker?> GetJobSeekerAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.AccountType != AccountType.JobSeeker)
                return null;

            return await _context.JobSeekers.FirstOrDefaultAsync(j => j.UserId == userId);
        }

        private async Task<Models.JobSeeker.JobSeeker?> GetOrCreateJobSeekerAsync(int userId)
        {
            var jobSeeker = await GetJobSeekerAsync(userId);
            if (jobSeeker != null) return jobSeeker;

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.AccountType != AccountType.JobSeeker)
                return null;

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
    }
}
