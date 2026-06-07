namespace RecruitmentPlatformAPI.DTOs.Recruiter
{
    /// <summary>
    /// Full candidate profile as seen by a recruiter —
    /// only returned when a Recommendation exists for this job.
    /// </summary>
    public class CandidateProfileDto
    {
        // ── AI match info ──────────────────────────────────────
        public decimal MatchScore { get; set; }
        public DateTime RecommendedAt { get; set; }

        // ── Basic identity (from User + JobSeeker) ─────────────
        public int JobSeekerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? JobTitle { get; set; }
        public int? YearsOfExperience { get; set; }

        // ── Assessment ─────────────────────────────────────────
        public decimal? AssessmentScore { get; set; }
        public DateTime? LastAssessmentDate { get; set; }

        // ── Languages ──────────────────────────────────────────
        public CandidateLanguageDto? FirstLanguage { get; set; }
        public CandidateLanguageDto? SecondLanguage { get; set; }

        // ── Skills ─────────────────────────────────────────────
        public List<CandidateSkillDto> Skills { get; set; } = new();

        // ── Experience ─────────────────────────────────────────
        public List<CandidateExperienceDto> Experiences { get; set; } = new();

        // ── Education ──────────────────────────────────────────
        public List<CandidateEducationDto> Educations { get; set; } = new();

        // ── Projects ───────────────────────────────────────────
        public List<CandidateProjectDto> Projects { get; set; } = new();

        // ── Certificates ───────────────────────────────────────
        public List<CandidateCertificateDto> Certificates { get; set; } = new();

        // ── Social & Resume ────────────────────────────────────
        public CandidateSocialDto? SocialAccounts { get; set; }
        public CandidateResumeDto? Resume { get; set; }
    }

    public class CandidateLanguageDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Proficiency { get; set; }
    }

    public class CandidateSkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // "manual" | "assessment"
    }

    public class CandidateExperienceDto
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Responsibilities { get; set; }
    }

    public class CandidateEducationDto
    {
        public int Id { get; set; }
        public string Institution { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string? GradeOrGPA { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class CandidateProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TechnologiesUsed { get; set; }
        public string? ProjectLink { get; set; }
    }

    public class CandidateCertificateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? IssuingOrganization { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    public class CandidateSocialDto
    {
        public string? LinkedIn { get; set; }
        public string? Github { get; set; }
        public string? PersonalWebsite { get; set; }
        public string? Behance { get; set; }
        public string? Dribbble { get; set; }
    }

    public class CandidateResumeDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}