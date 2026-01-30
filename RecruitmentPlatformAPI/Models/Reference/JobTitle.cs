using System.ComponentModel.DataAnnotations;
using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.Models.Reference
{
    /// <summary>
    /// Reference table for valid job titles
    /// </summary>
    public class JobTitle
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? Category { get; set; } // e.g., "Technology", "Design", "Finance", "Marketing"
        
        /// <summary>
        /// Role family for assessment compatibility across job title changes
        /// </summary>
        [Required]
        public JobTitleRoleFamily RoleFamily { get; set; } = JobTitleRoleFamily.Other;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
