using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Reference
{
    public class Language
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3)]
        public string IsoCode { get; set; } = string.Empty; // ISO 639-2/3 code

        [Required]
        [StringLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NameAr { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
