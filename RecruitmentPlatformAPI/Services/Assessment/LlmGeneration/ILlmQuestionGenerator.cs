using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RecruitmentPlatformAPI.Services.Assessment.LlmGeneration
{
    public class GenerationRequest
    {
        public string JobTitle { get; set; } = string.Empty;
        public string RoleFamily { get; set; } = string.Empty;
        public string SeniorityLevel { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public List<string> ClaimedSkillNames { get; set; } = new();
        public List<QuestionDistributionItem> Distribution { get; set; } = new();
        public int TotalQuestions { get; set; }
        public List<string> AlreadyCoveredTopics { get; set; } = new();
    }

    public class LlmGenerationException : Exception
    {
        public string? RawResponse { get; }

        public LlmGenerationException(string message, string? rawResponse = null, Exception? inner = null)
            : base(message, inner)
        {
            RawResponse = rawResponse;
        }
    }

    public interface ILlmQuestionGenerator
    {
        Task<List<LlmGeneratedQuestionDto>> GenerateQuestionsAsync(GenerationRequest request, CancellationToken ct);
    }
}
