using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Reference
{
    public class Country
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameAr { get; set; }

        /// <summary>
        /// ISO 3166-1 alpha-2 country code (e.g. "EG", "US", "GB").
        /// Used to render flag icons on the frontend.
        /// </summary>
        /// <example>EG</example>
        [MaxLength(2)]
        public string? CountryCode { get; set; }

        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
