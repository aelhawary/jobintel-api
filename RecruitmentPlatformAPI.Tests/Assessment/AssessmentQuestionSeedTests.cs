using RecruitmentPlatformAPI.Data.Seed;
using Xunit;

namespace RecruitmentPlatformAPI.Tests.Assessment;

public class AssessmentQuestionSeedTests
{
    [Fact]
    public void GetQuestions_AllQuestionsHaveRequiredSkillId()
    {
        var questions = AssessmentQuestionSeed.GetQuestions();

        Assert.NotEmpty(questions);
        Assert.All(questions, q => Assert.True(q.SkillId > 0, $"Question {q.Id} has invalid SkillId {q.SkillId}."));
    }
}
