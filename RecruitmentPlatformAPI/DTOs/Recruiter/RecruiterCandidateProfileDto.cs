using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.DTOs.Recruiter
{
    /// <summary>
    /// Full candidate profile returned to recruiters when viewing a specific candidate.
    /// Aggregates personal info, experience, education, projects, skills, and social links.
    /// </summary>
    public class RecruiterCandidateProfileDto
    {
        // ── Personal Info ──
        public int JobSeekerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }

        // ── Job Title & Experience ──
        public int? JobTitleId { get; set; }
        public string? JobTitle { get; set; }
        public int? YearsOfExperience { get; set; }

        // ── Location ──
        public int? CountryId { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public int? CityId { get; set; }
        public string? City { get; set; }

        // ── Languages ──
        public string? FirstLanguage { get; set; }
        public string? FirstLanguageProficiency { get; set; }
        public string? SecondLanguage { get; set; }
        public string? SecondLanguageProficiency { get; set; }

        // ── Preferences ──
        public List<WorkModel> WorkPreferences { get; set; } = new();
        public List<EmploymentType> DesiredEmploymentTypes { get; set; } = new();

        // ── Assessment ──
        public decimal? AssessmentScore { get; set; }
        public DateTime? LastAssessmentDate { get; set; }

        // ── Skills ──
        public List<RecruiterCandidateSkillDto> Skills { get; set; } = new();

        // ── Experience ──
        public List<RecruiterCandidateExperienceDto> Experiences { get; set; } = new();

        // ── Education ──
        public List<RecruiterCandidateEducationDto> Educations { get; set; } = new();

        // ── Projects ──
        public List<RecruiterCandidateProjectDto> Projects { get; set; } = new();

        // ── Social Links ──
        public RecruiterCandidateSocialDto? SocialAccounts { get; set; }

        // ── Resume ──
        public string? ResumeFileName { get; set; }
        public string? ResumeFilePath { get; set; }
        public long? ResumeFileSizeBytes { get; set; }

        // ── AI Match (from Recommendation for this job) ──
        public decimal? MatchScore { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public string? AiReasoning { get; set; }
        public bool IsShortlisted { get; set; }
    }

    public class RecruiterCandidateSkillDto
    {
        public int SkillId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Source { get; set; }
    }

    public class RecruiterCandidateExperienceDto
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? City { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Responsibilities { get; set; }
        public string DateRange { get; set; } = string.Empty;
    }

    public class RecruiterCandidateEducationDto
    {
        public int Id { get; set; }
        public string Institution { get; set; } = string.Empty;
        public Degree Degree { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? GradeOrGPA { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string DateRange { get; set; } = string.Empty;
    }

    public class RecruiterCandidateProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TechnologiesUsed { get; set; }
        public string? Description { get; set; }
        public string? ProjectLink { get; set; }
    }

    public class RecruiterCandidateSocialDto
    {
        public string? LinkedIn { get; set; }
        public string? Github { get; set; }
        public string? Behance { get; set; }
        public string? Dribbble { get; set; }
        public string? PersonalWebsite { get; set; }
    }
}
