using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Request DTO for adding/updating a certificate
    /// </summary>
    public class CertificateRequestDto
    {
        /// <summary>
        /// Certificate or credential title
        /// </summary>
        /// <example>AWS Solutions Architect</example>
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Organization that issued the certificate
        /// </summary>
        /// <example>Amazon Web Services</example>
        [MaxLength(150, ErrorMessage = "Issuing organization cannot exceed 150 characters")]
        public string? IssuingOrganization { get; set; }

        /// <summary>
        /// Date the certificate was issued
        /// </summary>
        /// <example>2024-01-15</example>
        public DateTime? IssueDate { get; set; }

        /// <summary>
        /// Date the certificate expires (null if no expiration)
        /// </summary>
        /// <example>2027-01-15</example>
        public DateTime? ExpirationDate { get; set; }
    }

    /// <summary>
    /// Response DTO for a single certificate
    /// </summary>
    public class CertificateDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Certificate title
        /// </summary>
        /// <example>AWS Solutions Architect</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Organization that issued the certificate
        /// </summary>
        /// <example>Amazon Web Services</example>
        public string? IssuingOrganization { get; set; }

        /// <summary>
        /// Date the certificate was issued
        /// </summary>
        public DateTime? IssueDate { get; set; }

        /// <summary>
        /// Date the certificate expires
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Whether a file is attached to this certificate
        /// </summary>
        /// <example>true</example>
        public bool HasFile { get; set; }

        /// <summary>
        /// Original filename of the attached file
        /// </summary>
        /// <example>aws-cert.pdf</example>
        public string? FileName { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long? FileSizeBytes { get; set; }

        /// <summary>
        /// Human-readable file size
        /// </summary>
        /// <example>1.2 MB</example>
        public string? FileSizeDisplay { get; set; }

        /// <summary>
        /// Download URL for the certificate file
        /// </summary>
        /// <example>/api/jobseeker/certificates/1/download</example>
        public string? DownloadUrl { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response wrapper for certificate list operations
    /// </summary>
    public class CertificateListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<CertificateDto> Certificates { get; set; } = new();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Response wrapper for single certificate operations
    /// </summary>
    public class CertificateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CertificateDto? Certificate { get; set; }

        public static CertificateResponseDto SuccessResult(CertificateDto certificate, string message = "Operation successful")
        {
            return new CertificateResponseDto { Success = true, Message = message, Certificate = certificate };
        }

        public static CertificateResponseDto FailureResult(string message)
        {
            return new CertificateResponseDto { Success = false, Message = message };
        }
    }
}
