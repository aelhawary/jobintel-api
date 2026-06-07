using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RecruitmentPlatformAPI.Services.Assessment.LlmGeneration
{
    public class LlmGeneratedQuestionDto
    {
        [JsonPropertyName("questionText")]
        public string QuestionText { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; } = string.Empty;

        [JsonPropertyName("skillName")]
        public string SkillName { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public List<string> Options { get; set; } = new();

        [JsonPropertyName("correctAnswerIndex")]
        public int CorrectAnswerIndex { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        [JsonPropertyName("topicTag")]
        public string? TopicTag { get; set; }
    }

    public class QuestionDistributionItem
    {
        [JsonPropertyName("skillName")]
        public string SkillName { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
