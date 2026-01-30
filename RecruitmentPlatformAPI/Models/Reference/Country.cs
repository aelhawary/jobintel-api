using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Reference
{
    /// <summary>
    /// Reference table for countries with localization support
    /// </summary>
    public class Country
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(2)]
        public string IsoCode { get; set; } = string.Empty; // ISO 3166-1 alpha-2 (e.g., "EG", "US")
        
        [Required]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string? PhoneCode { get; set; } // e.g., "+20", "+1"
        
        public bool IsActive { get; set; } = true;
        
        public int SortOrder { get; set; } = 999; // Lower values appear first
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
