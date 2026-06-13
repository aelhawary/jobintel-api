using System.Text.Json.Serialization;

namespace RecruitmentPlatformAPI.DTOs.Recruiter
{
    // ═══════════════════════════════════════════════════════════
    //  EXTERNAL AI MATCHING ENGINE — REQUEST DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Top-level request payload sent to the external AI matching API.
    /// </summary>
    public class AIMatchingRequest
    {
        [JsonPropertyName("job")]
        public AIJobInfo Job { get; set; } = new();

        [JsonPropertyName("maxResults")]
        public int MaxResults { get; set; } = 10;

        [JsonPropertyName("candidates")]
        public List<AICandidateInfo> Candidates { get; set; } = new();
    }

    /// <summary>
    /// Job information sent to the AI matching engine.
    /// </summary>
    public class AIJobInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("minYearsOfExperience")]
        public int MinYearsOfExperience { get; set; }

        [JsonPropertyName("required_skills")]
        public List<string> RequiredSkills { get; set; } = new();
    }

    /// <summary>
    /// Candidate information sent to the AI matching engine for scoring.
    /// </summary>
    public class AICandidateInfo
    {
        [JsonPropertyName("candidate_id")]
        public string CandidateId { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("total_years_exp")]
        public int TotalYearsExp { get; set; }

        [JsonPropertyName("bio")]
        public string? Bio { get; set; }

        [JsonPropertyName("experience_details")]
        public string? ExperienceDetails { get; set; }

        [JsonPropertyName("skills")]
        public string Skills { get; set; } = string.Empty;

        [JsonPropertyName("education")]
        public string? Education { get; set; }

        [JsonPropertyName("test_score soft&tech")]
        public decimal? TestScoreSoftTech { get; set; }
    }

    // ═══════════════════════════════════════════════════════════
    //  EXTERNAL AI MATCHING ENGINE — RESPONSE DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Top-level response from the external AI matching API.
    /// </summary>
    public class AIMatchingResponse
    {
        [JsonPropertyName("job")]
        public AIJobInfo Job { get; set; } = new();

        [JsonPropertyName("maxResults")]
        public int MaxResults { get; set; }

        [JsonPropertyName("results")]
        public List<AIResult> Results { get; set; } = new();
    }

    /// <summary>
    /// A single candidate result from the AI matching engine.
    /// </summary>
    public class AIResult
    {
        [JsonPropertyName("candidate_id")]
        public string CandidateId { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("final_score")]
        public decimal FinalScore { get; set; }

        [JsonPropertyName("matched_skills")]
        public List<string> MatchedSkills { get; set; } = new();

        [JsonPropertyName("missing_skills")]
        public List<string> MissingSkills { get; set; } = new();

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }

    // ═══════════════════════════════════════════════════════════
    //  INTERNAL API RESPONSE DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// A matched candidate returned to the recruiter after AI processing.
    /// Includes both the AI score and our internal profile data.
    /// </summary>
    public class MatchedCandidateDto
    {
        public int JobSeekerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string? JobTitle { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? CountryName { get; set; }
        public string? CityName { get; set; }
        public decimal? AssessmentScore { get; set; }
        public List<string> Skills { get; set; } = new();

        /// <summary>AI-calculated final match score (0-100).</summary>
        public decimal MatchScore { get; set; }

        /// <summary>Skills that matched between candidate and job.</summary>
        public List<string> MatchedSkills { get; set; } = new();

        /// <summary>Required skills the candidate is missing.</summary>
        public List<string> MissingSkills { get; set; } = new();

        /// <summary>AI reasoning explaining the match quality.</summary>
        public string? AiReasoning { get; set; }

        public bool IsShortlisted { get; set; }

        /// <summary>
        /// Whether the candidate has completed the platform technical assessment.
        /// False means they are visible but not yet assessed.
        /// </summary>
        public bool IsAssessed { get; set; }
    }

    /// <summary>
    /// Response DTO for the View Candidates endpoint.
    /// </summary>
    public class CandidateMatchResponseDto
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int? JobTitleId { get; set; }
        public string? JobTitleName { get; set; }
        public int TotalPreFiltered { get; set; }
        public int TotalMatched { get; set; }
        public List<MatchedCandidateDto> Candidates { get; set; } = new();
    }
}
