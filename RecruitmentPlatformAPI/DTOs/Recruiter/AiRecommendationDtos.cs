using System.Text.Json.Serialization;

namespace RecruitmentPlatformAPI.DTOs.Recruiter
{

    public class AiRecommendationRequestDto
    {
        [JsonPropertyName("job")]
        public AiJobDto Job { get; set; } = null!;

        [JsonPropertyName("maxResults")]
        public int MaxResults { get; set; }

        [JsonPropertyName("candidates")]
        public List<AiCandidateInputDto> Candidates { get; set; } = new();
    }

    public class AiJobDto
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

    public class AiCandidateInputDto
    {
        [JsonPropertyName("candidate_id")]
        public string CandidateId { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("total_years_exp")]
        public int TotalYearsExp { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; } = string.Empty;

        [JsonPropertyName("experience_details")]
        public string ExperienceDetails { get; set; } = string.Empty;

        [JsonPropertyName("skills")]
        public string Skills { get; set; } = string.Empty;

        [JsonPropertyName("education")]
        public string Education { get; set; } = string.Empty;

        [JsonPropertyName("test_score soft&tech")]
        public double TestScore { get; set; }
    }

    public class AiRecommendationResponseDto
    {
        [JsonPropertyName("job")]
        public AiJobDto Job { get; set; } = null!;

        [JsonPropertyName("maxResults")]
        public int MaxResults { get; set; }

        [JsonPropertyName("results")]
        public List<AiCandidateResultDto> Results { get; set; } = new();
    }

    public class AiCandidateResultDto
    {
        [JsonPropertyName("candidate_id")]
        public string CandidateId { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("final_score")]
        public double FinalScore { get; set; }

        [JsonPropertyName("matched_skills")]
        public List<string> MatchedSkills { get; set; } = new();

        [JsonPropertyName("missing_skills")]
        public List<string> MissingSkills { get; set; } = new();

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }


    public class JobRecommendationsDto
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int TotalCandidatesEvaluated { get; set; }
        public List<CandidateRecommendationDto> Recommendations { get; set; } = new();
    }

    public class CandidateRecommendationDto
    {
        public int JobSeekerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public double FinalScore { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }
}
