using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.JobSeeker;
using JobSeekerEntity = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(AppDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProjectResponseDto> AddProjectAsync(int userId, AddProjectDto dto)
        {
            try
            {
                _logger.LogInformation("Adding project for user {UserId}", userId);

                // Get user and verify they're a job seeker
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return new ProjectResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (user.AccountType != AccountType.JobSeeker)
                {
                    _logger.LogWarning("User {UserId} is not a job seeker", userId);
                    return new ProjectResponseDto
                    {
                        Success = false,
                        Message = "Only job seekers can add projects"
                    };
                }

                // Get or create JobSeeker record (backward compatibility)
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogInformation("Creating JobSeeker record for user {UserId}", userId);
                    jobSeeker = new JobSeekerEntity
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.JobSeekers.Add(jobSeeker);
                    await _context.SaveChangesAsync();
                }

                // Calculate next display order (first added = first order)
                var maxOrder = await _context.Projects
                    .Where(p => p.JobSeekerId == jobSeeker.Id && !p.IsDeleted)
                    .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;

                // Create new project
                var project = new Project
                {
                    JobSeekerId = jobSeeker.Id,
                    Title = dto.Title.Trim(),
                    TechnologiesUsed = dto.TechnologiesUsed?.Trim(),
                    Description = dto.Description?.Trim(),
                    ProjectLink = dto.ProjectLink?.Trim(),
                    DisplayOrder = maxOrder + 1,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Projects.Add(project);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Project {ProjectId} added successfully for JobSeeker {JobSeekerId}",
                    project.Id, jobSeeker.Id);

                return new ProjectResponseDto
                {
                    Success = true,
                    Message = "Project added successfully",
                    Project = MapToDto(project)
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding project for user {UserId}", userId);
                return new ProjectResponseDto
                {
                    Success = false,
                    Message = "Failed to add project. Please try again later."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding project for user {UserId}", userId);
                return new ProjectResponseDto
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
                };
            }
        }

        public async Task<ProjectResponseDto> UpdateProjectAsync(int userId, int projectId, UpdateProjectDto dto)
        {
            try
            {
                _logger.LogInformation("Updating project {ProjectId} for user {UserId}", projectId, userId);

                // Get user and verify they're a job seeker
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return new ProjectResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (user.AccountType != AccountType.JobSeeker)
                {
                    return new ProjectResponseDto
                    {
                        Success = false,
                        Message = "Only job seekers can update projects"
                    };
                }

                // Get JobSeeker record
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogWarning("JobSeeker profile not found for user {UserId}", userId);
                    return new ProjectResponseDto
                    {
                        Success = false,
                        Message = "Job seeker profile not found"
                    };
                }

                // Get project and verify ownership
                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == projectId && p.JobSeekerId == jobSeeker.Id);

                if (project == null)
                {
                    _logger.LogWarning("Project {ProjectId} not found for JobSeeker {JobSeekerId}", 
                        projectId, jobSeeker.Id);
                    return new ProjectResponseDto
                    {
                        Success = false,
                        Message = "Project not found"
                    };
                }

                // Cannot edit deleted projects
                if (project.IsDeleted)
                {
                    _logger.LogWarning("Attempted to edit deleted project {ProjectId}", projectId);
                    return new ProjectResponseDto
                    {
                        Success = false,
                        Message = "Cannot edit a deleted project"
                    };
                }

                // Update project fields
                project.Title = dto.Title.Trim();
                project.TechnologiesUsed = dto.TechnologiesUsed?.Trim();
                project.Description = dto.Description?.Trim();
                project.ProjectLink = dto.ProjectLink?.Trim();
                project.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Project {ProjectId} updated successfully", projectId);

                return new ProjectResponseDto
                {
                    Success = true,
                    Message = "Project updated successfully",
                    Project = MapToDto(project)
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating project {ProjectId}", projectId);
                return new ProjectResponseDto
                {
                    Success = false,
                    Message = "Failed to update project. Please try again later."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating project {ProjectId}", projectId);
                return new ProjectResponseDto
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectAsync(int userId, int projectId)
        {
            try
            {
                _logger.LogInformation("Deleting project {ProjectId} for user {UserId}", projectId, userId);

                // Get user and verify they're a job seeker
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<bool>(false, "User not found");
                }

                if (user.AccountType != AccountType.JobSeeker)
                {
                    return new ApiResponse<bool>(false, "Only job seekers can delete projects");
                }

                // Get JobSeeker record
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    return new ApiResponse<bool>(false, "Job seeker profile not found");
                }

                // Get project and verify ownership
                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == projectId && p.JobSeekerId == jobSeeker.Id);

                if (project == null)
                {
                    _logger.LogWarning("Project {ProjectId} not found for JobSeeker {JobSeekerId}", 
                        projectId, jobSeeker.Id);
                    return new ApiResponse<bool>(false, "Project not found");
                }

                if (project.IsDeleted)
                {
                    _logger.LogWarning("Project {ProjectId} is already deleted", projectId);
                    return new ApiResponse<bool>(false, "Project is already deleted");
                }

                // Soft delete the project
                project.IsDeleted = true;
                project.UpdatedAt = DateTime.UtcNow;

                // Reorder remaining projects to close gaps
                var remainingProjects = await _context.Projects
                    .Where(p => p.JobSeekerId == jobSeeker.Id 
                             && !p.IsDeleted 
                             && p.Id != projectId)
                    .OrderBy(p => p.DisplayOrder)
                    .ToListAsync();

                for (int i = 0; i < remainingProjects.Count; i++)
                {
                    remainingProjects[i].DisplayOrder = i + 1;
                    remainingProjects[i].UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Project {ProjectId} deleted and remaining projects reordered", projectId);

                return new ApiResponse<bool>(true, "Project deleted successfully");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting project {ProjectId}", projectId);
                return new ApiResponse<bool>(false, "Failed to delete project. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting project {ProjectId}", projectId);
                return new ApiResponse<bool>(false, "An unexpected error occurred. Please try again later.");
            }
        }

        public async Task<List<ProjectDto>> GetProjectsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting projects for user {UserId}", userId);

                // Get user
                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.AccountType != AccountType.JobSeeker)
                {
                    _logger.LogWarning("User {UserId} not found or not a job seeker", userId);
                    return new List<ProjectDto>();
                }

                // Get JobSeeker record
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(js => js.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogInformation("No JobSeeker profile found for user {UserId}", userId);
                    return new List<ProjectDto>();
                }

                // Get active projects sorted by display order
                var projects = await _context.Projects
                    .Where(p => p.JobSeekerId == jobSeeker.Id && !p.IsDeleted)
                    .OrderBy(p => p.DisplayOrder)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        TechnologiesUsed = p.TechnologiesUsed,
                        Description = p.Description,
                        ProjectLink = p.ProjectLink,
                        DisplayOrder = p.DisplayOrder,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} projects for user {UserId}", projects.Count, userId);

                return projects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects for user {UserId}", userId);
                return new List<ProjectDto>();
            }
        }

        private static ProjectDto MapToDto(Project project)
        {
            return new ProjectDto
            {
                Id = project.Id,
                Title = project.Title,
                TechnologiesUsed = project.TechnologiesUsed,
                Description = project.Description,
                ProjectLink = project.ProjectLink,
                DisplayOrder = project.DisplayOrder,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
    }
}
