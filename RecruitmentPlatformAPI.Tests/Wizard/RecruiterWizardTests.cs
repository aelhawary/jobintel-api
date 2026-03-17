using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Services.Recruiter;
using Xunit;

namespace RecruitmentPlatformAPI.Tests.Wizard;

public class RecruiterWizardTests
{
    // ─── Shared factory helpers ──────────────────────────────────────────────

    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"REC_{Guid.NewGuid()}")
            .Options);

    private static RecruiterService MakeService(AppDbContext ctx) =>
        new(ctx, NullLogger<RecruiterService>.Instance);

    private static User RecruiterUser(int id, int step = 0) => new()
    {
        Id = id, FirstName = "Sara", LastName = "Ahmed",
        Email = $"rec{id}@test.com",
        AccountType = AccountType.Recruiter,
        ProfileCompletionStep = step,
        IsActive = true, IsEmailVerified = true
    };

    private static User JobSeekerUser(int id) => new()
    {
        Id = id, FirstName = "Ali", LastName = "Hassan",
        Email = $"js{id}@test.com",
        AccountType = AccountType.JobSeeker,
        ProfileCompletionStep = 0,
        IsActive = true, IsEmailVerified = true
    };

    private static RecruiterCompanyInfoRequestDto ValidCompanyInfo() => new()
    {
        CompanyName = "Acme Corp",
        CompanySize = "51-200",
        Industry = "Technology",
        Location = "Cairo, Egypt",
        Website = null,
        LinkedIn = null,
        CompanyDescription = null
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
    public async Task GetWizardStatus_JobSeekerUser_ReturnsNotStarted()
    {
        // BUG 2 fix: job seeker calling recruiter wizard-status must get "Not Started"
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).GetWizardStatusAsync(1);

        Assert.Equal(0, result.CurrentStep);
        Assert.False(result.IsComplete);
        Assert.Equal("Not Started", result.StepName);
        Assert.Empty(result.CompletedSteps);
    }

    [Fact]
    public async Task GetWizardStatus_Step0_ReturnsNotStartedAndNotComplete()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1, step: 0));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).GetWizardStatusAsync(1);

        Assert.Equal(0, result.CurrentStep);
        Assert.False(result.IsComplete);
        Assert.Equal("Not Started", result.StepName);
        Assert.Empty(result.CompletedSteps);
    }

    [Fact]
    public async Task GetWizardStatus_Step1_ReturnsCompleteState()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1, step: 1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).GetWizardStatusAsync(1);

        Assert.Equal(1, result.CurrentStep);
        Assert.True(result.IsComplete);
        Assert.Equal("Complete", result.StepName);
        Assert.Single(result.CompletedSteps);
        Assert.Equal("Company Information", result.CompletedSteps[0]);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AdvanceWizardStepAsync
    // ═══════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-1)]
    public async Task AdvanceStep_InvalidStep_ReturnsFailure(int targetStep)
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1));
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
    public async Task AdvanceStep_JobSeekerAccount_ReturnsFailure()
    {
        // Recruiter AdvanceWizardStepAsync has an existing account-type guard
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).AdvanceWizardStepAsync(1, 1);

        Assert.False(result.Success);
        Assert.Contains("recruiter", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AdvanceStep_JobSeekerAccount_StepRemainsUnchanged()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        await MakeService(ctx).AdvanceWizardStepAsync(1, 1);

        Assert.Equal(0, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }

    [Fact]
    public async Task AdvanceStep_Step0To1_AdvancesSuccessfully()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1, step: 0));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).AdvanceWizardStepAsync(1, 1);

        Assert.True(result.Success);
        Assert.Equal(1, result.ProfileCompletionStep);
        Assert.Equal(1, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }

    [Fact]
    public async Task AdvanceStep_AlreadyAtStep1_IsIdempotent()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1, step: 1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).AdvanceWizardStepAsync(1, 1);

        Assert.True(result.Success);
        Assert.Equal(1, result.ProfileCompletionStep);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SaveCompanyInfoAsync
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SaveCompanyInfo_UserNotFound_ReturnsFailure()
    {
        using var ctx = CreateContext();
        var result = await MakeService(ctx).SaveCompanyInfoAsync(999, ValidCompanyInfo());

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SaveCompanyInfo_JobSeekerAccount_ReturnsFailure()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(JobSeekerUser(1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).SaveCompanyInfoAsync(1, ValidCompanyInfo());

        Assert.False(result.Success);
        Assert.Contains("recruiter", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveCompanyInfo_InvalidIndustry_ReturnsFailure()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1));
        await ctx.SaveChangesAsync();

        var dto = ValidCompanyInfo();
        dto.Industry = "NotAnIndustry";

        var result = await MakeService(ctx).SaveCompanyInfoAsync(1, dto);

        Assert.False(result.Success);
        Assert.Contains("industry", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveCompanyInfo_InvalidCompanySize_ReturnsFailure()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1));
        await ctx.SaveChangesAsync();

        var dto = ValidCompanyInfo();
        dto.CompanySize = "500-1000";  // not in allowed list

        var result = await MakeService(ctx).SaveCompanyInfoAsync(1, dto);

        Assert.False(result.Success);
        Assert.Contains("company size", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveCompanyInfo_ValidData_SucceedsAndAdvancesStepTo1()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1, step: 0));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).SaveCompanyInfoAsync(1, ValidCompanyInfo());

        Assert.True(result.Success);
        Assert.Equal(1, result.ProfileCompletionStep);
        Assert.Equal(1, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }

    [Fact]
    public async Task SaveCompanyInfo_ValidData_CreatesRecruiterRecord()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1, step: 0));
        await ctx.SaveChangesAsync();

        await MakeService(ctx).SaveCompanyInfoAsync(1, ValidCompanyInfo());

        var recruiter = await ctx.Recruiters.FirstOrDefaultAsync(r => r.UserId == 1);
        Assert.NotNull(recruiter);
        Assert.Equal("Acme Corp", recruiter.CompanyName);
        Assert.Equal("51-200", recruiter.CompanySize);
        Assert.Equal("Technology", recruiter.Industry);
    }

    [Fact]
    public async Task SaveCompanyInfo_CalledTwice_UpdatesExistingRecruiterRecord()
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1, step: 0));
        await ctx.SaveChangesAsync();

        await MakeService(ctx).SaveCompanyInfoAsync(1, ValidCompanyInfo());

        var updateDto = ValidCompanyInfo();
        updateDto.CompanyName = "Updated Corp";
        await MakeService(ctx).SaveCompanyInfoAsync(1, updateDto);

        var recruiterCount = await ctx.Recruiters.CountAsync(r => r.UserId == 1);
        Assert.Equal(1, recruiterCount);

        var recruiter = await ctx.Recruiters.FirstAsync(r => r.UserId == 1);
        Assert.Equal("Updated Corp", recruiter.CompanyName);
    }

    [Fact]
    public async Task SaveCompanyInfo_AlreadyComplete_DoesNotRegress()
    {
        // Saving company info again when already at step 1 must not drop to 0
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1, step: 1));
        await ctx.SaveChangesAsync();

        var result = await MakeService(ctx).SaveCompanyInfoAsync(1, ValidCompanyInfo());

        Assert.True(result.Success);
        Assert.Equal(1, result.ProfileCompletionStep);
        Assert.Equal(1, (await ctx.Users.FindAsync(1))!.ProfileCompletionStep);
    }

    [Theory]
    [InlineData("1-10")]
    [InlineData("11-50")]
    [InlineData("51-200")]
    [InlineData("201-500")]
    [InlineData("501-1000")]
    [InlineData("1000+")]
    public async Task SaveCompanyInfo_AllValidCompanySizes_Succeed(string companySize)
    {
        using var ctx = CreateContext();
        ctx.Users.Add(RecruiterUser(1));
        await ctx.SaveChangesAsync();

        var dto = ValidCompanyInfo();
        dto.CompanySize = companySize;

        var result = await MakeService(ctx).SaveCompanyInfoAsync(1, dto);

        Assert.True(result.Success);
    }
}
