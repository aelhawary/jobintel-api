using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Reference
{
    public class City
    {
        public int Id { get; set; }
        
        [Required]
        public int CountryId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameAr { get; set; }

        // Navigation property
        public Country Country { get; set; } = null!;
    }
}
