using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssessmentV1Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentAnswer");

            migrationBuilder.DropTable(
                name: "AssessmentAttempt");

            migrationBuilder.DropTable(
                name: "AssessmentQuestion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentAttempt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerId = table.Column<int>(type: "int", nullable: false),
                    JobTitleId = table.Column<int>(type: "int", nullable: false),
                    AlgorithmVersion = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ClaimedSkillIdsJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OverallScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    QuestionIdsJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QuestionsAnswered = table.Column<int>(type: "int", nullable: false),
                    ResumeCount = table.Column<int>(type: "int", nullable: false),
                    RetakeNumber = table.Column<int>(type: "int", nullable: false),
                    ScoreExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftSkillsScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TechnicalScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false)
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
                name: "AssessmentQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswerIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RoleFamily = table.Column<int>(type: "int", nullable: false),
                    SeniorityLevel = table.Column<int>(type: "int", nullable: false),
                    TimePerQuestion = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "AssessmentAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentAttemptId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    SelectedAnswerIndex = table.Column<int>(type: "int", nullable: false),
                    TimeSpentSeconds = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_AssessmentAttempt_JobSeeker_Version_Status",
                table: "AssessmentAttempt",
                columns: new[] { "JobSeekerId", "AlgorithmVersion", "Status", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_JobTitleId",
                table: "AssessmentAttempt",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "UX_AssessmentAttempt_JobSeeker_InProgress",
                table: "AssessmentAttempt",
                column: "JobSeekerId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestion_Filtering",
                table: "AssessmentQuestion",
                columns: new[] { "RoleFamily", "Category", "Difficulty", "SeniorityLevel", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestion_SkillId",
                table: "AssessmentQuestion",
                column: "SkillId");
        }
    }
}
