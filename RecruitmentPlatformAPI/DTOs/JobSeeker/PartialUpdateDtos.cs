using RecruitmentPlatformAPI.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    public class UpdateBioDto
    {
        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        public string? Bio { get; set; }
    }

    public class UpdateLanguagesDto : IValidatableObject
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "First language is required")]
        public int FirstLanguageId { get; set; }

        [Required]
        [Range(1, 4, ErrorMessage = "First language proficiency must be selected")]
        public LanguageProficiency FirstLanguageProficiency { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "If provided, second language must be valid")]
        public int? SecondLanguageId { get; set; }

        [Range(1, 4, ErrorMessage = "Second language proficiency must be a valid level")]
        public LanguageProficiency? SecondLanguageProficiency { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SecondLanguageId.HasValue && SecondLanguageId.Value == FirstLanguageId)
            {
                yield return new ValidationResult(
                    "Second language must be different from first language",
                    new[] { nameof(SecondLanguageId) });
            }

            if (SecondLanguageId.HasValue && !SecondLanguageProficiency.HasValue)
            {
                yield return new ValidationResult(
                    "Second language proficiency is required when a second language is selected",
                    new[] { nameof(SecondLanguageProficiency) });
            }

            if (!SecondLanguageId.HasValue && SecondLanguageProficiency.HasValue)
            {
                yield return new ValidationResult(
                    "Second language must be selected before choosing proficiency",
                    new[] { nameof(SecondLanguageId) });
            }
        }
    }

    public class UpdateBasicInfoDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Job title is required")]
        public int JobTitleId { get; set; }

        [Required]
        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        public int YearsOfExperience { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Country is required")]
        public int CountryId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "City is required")]
        public int CityId { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }
    }

    public class UpdatePreferencesDto
    {
        // Making them optional to allow clearing, or keeping them strictly required if the frontend enforces it.
        // Based on the prior discussion, the dashboard modals should allow saving.
        // We will keep them without [Required] to give maximum flexibility.
        public List<WorkModel> WorkPreferences { get; set; } = new List<WorkModel>();

        public List<EmploymentType> DesiredEmploymentTypes { get; set; } = new List<EmploymentType>();
    }
}
