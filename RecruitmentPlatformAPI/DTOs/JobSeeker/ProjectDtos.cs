using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// DTO for adding a new project to user profile
    /// </summary>
    public class AddProjectDto
    {
        /// <summary>
        /// Project title (required, max 150 characters)
        /// </summary>
        /// <example>E-Commerce Platform</example>
        [Required(ErrorMessage = "Project title is required")]
        [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Technologies and tools used in the project (max 300 characters)
        /// </summary>
        /// <example>React, Node.js, MongoDB, Stripe</example>
        [MaxLength(300, ErrorMessage = "Technologies cannot exceed 300 characters")]
        public string? TechnologiesUsed { get; set; }

        /// <summary>
        /// Detailed project description (max 1200 characters)
        /// </summary>
        /// <example>Full-stack e-commerce platform with payment integration and admin dashboard</example>
        [MaxLength(1200, ErrorMessage = "Description cannot exceed 1200 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Project URL or repository link (must be valid URL, max 300 characters)
        /// </summary>
        /// <example>https://github.com/username/ecommerce-platform</example>
        [MaxLength(300, ErrorMessage = "Project link cannot exceed 300 characters")]
        [Url(ErrorMessage = "Project link must be a valid URL")]
        public string? ProjectLink { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing project (same validation rules as AddProjectDto)
    /// </summary>
    public class UpdateProjectDto
    {
        /// <summary>
        /// Updated project title (required, max 150 characters)
        /// </summary>
        /// <example>E-Commerce Platform v2</example>
        [Required(ErrorMessage = "Project title is required")]
        [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Updated technologies and tools (max 300 characters)
        /// </summary>
        /// <example>React, Node.js, PostgreSQL, Stripe</example>
        [MaxLength(300, ErrorMessage = "Technologies cannot exceed 300 characters")]
        public string? TechnologiesUsed { get; set; }

        /// <summary>
        /// Updated project description (max 1200 characters)
        /// </summary>
        /// <example>Updated with PostgreSQL database and improved admin features</example>
        [MaxLength(1200, ErrorMessage = "Description cannot exceed 1200 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Updated project URL or repository link (must be valid URL, max 300 characters)
        /// </summary>
        /// <example>https://github.com/username/ecommerce-v2</example>
        [MaxLength(300, ErrorMessage = "Project link cannot exceed 300 characters")]
        [Url(ErrorMessage = "Project link must be a valid URL")]
        public string? ProjectLink { get; set; }
    }

    /// <summary>
    /// DTO for returning project data with metadata
    /// </summary>
    public class ProjectDto
    {
        /// <summary>
        /// Project unique identifier
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Project title
        /// </summary>
        /// <example>E-Commerce Platform</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Technologies and tools used
        /// </summary>
        /// <example>React, Node.js, MongoDB, Stripe</example>
        public string? TechnologiesUsed { get; set; }

        /// <summary>
        /// Project description
        /// </summary>
        /// <example>Full-stack e-commerce platform with payment integration</example>
        public string? Description { get; set; }

        /// <summary>
        /// Project URL or repository link
        /// </summary>
        /// <example>https://github.com/username/ecommerce-platform</example>
        public string? ProjectLink { get; set; }

        /// <summary>
        /// Display order for sorting (1-based, auto-assigned)
        /// </summary>
        /// <example>1</example>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Project creation timestamp (UTC)
        /// </summary>
        /// <example>2025-12-29T21:00:00Z</example>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update timestamp (UTC)
        /// </summary>
        /// <example>2025-12-29T21:00:00Z</example>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response DTO for project create/update/delete operations
    /// </summary>
    public class ProjectResponseDto
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// Operation result message
        /// </summary>
        /// <example>Project added successfully</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The project data (null on errors)
        /// </summary>
        public ProjectDto? Project { get; set; }
    }
}
