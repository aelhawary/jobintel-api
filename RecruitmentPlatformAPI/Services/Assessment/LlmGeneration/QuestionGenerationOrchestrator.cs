using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.Services.Assessment.LlmGeneration
{
    public class QuestionGenerationOrchestrator
    {
        private readonly ILlmQuestionGenerator _generator;
        private readonly ILogger<QuestionGenerationOrchestrator> _logger;

        public QuestionGenerationOrchestrator(
            ILlmQuestionGenerator generator,
            ILogger<QuestionGenerationOrchestrator> logger)
        {
            _generator = generator;
            _logger = logger;
        }

        public async Task<List<LlmGeneratedQuestionDto>> GenerateForCandidateAsync(
            string jobTitle,
            string roleFamily,
            ExperienceSeniorityLevel seniorityLevel,
            int yearsOfExperience,
            List<(int SkillId, string SkillName)> technicalSkills,
            CancellationToken cancellationToken = default)
        {
            // DEBT-3: Guard against empty technical skills early — prevents TotalQuestions=30
            // mismatching a distribution that sums to only 9 (soft-skill-only output).
            // Upstream eligibility checks should already block this, but defensive programming
            // is appropriate here to surface the error with an actionable message.
            if (!technicalSkills.Any())
            {
                _logger.LogError(
                    "Cannot generate assessment: no technical skills provided for {JobTitle}",
                    jobTitle);
                throw new ArgumentException(
                    "At least one technical skill is required to generate an assessment.",
                    nameof(technicalSkills));
            }

            var distribution = BuildDistributionPlan(seniorityLevel, technicalSkills.Select(s => s.SkillName).ToList());

            // SUG-1: Sanity-check that the distribution total matches the expected question count.
            // Catches DEBT-2 (questionsPerArea mismatch) and DEBT-3 (empty skills) at runtime
            // before they manifest as confusing "Got 9/30 after N retries" generation failures.
            var distributionTotal = distribution.Sum(d => d.Count);
            if (distributionTotal != AssessmentSettings.TotalQuestionsPerAssessment)
            {
                _logger.LogError(
                    "Distribution sum mismatch: plan produces {Actual} questions, expected {Expected}. JobTitle={JobTitle}",
                    distributionTotal, AssessmentSettings.TotalQuestionsPerAssessment, jobTitle);
            }

            _logger.LogInformation(
                "Assessment V2 - Generation Distribution Plan for {JobTitle} ({Seniority}): {Distribution}",
                jobTitle, seniorityLevel, JsonSerializer.Serialize(distribution));

            var request = new GenerationRequest
            {
                JobTitle = jobTitle,
                RoleFamily = roleFamily,
                SeniorityLevel = seniorityLevel.ToString(),
                YearsOfExperience = yearsOfExperience,
                ClaimedSkillNames = technicalSkills.Select(s => s.SkillName).ToList(),
                Distribution = distribution,
                TotalQuestions = AssessmentSettings.TotalQuestionsPerAssessment
            };

            return await _generator.GenerateQuestionsAsync(request, cancellationToken);
        }

        /// <summary>
        /// Known supporting/tooling skills that should receive lower question weight.
        /// These are important but not core to the role's primary technical competency.
        /// </summary>
        private static readonly HashSet<string> SupportingToolSkills = new(StringComparer.OrdinalIgnoreCase)
        {
            "Git", "GitHub", "GitHub Actions", "GitLab", "GitLab CI", "Bitbucket",
            "Docker", "Kubernetes", "Jenkins", "CircleCI", "Travis CI",
            "Jira", "Trello", "Asana", "Confluence", "Notion",
            "VS Code", "Visual Studio", "IntelliJ", "WebStorm", "PyCharm",
            "Postman", "Swagger", "Insomnia",
            "npm", "yarn", "pnpm", "pip", "Maven", "Gradle",
            "ESLint", "Prettier", "SonarQube",
            "Figma", "Sketch", "Adobe XD",
            "Linux", "Bash", "PowerShell", "Terminal"
        };

        private List<QuestionDistributionItem> BuildDistributionPlan(ExperienceSeniorityLevel seniorityLevel, List<string> technicalSkillNames)
        {
            var plan = new List<QuestionDistributionItem>();

            // 1. Get difficulty ratios
            (double Easy, double Medium, double Hard) ratios = seniorityLevel switch
            {
                ExperienceSeniorityLevel.Junior => (0.50, 0.35, 0.15),
                ExperienceSeniorityLevel.Mid => (0.20, 0.50, 0.30),
                ExperienceSeniorityLevel.Senior => (0.10, 0.30, 0.60),
                _ => (0.20, 0.50, 0.30) // Default to Mid
            };

            // 2. Technical block — weighted distribution
            int technicalCount = AssessmentSettings.TechnicalQuestionsCount;
            int skillCount = technicalSkillNames.Count;
            if (skillCount > 0)
            {
                // Classify skills: primary (3x weight) vs supporting (1x weight)
                const int PrimaryWeight = 3;
                const int SupportingWeight = 1;

                var skillWeights = technicalSkillNames
                    .Select(s => new { Name = s, Weight = SupportingToolSkills.Contains(s) ? SupportingWeight : PrimaryWeight })
                    .ToList();

                int totalWeight = skillWeights.Sum(sw => sw.Weight);

                // Distribute questions proportionally to weight
                var quotas = new List<(string Skill, int Quota)>();
                int assignedTotal = 0;

                for (int i = 0; i < skillWeights.Count; i++)
                {
                    int quota = (int)Math.Round((double)skillWeights[i].Weight / totalWeight * technicalCount);
                    quota = Math.Max(1, quota); // Every skill gets at least 1 question
                    quotas.Add((skillWeights[i].Name, quota));
                    assignedTotal += quota;
                }

                // Drift correction — adjust to match exact technicalCount
                while (assignedTotal > technicalCount)
                {
                    // Reduce from the skill with the most questions (prefer supporting tools first)
                    var reducible = quotas
                        .Select((q, i) => new { q, i })
                        .Where(x => x.q.Quota > 1)
                        .OrderBy(x => SupportingToolSkills.Contains(x.q.Skill) ? 0 : 1)
                        .ThenByDescending(x => x.q.Quota)
                        .FirstOrDefault();
                    if (reducible == null) break;
                    quotas[reducible.i] = (reducible.q.Skill, reducible.q.Quota - 1);
                    assignedTotal--;
                }
                while (assignedTotal < technicalCount)
                {
                    // Add to the primary skill with the fewest questions
                    var expandable = quotas
                        .Select((q, i) => new { q, i })
                        .Where(x => !SupportingToolSkills.Contains(x.q.Skill))
                        .OrderBy(x => x.q.Quota)
                        .FirstOrDefault()
                        ?? quotas.Select((q, i) => new { q, i }).OrderBy(x => x.q.Quota).First();
                    quotas[expandable.i] = (expandable.q.Skill, expandable.q.Quota + 1);
                    assignedTotal++;
                }

                // Apply difficulty distribution to each skill's quota
                foreach (var (skill, quota) in quotas)
                {
                    int easyCount = (int)Math.Round(quota * ratios.Easy);
                    int hardCount = (int)Math.Round(quota * ratios.Hard);
                    int mediumCount = quota - easyCount - hardCount; // Rounding correction to Medium

                    if (easyCount > 0) plan.Add(new QuestionDistributionItem { SkillName = skill, Category = "Technical", Difficulty = "Easy", Count = easyCount });
                    if (mediumCount > 0) plan.Add(new QuestionDistributionItem { SkillName = skill, Category = "Technical", Difficulty = "Medium", Count = mediumCount });
                    if (hardCount > 0) plan.Add(new QuestionDistributionItem { SkillName = skill, Category = "Technical", Difficulty = "Hard", Count = hardCount });
                }

                _logger.LogInformation("Weighted skill distribution: {Distribution}",
                    string.Join(", ", quotas.Select(q => $"{q.Skill}={q.Quota}")));
            }

            // 3. Soft-skill block
            var softSkillAreas = AssessmentSettings.SoftSkillAreas;
            int softTotal = AssessmentSettings.SoftSkillQuestionsCount;
            int areaCount = softSkillAreas.Length;

            if (areaCount > 0)
            {
                int basePerArea = softTotal / areaCount;
                int softRemainder = softTotal % areaCount;

                for (int i = 0; i < areaCount; i++)
                {
                    // Distribute any remainder 1-question-at-a-time to the first N areas
                    int questionsForThisArea = basePerArea + (i < softRemainder ? 1 : 0);

                    int easy = (int)Math.Round(questionsForThisArea * ratios.Easy);
                    int hard = (int)Math.Round(questionsForThisArea * ratios.Hard);
                    int medium = questionsForThisArea - easy - hard;

                    if (easy > 0) plan.Add(new QuestionDistributionItem { SkillName = softSkillAreas[i], Category = "SoftSkill", Difficulty = "Easy", Count = easy });
                    if (medium > 0) plan.Add(new QuestionDistributionItem { SkillName = softSkillAreas[i], Category = "SoftSkill", Difficulty = "Medium", Count = medium });
                    if (hard > 0) plan.Add(new QuestionDistributionItem { SkillName = softSkillAreas[i], Category = "SoftSkill", Difficulty = "Hard", Count = hard });
                }
            }

            return plan;
        }
    }
}
