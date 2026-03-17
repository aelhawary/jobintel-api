using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Models.Reference;
using RecruitmentPlatformAPI.Services.JobSeeker;
using Xunit;

namespace RecruitmentPlatformAPI.Tests.Wizard;

public class JobSeekerWizardTests
{
    // ─── Shared factory helpers ──────────────────────────────────────────────

    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"JS_{Guid.NewGuid()}")
            .Options);

    private static JobSeekerService MakeService(AppDbContext ctx) =>
        new(ctx, NullLogger<JobSeekerService>.Instance);

    private static User JobSeekerUser(int id, int step = 0) => new()
    {
        Id = id, FirstName = "Ali", LastName = "Hassan",
        Email = $"js{id}@test.com",
        AccountType = AccountType.JobSeeker,
        ProfileCompletionStep = step,
        IsActive = true, IsEmailVerified = true
    };

    private static User RecruiterUser(int id) => new()
    {
        Id = id, FirstName = "Sara", LastName = "Ahmed",
        Email = $"rec{id}@test.com",
        AccountType = AccountType.Recruiter,
        ProfileCompletionStep = 0,
        IsActive = true, IsEmailVerified = true
    };

    /// Seeds the minimum reference rows needed by SavePersonalInfoAsync.
    private static async Task SeedReferenceData(AppDbContext ctx)
    {
        ctx.JobTitles.Add(new JobTitle { Id = 1, Title = "Software Engineer", IsActive = true });
        ctx.Countries.Add(new Country { Id = 1, NameEn = "Egypt", NameAr = "مصر", IsoCode = "EG", IsActive = true });
        ctx.Languages.Add(new Language { Id = 1, NameEn = "English", NameAr = "الإنجليزية", IsoCode = "eng", IsActive = true });
        ctx.Languages.Add(new Language { Id = 2, NameEn = "Arabic", NameAr = "العربية", IsoCode = "ara", IsActive = true });
        await ctx.SaveChangesAsync();
    }

    private static PersonalInfoRequestDto ValidPersonalInfo() => new()
    {
        JobTitleId = 1, YearsOfExperience = 3,
        CountryId = 1, City = "Cairo",
        PhoneNumber = "+201001234567",
        FirstLanguageId = 1,
        FirstLanguageProficiency = LanguageProficiency.Native
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // GetWizardStatusAsync
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetWizardStatus_UserNotFound_ReturnsNotStarted()
    {
        using var ctx = CreateContext();
        var result = await MakeService(ctx).GetWizardStatusAsync(999);

        Assert.Equal(0, result.CurrentStep);
        Assert.False(result.IsComplete);
        Assert.Equal("Not Started", result.StepName);
        Assert.Empty(result.CompletedSteps);
    }

    [Fact]
    public async Task GetWizardStatus_RecruiterUser_ReturnsNotStarted()
    {
        // BUG 2 fix: recruiter calling JS wizard-status must get "Not Started" not recruiter data
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).GetWizardStatusAsync(1);

        Assert.Equal(0, result.CurrentStep);
        Assert.False(result.IsComplete);
        Assert.Equal("Not Started", result.StepName);
        Assert.Empty(result.CompletedSteps);
    }

    [Theory]
    [InlineData(0, "Not Started", false, 0)]
    [InlineData(1, "Personal Info & CV", false, 1)]
    [InlineData(2, "Experience & Education", false, 2)]
    [InlineData(3, "Projects", false, 3)]
    [InlineData(4, "Complete", true, 4)]
    public async Task GetWizardStatus_AllSteps_ReturnCorrectState(
        int step, string expectedName, bool expectedComplete, int expectedCompletedCount)
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1, step));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).GetWizardStatusAsync(1);

        Assert.Equal(step, result.CurrentStep);
        Assert.Equal(expectedName, result.StepName);
        Assert.Equal(expectedComplete, result.IsComplete);
        Assert.Equal(expectedCompletedCount, result.CompletedSteps.Length);
    }

    [Fact]
    public async Task GetWizardStatus_Step4_ListsAllFourCompletedStepNames()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1, 4));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).GetWizardStatusAsync(1);

        Assert.Equal(new[]
        {
            "Personal Info & CV",
            "Experience & Education",
            "Projects",
            "Skills, Social Links & Certificates"
        }, result.CompletedSteps);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AdvanceWizardStepAsync
    // ═══════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    public async Task AdvanceStep_InvalidStep_ReturnsFailure(int targetStep)
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).AdvanceWizardStepAsync(1, targetStep);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AdvanceStep_UserNotFound_ReturnsFailure()
    {
        using var ctx = CreateContext();
        var result = await MakeService(ctx).AdvanceWizardStepAsync(999, 1);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AdvanceStep_RecruiterAccount_ReturnsFailure()
    {
        // BUG 1 fix: recruiter must not be able to advance the JS wizard
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).AdvanceWizardStepAsync(1, 2);

        Assert.False(result.Success);
        Assert.Contains("job seeker", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AdvanceStep_RecruiterAccount_StepRemainsUnchanged()
    {
        // BUG 1 fix: recruiter's ProfileCompletionStep must not be mutated
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1));
        await ctx.SaveChangesAsync();

        await MakeService(ctx).AdvanceWizardStepAsync(1, 4);

        var user = await ctx.Users.FindAsync(1);
        Assert.Equal(0, user!.ProfileCompletionStep);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 2, 2)]
    [InlineData(2, 3, 3)]
    [InlineData(3, 4, 4)]
    public async Task AdvanceStep_ValidForwardStep_AdvancesAndReturnsNewStep(
        int current, int target, int expected)
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1, current));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).AdvanceWizardStepAsync(1, target);

        Assert.True(result.Success);
        Assert.Equal(expected, result.ProfileCompletionStep);
        Assert.Equal(expected, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }

    [Fact]
    public async Task AdvanceStep_SameStepTwice_IsIdempotent()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1, 2));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).AdvanceWizardStepAsync(1, 2);

        Assert.True(result.Success);
        Assert.Equal(2, result.ProfileCompletionStep);
        Assert.Equal(2, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }

    [Fact]
    public async Task AdvanceStep_BackwardStep_DoesNotRegress()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1, 3));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).AdvanceWizardStepAsync(1, 1);

        Assert.True(result.Success);
        Assert.Equal(3, result.ProfileCompletionStep);
        Assert.Equal(3, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SavePersonalInfoAsync
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SavePersonalInfo_UserNotFound_ReturnsFailure()
    {
        using var ctx = CreateContext();
        var result = await MakeService(ctx).SavePersonalInfoAsync(999, ValidPersonalInfo());

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SavePersonalInfo_RecruiterAccount_ReturnsFailure()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).SavePersonalInfoAsync(1, ValidPersonalInfo());

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SavePersonalInfo_SameFirstAndSecondLanguage_ReturnsFailure()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var dto = ValidPersonalInfo();
        dto.SecondLanguageId = dto.FirstLanguageId;  // duplicate

        var result = await MakeService(ctx).SavePersonalInfoAsync(1, dto);

        Assert.False(result.Success);
        Assert.Contains("different from first language", result.Message);
    }

    [Fact]
    public async Task SavePersonalInfo_InvalidJobTitle_ReturnsFailure()
    {
        using var ctx = CreateContext();
        await SeedReferenceData(ctx);
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var dto = ValidPersonalInfo();
        dto.JobTitleId = 9999;

        var result = await MakeService(ctx).SavePersonalInfoAsync(1, dto);

        Assert.False(result.Success);
        Assert.Contains("job title", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SavePersonalInfo_InvalidCountry_ReturnsFailure()
    {
        using var ctx = CreateContext();
        await SeedReferenceData(ctx);
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var dto = ValidPersonalInfo();
        dto.CountryId = 9999;

        var result = await MakeService(ctx).SavePersonalInfoAsync(1, dto);

        Assert.False(result.Success);
        Assert.Contains("country", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SavePersonalInfo_InvalidLanguage_ReturnsFailure()
    {
        using var ctx = CreateContext();
        await SeedReferenceData(ctx);
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var dto = ValidPersonalInfo();
        dto.FirstLanguageId = 9999;

        var result = await MakeService(ctx).SavePersonalInfoAsync(1, dto);

        Assert.False(result.Success);
        Assert.Contains("language", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SavePersonalInfo_ValidData_SucceedsAndAdvancesStepTo1()
    {
        using var ctx = CreateContext();
        await SeedReferenceData(ctx);
        ctx.Users.Add(JobSeekerUser(1, step: 0));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).SavePersonalInfoAsync(1, ValidPersonalInfo());

        Assert.True(result.Success);
        Assert.Equal(1, result.ProfileCompletionStep);
        Assert.Equal(1, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }

    [Fact]
    public async Task SavePersonalInfo_ValidData_WithSecondLanguage_Succeeds()
    {
        using var ctx = CreateContext();
        await SeedReferenceData(ctx);
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var dto = ValidPersonalInfo();
        dto.SecondLanguageId = 2;
        dto.SecondLanguageProficiency = LanguageProficiency.Intermediate;

        var result = await MakeService(ctx).SavePersonalInfoAsync(1, dto);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task SavePersonalInfo_StepAlreadyAhead_DoesNotRegress()
    {
        // Saving personal info again when already at step 3 must not drop the step back to 1
        using var ctx = CreateContext();
        await SeedReferenceData(ctx);
        ctx.Users.Add(JobSeekerUser(1, step: 3));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).SavePersonalInfoAsync(1, ValidPersonalInfo());

        Assert.True(result.Success);
        Assert.Equal(3, result.ProfileCompletionStep);
        Assert.Equal(3, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }
}
