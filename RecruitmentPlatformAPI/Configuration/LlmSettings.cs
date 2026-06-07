namespace RecruitmentPlatformAPI.Configuration
{
    public class LlmSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "llama-3.3-70b-versatile";
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";
        public int MaxRetries { get; set; } = 5;
        public int RetryDelayMs { get; set; } = 1000;
        public int TimeoutSeconds { get; set; } = 45;

        // Gemini settings for CV parsing
        public string GeminiApiKey { get; set; } = string.Empty;
        public string GeminiModel { get; set; } = "gemini-2.5-flash";
    }
}
