using System.Text;
using System.Text.Json;
using RecruitmentPlatformAPI.DTOs.Recruiter;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    public class AiRecommendationService : IAiRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiRecommendationService> _logger;

        private const string AiApiUrl =
            "https://alikhaled123-ai-recruitment-api.hf.space/api/recommend";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = null  // نحترم الـ JsonPropertyName attributes
        };

        public AiRecommendationService(
            HttpClient httpClient,
            ILogger<AiRecommendationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AiRecommendationResponseDto?> GetRecommendationsAsync(
            AiRecommendationRequestDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation(
                    "Calling AI API for JobId={JobId} with {Count} candidates",
                    request.Job.Id, request.Candidates.Count);

                var response = await _httpClient.PostAsync(AiApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "AI API returned {StatusCode} for JobId={JobId}",
                        response.StatusCode, request.Job.Id);
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AiRecommendationResponseDto>(
                    responseJson, JsonOptions);

                _logger.LogInformation(
                    "AI API returned {Count} results for JobId={JobId}",
                    result?.Results.Count ?? 0, request.Job.Id);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error calling AI API for JobId={JobId}", request.Job.Id);
                return null;
            }
        }
    }
}
