using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Assessment;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Assessment;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Models.JobSeeker;
using RecruitmentPlatformAPI.Models.Reference;
using RecruitmentPlatformAPI.Services.Assessment;
using Xunit;

namespace RecruitmentPlatformAPI.Tests.Assessment;

public class AssessmentServiceTests
{
    private const int UserId = 9001;
    private const int JobSeekerId = 9101;
    private const int JobTitleId = 9201;
    private const int ClaimedTechnicalSkillId = 101;
    private const int SoftSkillId = 201;

    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Assessment_{Guid.NewGuid()}")
            .Options);

    private static AssessmentService MakeService(AppDbContext ctx) =>
        new(ctx, NullLogger<AssessmentService>.Instance);

    private static async Task SeedEligibleJobSeekerAsync(
        AppDbContext ctx,
        bool includeClaimedSkill,
        int profileStep = 4,
        JobTitleRoleFamily roleFamily = JobTitleRoleFamily.Backend)
    {
        var user = new User
        {
            Id = UserId,
            FirstName = "Mona",
            LastName = "Hassan",
            Email = $"user{UserId}@test.local",
            AccountType = AccountType.JobSeeker,
            ProfileCompletionStep = profileStep,
            IsActive = true,
            IsEmailVerified = true
        };

        var jobTitle = new JobTitle
        {
            Id = JobTitleId,
            Title = "Backend Engineer",
            RoleFamily = roleFamily,
            IsActive = true
        };

        var jobSeeker = new JobSeeker
        {
            Id = JobSeekerId,
            UserId = UserId,
            JobTitleId = JobTitleId,
            YearsOfExperience = 4
        };

        ctx.Users.Add(user);
        ctx.JobTitles.Add(jobTitle);
        ctx.JobSeekers.Add(jobSeeker);

        var claimedSkill = new Skill { Id = ClaimedTechnicalSkillId, Name = "ASP.NET Core" };
        var softSkill = new Skill { Id = SoftSkillId, Name = "Communication" };
        ctx.Skills.AddRange(claimedSkill, softSkill);

        if (includeClaimedSkill)
        {
            ctx.JobSeekerSkills.Add(new JobSeekerSkill
            {
                Id = 9301,
                JobSeekerId = JobSeekerId,
                SkillId = ClaimedTechnicalSkillId,
                Source = "Self"
            });
        }

        await ctx.SaveChangesAsync();
    }

    private static async Task SeedMinimalQuestionSetAsync(AppDbContext ctx)
    {
        var optionsJson = JsonSerializer.Serialize(new[] { "A", "B", "C", "D" });

        var questions = new List<AssessmentQuestion>();
        var id = 9401;

        for (var i = 0; i < AssessmentSettings.TechnicalQuestionsCount; i++)
        {
            questions.Add(new AssessmentQuestion
            {
                Id = id++,
                QuestionText = $"Technical question {i + 1}",
                Category = QuestionCategory.Technical,
                RoleFamily = JobTitleRoleFamily.Backend,
                SkillId = ClaimedTechnicalSkillId,
                Difficulty = QuestionDifficulty.Medium,
                SeniorityLevel = ExperienceSeniorityLevel.Mid,
                Options = optionsJson,
                CorrectAnswerIndex = 0,
                IsActive = true
            });
        }

        for (var i = 0; i < AssessmentSettings.SoftSkillQuestionsCount; i++)
        {
            questions.Add(new AssessmentQuestion
            {
                Id = id++,
                QuestionText = $"Soft skill question {i + 1}",
                Category = QuestionCategory.SoftSkill,
                RoleFamily = JobTitleRoleFamily.Other,
                SkillId = SoftSkillId,
                Difficulty = QuestionDifficulty.Medium,
                SeniorityLevel = ExperienceSeniorityLevel.Mid,
                Options = optionsJson,
                CorrectAnswerIndex = 0,
                IsActive = true
            });
        }

        ctx.AssessmentQuestions.AddRange(questions);
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task CheckEligibility_WithoutClaimedSkills_ReturnsNotEligible()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: false);

        var service = MakeService(ctx);
        var result = await service.CheckEligibilityAsync(UserId);

        Assert.False(result.IsEligible);
        Assert.False(result.HasClaimedSkills);
        Assert.Contains("at least one skill", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckEligibility_PreviousAttempts_CountsOnlyCurrentAlgorithmAttempts()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);

        ctx.AssessmentAttempts.AddRange(
            new AssessmentAttempt
            {
                Id = 9501,
                JobSeekerId = JobSeekerId,
                JobTitleId = JobTitleId,
                Status = AssessmentStatus.Completed,
                StartedAt = DateTime.UtcNow.AddDays(-10),
                CompletedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(-10).AddMinutes(30),
                TimeLimitMinutes = 30,
                TotalQuestions = 3,
                QuestionsAnswered = 3,
                AlgorithmVersion = 1,  // legacy attempt — not counted
                RetakeNumber = 1,
                IsActive = false
            },
            new AssessmentAttempt
            {
                Id = 9502,
                JobSeekerId = JobSeekerId,
                JobTitleId = JobTitleId,
                Status = AssessmentStatus.Completed,
                StartedAt = DateTime.UtcNow.AddDays(-5),
                CompletedAt = DateTime.UtcNow.AddDays(-5),
                ExpiresAt = DateTime.UtcNow.AddDays(-5).AddMinutes(30),
                TimeLimitMinutes = 30,
                TotalQuestions = 3,
                QuestionsAnswered = 3,
                AlgorithmVersion = 2,
                RetakeNumber = 1,
                IsActive = false
            });

        await ctx.SaveChangesAsync();

        var service = MakeService(ctx);
        var result = await service.CheckEligibilityAsync(UserId);

        Assert.True(result.IsEligible);
        Assert.Equal(1, result.PreviousAttempts);
    }

    [Fact]
    public async Task StartAssessment_WithClaimedSkill_CreatesAttemptWithSnapshot()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var result = await service.StartAssessmentAsync(UserId);

        Assert.NotNull(result);

        var attempt = await ctx.AssessmentAttempts.SingleAsync();
        Assert.Equal(2, attempt.AlgorithmVersion);

        var claimedSnapshot = JsonSerializer.Deserialize<List<int>>(attempt.ClaimedSkillIdsJson ?? "[]");
        Assert.NotNull(claimedSnapshot);
        Assert.Contains(ClaimedTechnicalSkillId, claimedSnapshot!);

        Assert.Equal(result!.AttemptId, attempt.Id);
        Assert.Equal(1, result.ClaimedSkillsCount);
    }

    [Fact]
    public async Task GetQuestionStatuses_ReturnsAnsweredFlags()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        var statuses = await service.GetQuestionStatusesAsync(UserId);
        Assert.NotNull(statuses);
        Assert.Equal(started!.TotalQuestions, statuses!.Count);
        Assert.All(statuses, s => Assert.False(s.IsAnswered));

        var question = await service.GetQuestionByNumberAsync(UserId, 1);
        Assert.NotNull(question);

        await service.SubmitAnswerAsync(UserId, new SubmitAnswerRequestDto
        {
            QuestionId = question!.QuestionId,
            SelectedAnswerIndex = 0,
            TimeSpentSeconds = 12
        });

        var updated = await service.GetQuestionStatusesAsync(UserId);
        Assert.NotNull(updated);
        Assert.Equal(started.TotalQuestions, updated!.Count);
        Assert.True(updated[0].IsAnswered);
    }

    [Fact]
    public async Task GetQuestionByNumber_ReturnsSelectedAnswerWhenAnswered()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        var question = await service.GetQuestionByNumberAsync(UserId, 1);
        Assert.NotNull(question);

        await service.SubmitAnswerAsync(UserId, new SubmitAnswerRequestDto
        {
            QuestionId = question!.QuestionId,
            SelectedAnswerIndex = 2,
            TimeSpentSeconds = 10
        });

        var revisited = await service.GetQuestionByNumberAsync(UserId, 1);
        Assert.NotNull(revisited);
        Assert.Equal(2, revisited!.SelectedAnswerIndex);
    }

    [Fact]
    public async Task SubmitAnswer_Overwrite_DoesNotIncreaseAnsweredCount()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        var question = await service.GetQuestionByNumberAsync(UserId, 1);
        Assert.NotNull(question);

        var first = await service.SubmitAnswerAsync(UserId, new SubmitAnswerRequestDto
        {
            QuestionId = question!.QuestionId,
            SelectedAnswerIndex = 0,
            TimeSpentSeconds = 11
        });

        var second = await service.SubmitAnswerAsync(UserId, new SubmitAnswerRequestDto
        {
            QuestionId = question!.QuestionId,
            SelectedAnswerIndex = 1,
            TimeSpentSeconds = 9
        });

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first!.QuestionsAnswered, second!.QuestionsAnswered);

        var savedAnswer = await ctx.AssessmentAnswers.SingleAsync();
        Assert.Equal(1, savedAnswer.SelectedAnswerIndex);
    }

    [Fact]
    public async Task StartAssessment_WhenAnyAttemptInProgress_ReturnsNull()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        ctx.AssessmentAttempts.Add(new AssessmentAttempt
        {
            Id = 9601,
            JobSeekerId = JobSeekerId,
            JobTitleId = JobTitleId,
            Status = AssessmentStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            TimeLimitMinutes = 30,
            TotalQuestions = 3,
            QuestionsAnswered = 0,
            AlgorithmVersion = 1,  // any version blocks start
            RetakeNumber = 1,
            IsActive = false,
            QuestionIdsJson = JsonSerializer.Serialize(new[] { 9401, 9402, 9403 })
        });

        await ctx.SaveChangesAsync();

        var service = MakeService(ctx);
        var result = await service.StartAssessmentAsync(UserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CompleteAssessment_AllCorrect_ReturnsSkillScoresAndPersistsAttempt()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        var answered = 0;
        while (true)
        {
            var question = await service.GetNextQuestionAsync(UserId);
            if (question == null) break;

            var submit = await service.SubmitAnswerAsync(UserId, new SubmitAnswerRequestDto
            {
                QuestionId = question.QuestionId,
                SelectedAnswerIndex = 0,
                TimeSpentSeconds = 15
            });

            Assert.NotNull(submit);
            answered++;
        }

        Assert.Equal(started!.TotalQuestions, answered);

        var completed = await service.CompleteAssessmentAsync(UserId);

        Assert.NotNull(completed);
        Assert.Equal(100m, completed!.OverallScore);
        Assert.Equal(100m, completed.TechnicalSkillsTotalScore);
        Assert.Equal(100m, completed.SoftSkillsScore);
        Assert.True(completed.SkillScores.Count >= 2);
        Assert.Contains(completed.SkillScores, s => s.SkillId == ClaimedTechnicalSkillId && s.IsClaimedSkill);

        var attempt = await ctx.AssessmentAttempts.SingleAsync(a => a.Id == completed.AttemptId);
        Assert.Equal(AssessmentStatus.Completed, attempt.Status);
        Assert.Equal(100m, attempt.OverallScore);
    }

    [Fact]
    public async Task CompleteAssessment_PartialSubmission_CountsUnansweredAsIncorrect()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        var question = await service.GetQuestionByNumberAsync(UserId, 1);
        Assert.NotNull(question);

        await service.SubmitAnswerAsync(UserId, new SubmitAnswerRequestDto
        {
            QuestionId = question!.QuestionId,
            SelectedAnswerIndex = 0,
            TimeSpentSeconds = 6
        });

        var completed = await service.CompleteAssessmentAsync(UserId);
        Assert.NotNull(completed);
        Assert.Equal(started!.TotalQuestions, completed!.TotalQuestions);
        Assert.Equal(3.33m, completed.OverallScore);
        Assert.Equal(completed.TotalQuestions, completed.TechnicalTotal + completed.SoftSkillTotal);
    }

    [Fact]
    public async Task GetResult_AfterPartialCompletion_IncludesUnansweredQuestions()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        var question = await service.GetQuestionByNumberAsync(UserId, 1);
        Assert.NotNull(question);

        await service.SubmitAnswerAsync(UserId, new SubmitAnswerRequestDto
        {
            QuestionId = question!.QuestionId,
            SelectedAnswerIndex = 0,
            TimeSpentSeconds = 7
        });

        var completed = await service.CompleteAssessmentAsync(UserId);
        Assert.NotNull(completed);

        var result = await service.GetResultAsync(UserId, completed!.AttemptId);
        Assert.NotNull(result);
        Assert.NotNull(result!.QuestionResults);
        Assert.Equal(started!.TotalQuestions, result.QuestionResults!.Count);
        Assert.Contains(result.QuestionResults, qr => qr.SelectedAnswerIndex == null);
        Assert.Contains(result.QuestionResults, qr => !qr.IsCorrect);
    }

    [Fact]
    public async Task GetResult_AfterCompletion_IncludesQuestionResults()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        while (true)
        {
            var question = await service.GetNextQuestionAsync(UserId);
            if (question == null) break;

            await service.SubmitAnswerAsync(UserId, new SubmitAnswerRequestDto
            {
                QuestionId = question.QuestionId,
                SelectedAnswerIndex = 0,
                TimeSpentSeconds = 8
            });
        }

        var completed = await service.CompleteAssessmentAsync(UserId);
        Assert.NotNull(completed);

        var result = await service.GetResultAsync(UserId, completed!.AttemptId);

        Assert.NotNull(result);
        Assert.NotNull(result!.QuestionResults);
        Assert.Equal(started!.TotalQuestions, result.QuestionResults!.Count);
        Assert.All(result.QuestionResults, qr => Assert.True(qr.IsCorrect));
        Assert.Contains(result.QuestionResults, qr => qr.SkillId == ClaimedTechnicalSkillId);
    }

    [Fact]
    public async Task CompleteAssessment_ExpiredAttempt_BecomesCompletedAndActive()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        var inProgressAttempt = await ctx.AssessmentAttempts.SingleAsync(a => a.Id == started!.AttemptId);
        inProgressAttempt.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await ctx.SaveChangesAsync();

        var completed = await service.CompleteAssessmentAsync(UserId);
        Assert.NotNull(completed);
        Assert.Equal(AssessmentStatus.Completed.ToString(), completed!.Status);

        var savedAttempt = await ctx.AssessmentAttempts.SingleAsync(a => a.Id == started.AttemptId);
        var jobSeeker = await ctx.JobSeekers.SingleAsync(j => j.Id == JobSeekerId);

        Assert.Equal(AssessmentStatus.Completed, savedAttempt.Status);
        Assert.True(savedAttempt.IsActive);
        Assert.NotNull(savedAttempt.ScoreExpiresAt);
        Assert.Equal(0m, jobSeeker.CurrentAssessmentScore);
    }

    [Fact]
    public async Task GetCurrentStatus_ExpiredAttempt_AutoSubmitsAsCompleted()
    {
        using var ctx = CreateContext();
        await SeedEligibleJobSeekerAsync(ctx, includeClaimedSkill: true);
        await SeedMinimalQuestionSetAsync(ctx);

        var service = MakeService(ctx);
        var started = await service.StartAssessmentAsync(UserId);
        Assert.NotNull(started);

        var attempt = await ctx.AssessmentAttempts.SingleAsync(a => a.Id == started!.AttemptId);
        attempt.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await ctx.SaveChangesAsync();

        var status = await service.GetCurrentStatusAsync(UserId);
        Assert.NotNull(status);
        Assert.Equal(AssessmentStatus.Completed.ToString(), status!.Status);

        var savedAttempt = await ctx.AssessmentAttempts.SingleAsync(a => a.Id == started.AttemptId);
        Assert.Equal(AssessmentStatus.Completed, savedAttempt.Status);
        Assert.True(savedAttempt.IsActive);
    }
}
