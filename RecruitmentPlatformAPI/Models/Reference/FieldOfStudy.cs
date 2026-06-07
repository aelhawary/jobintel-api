using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Reference
{
    public class FieldOfStudy
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
