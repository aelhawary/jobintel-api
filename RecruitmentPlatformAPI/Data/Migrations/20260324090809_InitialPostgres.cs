using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsoCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobTitle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoleFamily = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTitle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsoCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    AuthProvider = table.Column<int>(type: "integer", nullable: false),
                    ProviderUserId = table.Column<string>(type: "text", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    AccountType = table.Column<string>(type: "text", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastFailedLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockoutReason = table.Column<string>(type: "text", nullable: true),
                    LastSuccessfulLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProfileCompletionStep = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    RoleFamily = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: true),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    SeniorityLevel = table.Column<int>(type: "integer", nullable: false),
                    Options = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CorrectAnswerIndex = table.Column<int>(type: "integer", nullable: false),
                    TimePerQuestion = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Explanation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentQuestion_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VerificationCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVerifications_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    JobTitleId = table.Column<int>(type: "integer", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FirstLanguageId = table.Column<int>(type: "integer", nullable: true),
                    FirstLanguageProficiency = table.Column<string>(type: "text", nullable: true),
                    SecondLanguageId = table.Column<int>(type: "integer", nullable: true),
                    SecondLanguageProficiency = table.Column<string>(type: "text", nullable: true),
                    Bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentAssessmentScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    LastAssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssessmentJobTitleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekers_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobSeekers_JobTitle_JobTitleId",
                        column: x => x.JobTitleId,
                        principalTable: "JobTitle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobSeekers_Language_FirstLanguageId",
                        column: x => x.FirstLanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobSeekers_Language_SecondLanguageId",
                        column: x => x.SecondLanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobSeekers_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResets_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recruiters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CompanySize = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Website = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LinkedIn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CompanyDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recruiters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recruiters_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentAttempt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    JobTitleId = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    TechnicalScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    SoftSkillsScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    QuestionsAnswered = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScoreExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RetakeNumber = table.Column<int>(type: "integer", nullable: false),
                    QuestionIdsJson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempt_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempt_JobTitle_JobTitleId",
                        column: x => x.JobTitleId,
                        principalTable: "JobTitle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IssuingOrganization = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    StoredFileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.CheckConstraint("CK_Certificate_ExpirationDateAfterIssueDate", "\"ExpirationDate\" IS NULL OR \"IssueDate\" IS NULL OR \"ExpirationDate\" >= \"IssueDate\"");
                    table.ForeignKey(
                        name: "FK_Certificates_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Educations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    Institution = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Degree = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Major = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    GradeOrGPA = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Educations", x => x.Id);
                    table.CheckConstraint("CK_Education_EndDateAfterStartDate", "\"EndDate\" IS NULL OR \"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_Educations_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EmploymentType = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    Responsibilities = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.Id);
                    table.CheckConstraint("CK_Experience_EndDateAfterStartDate", "\"EndDate\" IS NULL OR \"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_Experiences_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerSkills_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobSeekerSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TechnologiesUsed = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: true),
                    ProjectLink = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resumes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ParseStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resumes_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    LinkedIn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Github = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Behance = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Dribbble = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PersonalWebsite = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialAccounts_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecruiterId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    Requirements = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    EmploymentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinYearsOfExperience = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Recruiters_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "Recruiters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssessmentAttemptId = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedAnswerIndex = table.Column<int>(type: "integer", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    TimeSpentSeconds = table.Column<int>(type: "integer", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentAnswer_AssessmentAttempt_AssessmentAttemptId",
                        column: x => x.AssessmentAttemptId,
                        principalTable: "AssessmentAttempt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentAnswer_AssessmentQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "AssessmentQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSkills_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    JobSeekerId = table.Column<int>(type: "integer", nullable: false),
                    MatchScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_JobSeekers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "JobSeekers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recommendations_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AssessmentQuestion",
                columns: new[] { "Id", "Category", "CorrectAnswerIndex", "CreatedAt", "Difficulty", "Explanation", "IsActive", "Options", "QuestionText", "RoleFamily", "SeniorityLevel", "SkillId", "TimePerQuestion", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "REST is an architectural style for designing networked applications using HTTP requests.", true, "[\"Representational State Transfer\",\"Remote Execution Service Technology\",\"Reliable External System Transport\",\"Request-Response State Transition\"]", "What does REST stand for?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "GET is used to request data from a specified resource.", true, "[\"GET\",\"POST\",\"PUT\",\"DELETE\"]", "Which HTTP method is used to retrieve data from a server?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Indexes improve query performance by allowing faster data lookup.", true, "[\"To enforce data constraints\",\"To speed up data retrieval\",\"To encrypt sensitive data\",\"To backup data automatically\"]", "What is the purpose of a database index?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 1, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "HTTP 200 OK indicates that the request has succeeded.", true, "[\"404\",\"500\",\"200\",\"301\"]", "What status code indicates a successful HTTP request?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "JSON (JavaScript Object Notation) is a lightweight data interchange format.", true, "[\"Styling web pages\",\"Data interchange between systems\",\"Database queries\",\"User authentication\"]", "What is JSON primarily used for?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "PUT replaces the entire resource, while PATCH applies partial modifications.", true, "[\"PUT creates, PATCH deletes\",\"PUT replaces entire resource, PATCH updates partial resource\",\"They are identical\",\"PUT is faster than PATCH\"]", "What is the difference between PUT and PATCH HTTP methods?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Dependency injection is a pattern that allows dependencies to be provided rather than hard-coded.", true, "[\"A security vulnerability\",\"A design pattern for loose coupling\",\"A database optimization technique\",\"A type of API authentication\"]", "What is dependency injection?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "ACID properties ensure reliable database transactions.", true, "[\"Atomicity, Consistency, Isolation, Durability\",\"Authentication, Confidentiality, Integrity, Durability\",\"Automated, Consistent, Indexed, Distributed\",\"Access, Control, Identity, Data\"]", "What does ACID stand for in database transactions?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Middleware functions process requests/responses in the application pipeline.", true, "[\"To store user sessions\",\"To process requests between client and server\",\"To render HTML templates\",\"To manage database connections only\"]", "What is the purpose of middleware in web applications?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "N+1 occurs when code executes N additional queries to fetch related data for N records.", true, "[\"A network latency issue\",\"Executing one query for the list plus one query per item\",\"A database connection pool exhaustion\",\"An SQL syntax error\"]", "What is the N+1 query problem?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Eventual consistency means replicas will eventually be consistent, but not immediately.", true, "[\"Data is always immediately consistent\",\"Given enough time, all replicas will converge\",\"Data is never consistent\",\"Only writes are consistent\"]", "What is eventual consistency?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "An API gateway acts as a single entry point for all API calls.", true, "[\"To store API documentation\",\"To serve as a single entry point for microservices\",\"To compile API code\",\"To test API endpoints\"]", "What is the purpose of an API gateway?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Caching stores data temporarily to reduce latency and database load.", true, "[\"To encrypt data\",\"To store frequently accessed data for faster retrieval\",\"To validate user input\",\"To compress response data\"]", "What is caching used for in backend systems?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "CAP theorem states distributed systems can only guarantee 2 of the 3 properties.", true, "[\"A distributed system can have at most 2 of: Consistency, Availability, Partition tolerance\",\"A database optimization rule\",\"A caching strategy\",\"An API design principle\"]", "What is the CAP theorem?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Horizontal scaling adds more instances, vertical scaling increases resources of existing instances.", true, "[\"Horizontal adds more machines, vertical adds more power to existing machines\",\"They are the same\",\"Horizontal is cheaper always\",\"Vertical is for databases only\"]", "What is the difference between horizontal and vertical scaling?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Message queues enable asynchronous, decoupled communication between services.", true, "[\"Real-time chat only\",\"Asynchronous communication between services\",\"Database replication\",\"API versioning\"]", "What is a message queue used for?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Sharding distributes data across multiple database instances for scalability.", true, "[\"Encrypting database fields\",\"Partitioning data across multiple databases\",\"Creating database backups\",\"Indexing database tables\"]", "What is database sharding?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Saga pattern manages distributed transactions through a sequence of local transactions.", true, "[\"A logging pattern\",\"A pattern for managing distributed transactions\",\"A caching strategy\",\"An API versioning approach\"]", "What is the Saga pattern in microservices?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Circuit breaker prevents repeated calls to a failing service, allowing recovery time.", true, "[\"A network security measure\",\"A pattern to prevent cascading failures in distributed systems\",\"A database constraint\",\"An authentication method\"]", "What is circuit breaker pattern?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "CQRS separates read and write operations for better scalability and performance.", true, "[\"A caching technique\",\"Separating read and write operations into different models\",\"A database type\",\"An API versioning strategy\"]", "What is CQRS (Command Query Responsibility Segregation)?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Effective rate limiting considers multiple factors including user tiers and endpoint importance.", true, "[\"Only request count\",\"Request count, user tier, endpoint sensitivity, and time windows\",\"Only user authentication status\",\"Only server capacity\"]", "What factors would you consider when designing a rate limiting strategy?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Event sourcing persists state changes as immutable events rather than current state.", true, "[\"Logging all events\",\"Storing state changes as a sequence of events\",\"A pub/sub mechanism\",\"An API documentation approach\"]", "What is event sourcing?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 23, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Handling high concurrency requires a combination of scaling techniques.", true, "[\"Use a single powerful server\",\"Use load balancing, caching, CDN, database sharding, and async processing\",\"Just increase database size\",\"Only add more API servers\"]", "How would you design a system to handle 1 million concurrent users?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Optimistic locking checks for conflicts at commit time, pessimistic locks prevent conflicts.", true, "[\"Optimistic assumes no conflicts, pessimistic locks resources preemptively\",\"They are identical\",\"Optimistic is for reads, pessimistic for writes\",\"Optimistic is always faster\"]", "What is the difference between optimistic and pessimistic locking?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Effective cache invalidation combines TTL, pub/sub notifications, and versioning.", true, "[\"Clear all caches periodically\",\"Use TTL, pub/sub invalidation, and versioning strategies\",\"Never invalidate\",\"Only invalidate on server restart\"]", "How would you implement a distributed cache invalidation strategy?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Idempotent APIs handle retries safely without side effects.", true, "[\"Only status codes matter\",\"Idempotency keys, proper HTTP methods, and handling duplicate requests\",\"Only GET methods need to be idempotent\",\"Idempotency is not important\"]", "What considerations are important for designing idempotent APIs?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "CSS (Cascading Style Sheets) is used to style HTML documents.", true, "[\"Computer Style Sheets\",\"Cascading Style Sheets\",\"Creative Style System\",\"Content Styling Standard\"]", "What does CSS stand for?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 28, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "useState allows functional components to have state.", true, "[\"To make HTTP requests\",\"To manage component state\",\"To create routes\",\"To style components\"]", "What is the purpose of the 'useState' hook in React?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "The DOM is a programming interface for HTML documents.", true, "[\"Data Object Model\",\"Document Object Model\",\"Digital Output Method\",\"Direct Object Manipulation\"]", "What is the DOM?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "The alt attribute provides text description for screen readers and when images fail to load.", true, "[\"To make images load faster\",\"To provide alternative text for accessibility\",\"To set image dimensions\",\"To add image effects\"]", "What is the purpose of 'alt' attribute in images?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Responsive design ensures websites work well on all devices and screen sizes.", true, "[\"Fast-loading websites\",\"Design that adapts to different screen sizes\",\"Websites that respond quickly\",\"Design with animations\"]", "What is responsive design?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 32, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "The Virtual DOM is React's strategy for efficient DOM manipulation.", true, "[\"A new browser feature\",\"A lightweight copy of the actual DOM for efficient updates\",\"A CSS framework\",\"A JavaScript library\"]", "What is the Virtual DOM?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 33, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Event bubbling means events triggered on nested elements propagate up the DOM tree.", true, "[\"A CSS animation\",\"Events propagating from child to parent elements\",\"A JavaScript error\",\"A browser bug\"]", "What is event bubbling?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "'===' checks both value and type without coercion.", true, "[\"They are identical\",\"\\u0027==\\u0027 compares with type coercion, \\u0027===\\u0027 compares strictly\",\"\\u0027===\\u0027 is deprecated\",\"\\u0027==\\u0027 is for strings only\"]", "What is the difference between '==' and '===' in JavaScript?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 35, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "A closure is a function that has access to variables from its outer scope.", true, "[\"A browser window closing\",\"A function that remembers its outer scope variables\",\"A way to end loops\",\"A CSS property\"]", "What is a closure in JavaScript?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 36, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "useCallback memoizes callback functions to optimize performance.", true, "[\"To fetch data\",\"To memoize functions and prevent unnecessary re-renders\",\"To handle routing\",\"To manage global state\"]", "What is the purpose of useCallback in React?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 37, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Tree shaking eliminates dead code from the final bundle.", true, "[\"A CSS animation\",\"Removing unused code during bundling\",\"A testing technique\",\"A debugging method\"]", "What is tree shaking?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 38, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Code splitting loads code chunks on demand for better performance.", true, "[\"Writing code in multiple files\",\"Loading code on demand to reduce initial bundle size\",\"A coding style\",\"Splitting CSS and JS\"]", "What is code splitting?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 39, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "React.memo is a HOC that memoizes components based on props.", true, "[\"To store notes in components\",\"To memoize components and prevent unnecessary re-renders\",\"To manage memory\",\"To create memos\"]", "What is the purpose of React.memo?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 40, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "SSR renders HTML on the server, CSR renders in the client's browser.", true, "[\"SSR renders on server, CSR renders in browser\",\"They are identical\",\"SSR is faster always\",\"CSR is for mobile only\"]", "What is the difference between SSR and CSR?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 41, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Web Workers allow scripts to run in background threads without blocking the UI.", true, "[\"To style web pages\",\"To run scripts in background threads\",\"To handle HTTP requests\",\"To manage cookies\"]", "What is the purpose of Web Workers?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 42, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Virtualization renders only visible items, dramatically improving performance for large lists.", true, "[\"Use more useState\",\"Implement virtualization/windowing techniques\",\"Add more CSS\",\"Use setTimeout\"]", "How would you optimize a large list rendering in React?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 43, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Hydration makes server-rendered HTML interactive by attaching event handlers.", true, "[\"Adding water effects\",\"Attaching JavaScript event handlers to server-rendered HTML\",\"A CSS property\",\"A database technique\"]", "What is hydration in the context of SSR?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 44, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "The Critical Rendering Path is the sequence from receiving HTML to displaying pixels.", true, "[\"The most important CSS rules\",\"The sequence of steps browser takes to render a page\",\"A JavaScript function\",\"A routing pattern\"]", "What is the Critical Rendering Path?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 45, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Micro-frontends allow independent teams to develop and deploy separate frontend modules.", true, "[\"Use only one framework\",\"Use module federation, independent deployments, and shared dependencies\",\"Avoid using any frameworks\",\"Use iframes only\"]", "How would you implement a micro-frontend architecture?", 1, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 46, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Large apps benefit from a layered approach combining different state management strategies.", true, "[\"Use only local state\",\"Combine local, global state, server state caching, and derived state appropriately\",\"Use only Redux\",\"Avoid state management\"]", "What strategies would you use for state management in a large application?", 1, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 47, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Performance optimization requires profiling, identifying bottlenecks, and applying targeted fixes.", true, "[\"Add more components\",\"Profile, analyze renders, memoize, split code, lazy load, and optimize network\",\"Remove all styling\",\"Use class components only\"]", "How would you approach performance optimization for a slow React application?", 1, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 48, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "GraphQL provides flexible queries while REST has fixed endpoints.", true, "[\"They are identical\",\"GraphQL allows clients to request specific data, REST returns fixed responses\",\"REST is newer than GraphQL\",\"GraphQL is only for databases\"]", "What is GraphQL and how does it differ from REST?", 3, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 49, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "JWT (JSON Web Token) enables stateless authentication between client and server.", true, "[\"A JavaScript testing tool\",\"A stateless authentication token for API security\",\"A CSS framework\",\"A database type\"]", "What is a JWT and when would you use it?", 3, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 50, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "CORS controls which origins can access resources from a different domain.", true, "[\"A CSS feature\",\"A security mechanism controlling cross-origin requests\",\"A JavaScript library\",\"A database constraint\"]", "What is CORS and why is it important?", 3, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 51, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Real-time features typically use WebSockets or SSE for bi-directional or server-push communication.", true, "[\"Refresh the page frequently\",\"Use WebSockets, Server-Sent Events, or long polling\",\"Only use REST APIs\",\"Disable caching\"]", "How would you implement real-time features in a web application?", 3, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 52, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Robust authentication requires multiple security layers and considerations.", true, "[\"Only password strength\",\"Secure storage, token management, MFA, session handling, and rate limiting\",\"Only using HTTPS\",\"Only email verification\"]", "What considerations are important when designing an authentication system?", 3, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 53, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Honesty and willingness to learn are valued traits in collaborative environments.", true, "[\"Pretend you know and try anyway\",\"Honestly say you\\u0027re unfamiliar but offer to learn together or find help\",\"Ignore the request\",\"Tell them to ask someone else\"]", "A colleague asks for help with a task you're unfamiliar with. What should you do?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 54, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Constructive criticism is an opportunity for growth when received with an open mind.", true, "[\"Get defensive and argue\",\"Listen carefully, ask clarifying questions, and use it to improve\",\"Ignore it completely\",\"Criticize the reviewer\\u0027s work in return\"]", "How should you handle constructive criticism of your work?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 55, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Effective communication adapts to the audience's level of understanding.", true, "[\"Use as much jargon as possible\",\"Use analogies and simple language avoiding technical terms\",\"Show them the code\",\"Tell them it\\u0027s too complex to explain\"]", "What is the best way to communicate a technical concept to a non-technical person?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 56, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Transparency and quick action when mistakes occur build trust and minimize impact.", true, "[\"Hide it and hope no one notices\",\"Inform your team immediately and work on a fix\",\"Blame someone else\",\"Wait until someone reports it\"]", "You realize you made a mistake in production code. What should you do first?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 57, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Effective prioritization considers multiple factors beyond perceived urgency.", true, "[\"Work on whatever is easiest first\",\"Assess impact, deadlines, and dependencies to determine true priorities\",\"Ask someone else to decide\",\"Work overtime on everything\"]", "How do you prioritize tasks when everything seems urgent?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 58, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Private, specific, and constructive feedback helps team members improve.", true, "[\"Rewrite their code without telling them\",\"Provide constructive feedback privately with specific suggestions\",\"Complain to the manager immediately\",\"Ignore it\"]", "A team member's code doesn't meet standards. How do you address this?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 59, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Breaking down large projects and acknowledging progress helps maintain motivation.", true, "[\"Just push through regardless of burnout\",\"Break into milestones, celebrate progress, and take regular breaks\",\"Complain frequently\",\"Switch to other projects\"]", "How do you stay motivated when working on a long, complex project?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 60, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Respectful disagreement with reasoning contributes to better decisions.", true, "[\"Stay silent and follow orders\",\"Respectfully present your concerns with supporting reasoning in private\",\"Argue publicly in meetings\",\"Implement your approach anyway\"]", "You disagree with a technical decision made by a senior developer. What do you do?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 61, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Good code reviews balance quality enforcement with constructive learning.", true, "[\"Finding as many problems as possible\",\"Being constructive, specific, and focused on code quality and learning\",\"Approving everything quickly\",\"Only checking for syntax errors\"]", "What makes a good code review?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 62, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Direct, empathetic communication often resolves interpersonal difficulties.", true, "[\"Avoid them completely\",\"Try to understand their perspective and communicate directly about issues\",\"Complain to others\",\"Request a team change immediately\"]", "How do you handle working with a difficult team member?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 63, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Balancing debt and features requires continuous assessment and communication.", true, "[\"Always prioritize new features\",\"Assess impact, allocate regular time for debt, and communicate tradeoffs\",\"Never work on new features until debt is cleared\",\"Technical debt doesn\\u0027t matter\"]", "How do you balance technical debt against new feature development?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 64, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Proactive, supportive assistance helps team members without undermining their autonomy.", true, "[\"Wait for them to ask\",\"Offer help in a supportive, non-judgmental way\",\"Report them to the manager\",\"Take over their work\"]", "You notice a team member struggling but not asking for help. What do you do?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 65, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Managing scope creep requires documentation, impact assessment, and negotiation.", true, "[\"Accept all changes without question\",\"Document changes, assess impact, and negotiate timeline or resource adjustments\",\"Refuse all changes\",\"Work overtime to include everything\"]", "How do you handle scope creep in a project?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 66, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Proactive, data-driven communication about constraints enables informed decisions.", true, "[\"Promise to meet it anyway\",\"Present data-driven concerns early with alternative proposals\",\"Miss the deadline and explain later\",\"Quit the project\"]", "A project deadline is clearly unrealistic. How do you handle this?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 67, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Effective mentoring balances guidance with opportunities for independent growth.", true, "[\"Tell them exactly what to do\",\"Guide with questions, provide resources, give autonomy, and offer regular feedback\",\"Let them figure everything out alone\",\"Do their work for them\"]", "How do you mentor a junior developer effectively?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 68, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Influence without authority requires relationship building and persuasive communication.", true, "[\"Force your opinion\",\"Build relationships, present data, understand stakeholders, and find common ground\",\"Give up trying\",\"Escalate immediately\"]", "How do you influence decisions when you don't have direct authority?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 69, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Professional disagreement followed by committed execution maintains trust.", true, "[\"Comply silently\",\"Voice concerns diplomatically, disagree and commit if overruled\",\"Refuse to implement\",\"Undermine the decision quietly\"]", "How do you handle technical decisions you disagree with from leadership?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 70, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Innovation requires psychological safety and dedicated time for experimentation.", true, "[\"Assign innovation tasks\",\"Create psychological safety, allocate time for experimentation, celebrate learning from failures\",\"Hire more people\",\"Wait for good ideas to emerge\"]", "How do you foster innovation in an engineering team?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 71, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Declining performance requires diagnosis, support, and systemic improvements.", true, "[\"Work everyone harder\",\"Diagnose root causes, address issues individually and systemically, adjust processes\",\"Replace team members immediately\",\"Ignore it and hope it improves\"]", "How do you handle a situation where team performance is declining?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 72, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Business cases for DevEx require quantified benefits and demonstrable outcomes.", true, "[\"Just request budget\",\"Quantify productivity gains, connect to business outcomes, and pilot small improvements\",\"Complain until approved\",\"Implement without approval\"]", "How do you make a case for investing in developer experience improvements?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 73, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Addressing departures requires understanding, respect, and knowledge transfer planning.", true, "[\"Let them go without discussion\",\"Understand their reasons, address what you can, plan for transitions respectfully\",\"Offer more money immediately\",\"Make them feel guilty\"]", "A key team member wants to leave. How do you approach this situation?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsoCode", "NameAr", "NameEn", "PhoneCode", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "EG", "مصر", "Egypt", "+20", 1 },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SA", "المملكة العربية السعودية", "Saudi Arabia", "+966", 2 },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "AE", "الإمارات العربية المتحدة", "United Arab Emirates", "+971", 3 },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "KW", "الكويت", "Kuwait", "+965", 4 },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "QA", "قطر", "Qatar", "+974", 5 },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "BH", "البحرين", "Bahrain", "+973", 6 },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "OM", "عمان", "Oman", "+968", 7 },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "JO", "الأردن", "Jordan", "+962", 8 },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "LB", "لبنان", "Lebanon", "+961", 9 },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "IQ", "العراق", "Iraq", "+964", 10 },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SY", "سوريا", "Syria", "+963", 11 },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "YE", "اليمن", "Yemen", "+967", 12 },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "PS", "فلسطين", "Palestine", "+970", 13 },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "LY", "ليبيا", "Libya", "+218", 14 },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "TN", "تونس", "Tunisia", "+216", 15 },
                    { 16, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "DZ", "الجزائر", "Algeria", "+213", 16 },
                    { 17, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "MA", "المغرب", "Morocco", "+212", 17 },
                    { 18, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SD", "السودان", "Sudan", "+249", 18 },
                    { 19, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "US", "الولايات المتحدة", "United States", "+1", 100 },
                    { 20, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "GB", "المملكة المتحدة", "United Kingdom", "+44", 101 },
                    { 21, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "CA", "كندا", "Canada", "+1", 102 },
                    { 22, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "AU", "أستراليا", "Australia", "+61", 103 },
                    { 23, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "DE", "ألمانيا", "Germany", "+49", 104 },
                    { 24, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "FR", "فرنسا", "France", "+33", 105 },
                    { 25, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "IT", "إيطاليا", "Italy", "+39", 106 },
                    { 26, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ES", "إسبانيا", "Spain", "+34", 107 },
                    { 27, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "NL", "هولندا", "Netherlands", "+31", 108 },
                    { 28, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SE", "السويد", "Sweden", "+46", 109 },
                    { 29, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "NO", "النرويج", "Norway", "+47", 110 },
                    { 30, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "CH", "سويسرا", "Switzerland", "+41", 111 },
                    { 31, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "CN", "الصين", "China", "+86", 200 },
                    { 32, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "JP", "اليابان", "Japan", "+81", 201 },
                    { 33, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "IN", "الهند", "India", "+91", 202 },
                    { 34, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "KR", "كوريا الجنوبية", "South Korea", "+82", 203 },
                    { 35, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SG", "سنغافورة", "Singapore", "+65", 204 },
                    { 36, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "MY", "ماليزيا", "Malaysia", "+60", 205 },
                    { 37, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "TH", "تايلاند", "Thailand", "+66", 206 },
                    { 38, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "PH", "الفلبين", "Philippines", "+63", 207 },
                    { 39, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ID", "إندونيسيا", "Indonesia", "+62", 208 },
                    { 40, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "VN", "فيتنام", "Vietnam", "+84", 209 },
                    { 41, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "PK", "باكستان", "Pakistan", "+92", 210 },
                    { 42, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "BD", "بنغلاديش", "Bangladesh", "+880", 211 },
                    { 43, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "TR", "تركيا", "Turkey", "+90", 212 },
                    { 44, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "IR", "إيران", "Iran", "+98", 214 },
                    { 45, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ZA", "جنوب أفريقيا", "South Africa", "+27", 300 },
                    { 46, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "NG", "نيجيريا", "Nigeria", "+234", 301 },
                    { 47, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "KE", "كينيا", "Kenya", "+254", 302 },
                    { 48, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ET", "إثيوبيا", "Ethiopia", "+251", 303 },
                    { 49, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "GH", "غانا", "Ghana", "+233", 304 },
                    { 50, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "BR", "البرازيل", "Brazil", "+55", 400 },
                    { 51, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "MX", "المكسيك", "Mexico", "+52", 401 },
                    { 52, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "AR", "الأرجنتين", "Argentina", "+54", 402 },
                    { 53, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "CL", "تشيلي", "Chile", "+56", 403 },
                    { 54, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "CO", "كولومبيا", "Colombia", "+57", 404 },
                    { 55, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "PL", "بولندا", "Poland", "+48", 500 },
                    { 56, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "RO", "رومانيا", "Romania", "+40", 501 },
                    { 57, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "GR", "اليونان", "Greece", "+30", 502 },
                    { 58, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "PT", "البرتغال", "Portugal", "+351", 503 },
                    { 59, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "BE", "بلجيكا", "Belgium", "+32", 504 },
                    { 60, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "AT", "النمسا", "Austria", "+43", 505 },
                    { 61, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "DK", "الدنمارك", "Denmark", "+45", 506 },
                    { 62, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "FI", "فنلندا", "Finland", "+358", 507 },
                    { 63, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "IE", "أيرلندا", "Ireland", "+353", 508 },
                    { 64, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "NZ", "نيوزيلندا", "New Zealand", "+64", 509 },
                    { 65, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "RU", "روسيا", "Russia", "+7", 510 }
                });

            migrationBuilder.InsertData(
                table: "JobTitle",
                columns: new[] { "Id", "Category", "CreatedAt", "IsActive", "RoleFamily", "Title" },
                values: new object[,]
                {
                    { 1, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, "Backend Developer" },
                    { 2, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, "Frontend Developer" },
                    { 3, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, "Full Stack Developer" },
                    { 4, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 4, "Mobile Developer" },
                    { 5, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 4, "iOS Developer" },
                    { 6, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 4, "Android Developer" },
                    { 7, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, "DevOps Engineer" },
                    { 8, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, "Data Scientist" },
                    { 9, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, "Data Engineer" },
                    { 10, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, "Machine Learning Engineer" },
                    { 11, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, "AI Engineer" },
                    { 12, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, "Software Engineer" },
                    { 13, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 7, "QA Engineer" },
                    { 14, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 7, "Test Automation Engineer" },
                    { 15, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, "Cloud Engineer" },
                    { 16, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, "Security Engineer" },
                    { 17, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, "Cybersecurity Analyst" },
                    { 18, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, "Network Engineer" },
                    { 19, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, "Systems Administrator" },
                    { 20, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, "Database Administrator" },
                    { 21, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, "Solutions Architect" },
                    { 22, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, "Technical Architect" },
                    { 23, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 6, "Site Reliability Engineer" },
                    { 24, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Embedded Systems Engineer" },
                    { 25, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Game Developer" },
                    { 26, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, "Blockchain Developer" },
                    { 27, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "IoT Engineer" },
                    { 28, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, "Computer Vision Engineer" },
                    { 29, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, "NLP Engineer" },
                    { 30, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, "Business Intelligence Analyst" },
                    { 31, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 5, "Data Analyst" },
                    { 32, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "IT Support Specialist" },
                    { 33, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Technical Support Engineer" },
                    { 34, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "IT Manager" },
                    { 35, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "CTO" },
                    { 36, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Engineering Manager" },
                    { 37, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, "Technical Lead" },
                    { 38, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Scrum Master" },
                    { 39, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Product Manager" },
                    { 40, "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Technical Product Manager" },
                    { 41, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "UX Designer" },
                    { 42, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "UI Designer" },
                    { 43, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "UX/UI Designer" },
                    { 44, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Graphic Designer" },
                    { 45, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Web Designer" },
                    { 46, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Visual Designer" },
                    { 47, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Product Designer" },
                    { 48, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Interaction Designer" },
                    { 49, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Motion Designer" },
                    { 50, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "3D Designer" },
                    { 51, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Game Designer" },
                    { 52, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "UX Researcher" },
                    { 53, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Creative Director" },
                    { 54, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Art Director" },
                    { 55, "Design", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 8, "Brand Designer" },
                    { 56, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Digital Marketing Specialist" },
                    { 57, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "SEO Specialist" },
                    { 58, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Content Marketing Manager" },
                    { 59, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Social Media Manager" },
                    { 60, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Marketing Manager" },
                    { 61, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Brand Manager" },
                    { 62, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Growth Manager" },
                    { 63, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Email Marketing Specialist" },
                    { 64, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Marketing Analyst" },
                    { 65, "Marketing", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Content Writer" },
                    { 66, "Sales", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Sales Representative" },
                    { 67, "Sales", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Account Executive" },
                    { 68, "Sales", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Sales Manager" },
                    { 69, "Sales", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Business Development Manager" },
                    { 70, "Sales", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Customer Success Manager" },
                    { 71, "Finance", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Accountant" },
                    { 72, "Finance", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Financial Analyst" },
                    { 73, "Finance", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Finance Manager" },
                    { 74, "Finance", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "CFO" },
                    { 75, "Finance", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Investment Analyst" },
                    { 76, "Human Resources", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "HR Manager" },
                    { 77, "Human Resources", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Recruiter" },
                    { 78, "Human Resources", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Talent Acquisition Specialist" },
                    { 79, "Human Resources", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "HR Business Partner" },
                    { 80, "Human Resources", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "People Operations Manager" },
                    { 81, "Operations", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Operations Manager" },
                    { 82, "Operations", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Project Manager" },
                    { 83, "Operations", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Program Manager" },
                    { 84, "Operations", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Supply Chain Manager" },
                    { 85, "Operations", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "Logistics Coordinator" },
                    { 86, "Executive", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "CEO" },
                    { 87, "Executive", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "COO" },
                    { 88, "Executive", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "VP of Engineering" },
                    { 89, "Executive", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "VP of Product" },
                    { 90, "Executive", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 9, "VP of Sales" }
                });

            migrationBuilder.InsertData(
                table: "Language",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsoCode", "NameAr", "NameEn", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ara", "العربية", "Arabic", 1 },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "eng", "الإنجليزية", "English", 2 },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "tur", "التركية", "Turkish", 10 },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "fas", "الفارسية", "Persian (Farsi)", 11 },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "heb", "العبرية", "Hebrew", 12 },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "kur", "الكردية", "Kurdish", 13 },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "urd", "الأردية", "Urdu", 14 },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "fra", "الفرنسية", "French", 20 },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "deu", "الألمانية", "German", 21 },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "spa", "الإسبانية", "Spanish", 22 },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ita", "الإيطالية", "Italian", 23 },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "por", "البرتغالية", "Portuguese", 24 },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "rus", "الروسية", "Russian", 25 },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "nld", "الهولندية", "Dutch", 26 },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "pol", "البولندية", "Polish", 27 },
                    { 16, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ukr", "الأوكرانية", "Ukrainian", 28 },
                    { 17, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "swe", "السويدية", "Swedish", 29 },
                    { 18, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "nor", "النرويجية", "Norwegian", 30 },
                    { 19, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "dan", "الدنماركية", "Danish", 31 },
                    { 20, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "fin", "الفنلندية", "Finnish", 32 },
                    { 21, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ell", "اليونانية", "Greek", 33 },
                    { 22, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "zho", "الصينية (الماندرين)", "Chinese (Mandarin)", 40 },
                    { 23, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "jpn", "اليابانية", "Japanese", 41 },
                    { 24, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "kor", "الكورية", "Korean", 42 },
                    { 25, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "hin", "الهندية", "Hindi", 43 },
                    { 26, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ben", "البنغالية", "Bengali", 44 },
                    { 27, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "vie", "الفيتنامية", "Vietnamese", 45 },
                    { 28, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "tha", "التايلاندية", "Thai", 46 },
                    { 29, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ind", "الإندونيسية", "Indonesian", 47 },
                    { 30, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "msa", "الماليزية", "Malay", 48 },
                    { 31, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "fil", "الفلبينية (تاغالوغ)", "Filipino (Tagalog)", 49 },
                    { 32, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "swa", "السواحيلية", "Swahili", 50 },
                    { 33, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "amh", "الأمهرية", "Amharic", 51 },
                    { 34, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "hau", "الهوسا", "Hausa", 52 },
                    { 35, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "yor", "اليوروبا", "Yoruba", 53 },
                    { 36, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "zul", "الزولو", "Zulu", 54 },
                    { 37, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ron", "الرومانية", "Romanian", 60 },
                    { 38, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "ces", "التشيكية", "Czech", 61 },
                    { 39, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "hun", "المجرية", "Hungarian", 62 },
                    { 40, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "bul", "البلغارية", "Bulgarian", 63 },
                    { 41, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "hrv", "الكرواتية", "Croatian", 64 },
                    { 42, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "srp", "الصربية", "Serbian", 65 },
                    { 43, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "slk", "السلوفاكية", "Slovak", 66 },
                    { 44, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "slv", "السلوفينية", "Slovenian", 67 },
                    { 45, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "lit", "الليتوانية", "Lithuanian", 68 },
                    { 46, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "lav", "اللاتفية", "Latvian", 69 },
                    { 47, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "est", "الإستونية", "Estonian", 70 },
                    { 48, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "cat", "الكتالونية", "Catalan", 71 },
                    { 49, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "glg", "الجاليكية", "Galician", 72 },
                    { 50, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "eus", "الباسكية", "Basque", 73 }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "C#" },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "JavaScript" },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "TypeScript" },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Python" },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Java" },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "C++" },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PHP" },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ruby" },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Go" },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Swift" },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "React" },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Angular" },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vue.js" },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Next.js" },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HTML/CSS" },
                    { 16, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tailwind CSS" },
                    { 17, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ASP.NET Core" },
                    { 18, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Node.js" },
                    { 19, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Django" },
                    { 20, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Spring Boot" },
                    { 21, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Express.js" },
                    { 22, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Flask" },
                    { 23, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SQL Server" },
                    { 24, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PostgreSQL" },
                    { 25, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MySQL" },
                    { 26, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MongoDB" },
                    { 27, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Redis" },
                    { 28, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Entity Framework" },
                    { 29, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AWS" },
                    { 30, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Azure" },
                    { 31, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Docker" },
                    { 32, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kubernetes" },
                    { 33, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CI/CD" },
                    { 34, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Git" },
                    { 35, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Linux" },
                    { 36, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "React Native" },
                    { 37, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Flutter" },
                    { 38, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Android" },
                    { 39, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "iOS" },
                    { 40, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Machine Learning" },
                    { 41, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Data Analysis" },
                    { 42, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Power BI" },
                    { 43, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "REST APIs" },
                    { 44, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "GraphQL" },
                    { 45, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Agile/Scrum" },
                    { 46, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unit Testing" },
                    { 47, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Problem Solving" },
                    { 48, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Communication" },
                    { 49, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Project Management" },
                    { 50, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UI/UX Design" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswer_Attempt_Question",
                table: "AssessmentAnswer",
                columns: new[] { "AssessmentAttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswer_QuestionId",
                table: "AssessmentAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_JobSeeker_Active",
                table: "AssessmentAttempt",
                columns: new[] { "JobSeekerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_JobSeeker_Status",
                table: "AssessmentAttempt",
                columns: new[] { "JobSeekerId", "Status", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_JobSeeker_Status_Unique",
                table: "AssessmentAttempt",
                columns: new[] { "JobSeekerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_JobTitleId",
                table: "AssessmentAttempt",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestion_Filtering",
                table: "AssessmentQuestion",
                columns: new[] { "RoleFamily", "Category", "Difficulty", "SeniorityLevel", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestion_SkillId",
                table: "AssessmentQuestion",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_JobSeekerId_IsDeleted",
                table: "Certificates",
                columns: new[] { "JobSeekerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Country_IsoCode",
                table: "Country",
                column: "IsoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Education_JobSeekerId_IsDeleted",
                table: "Educations",
                columns: new[] { "JobSeekerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_UserId",
                table: "EmailVerifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Experience_JobSeekerId_IsDeleted",
                table: "Experiences",
                columns: new[] { "JobSeekerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RecruiterId",
                table: "Jobs",
                column: "RecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekers_CountryId",
                table: "JobSeekers",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekers_FirstLanguageId",
                table: "JobSeekers",
                column: "FirstLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekers_JobTitleId",
                table: "JobSeekers",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekers_SecondLanguageId",
                table: "JobSeekers",
                column: "SecondLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekers_UserId",
                table: "JobSeekers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerSkills_JobSeekerId_SkillId",
                table: "JobSeekerSkills",
                columns: new[] { "JobSeekerId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerSkills_SkillId",
                table: "JobSeekerSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSkills_JobId_SkillId",
                table: "JobSkills",
                columns: new[] { "JobId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSkills_SkillId",
                table: "JobSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitle_Title",
                table: "JobTitle",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Language_IsoCode",
                table: "Language",
                column: "IsoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId",
                table: "PasswordResets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_JobSeekerId_IsDeleted",
                table: "Projects",
                columns: new[] { "JobSeekerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_JobId_JobSeekerId",
                table: "Recommendations",
                columns: new[] { "JobId", "JobSeekerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_JobSeekerId",
                table: "Recommendations",
                column: "JobSeekerId");

            migrationBuilder.CreateIndex(
                name: "IX_Recruiters_UserId",
                table: "Recruiters",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resume_JobSeekerId_IsDeleted",
                table: "Resumes",
                columns: new[] { "JobSeekerId", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialAccounts_JobSeekerId",
                table: "SocialAccounts",
                column: "JobSeekerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentAnswer");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Educations");

            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropTable(
                name: "Experiences");

            migrationBuilder.DropTable(
                name: "JobSeekerSkills");

            migrationBuilder.DropTable(
                name: "JobSkills");

            migrationBuilder.DropTable(
                name: "PasswordResets");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Resumes");

            migrationBuilder.DropTable(
                name: "SocialAccounts");

            migrationBuilder.DropTable(
                name: "AssessmentAttempt");

            migrationBuilder.DropTable(
                name: "AssessmentQuestion");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "JobSeekers");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Recruiters");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "JobTitle");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
