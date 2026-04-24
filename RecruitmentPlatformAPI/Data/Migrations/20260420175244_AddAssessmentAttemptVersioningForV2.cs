using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentAttemptVersioningForV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlgorithmVersion",
                table: "AssessmentAttempt",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ClaimedSkillIdsJson",
                table: "AssessmentAttempt",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_JobSeeker_Version_Status",
                table: "AssessmentAttempt",
                columns: new[] { "JobSeekerId", "AlgorithmVersion", "Status", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssessmentAttempt_JobSeeker_Version_Status",
                table: "AssessmentAttempt");

            migrationBuilder.DropColumn(
                name: "AlgorithmVersion",
                table: "AssessmentAttempt");

            migrationBuilder.DropColumn(
                name: "ClaimedSkillIdsJson",
                table: "AssessmentAttempt");
        }
    }
}
