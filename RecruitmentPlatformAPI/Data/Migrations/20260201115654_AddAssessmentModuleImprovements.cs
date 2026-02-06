using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentModuleImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssessmentQuestion_Filtering",
                table: "AssessmentQuestion");

            migrationBuilder.AddColumn<int>(
                name: "RoleFamily",
                table: "AssessmentQuestion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuestionsAnswered",
                table: "AssessmentAttempt",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuestions",
                table: "AssessmentAttempt",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeSpentSeconds",
                table: "AssessmentAnswer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestion_Filtering",
                table: "AssessmentQuestion",
                columns: new[] { "RoleFamily", "Category", "Difficulty", "SeniorityLevel", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_SingleInProgress",
                table: "AssessmentAttempt",
                column: "JobSeekerId",
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssessmentQuestion_Filtering",
                table: "AssessmentQuestion");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentAttempt_SingleInProgress",
                table: "AssessmentAttempt");

            migrationBuilder.DropColumn(
                name: "RoleFamily",
                table: "AssessmentQuestion");

            migrationBuilder.DropColumn(
                name: "QuestionsAnswered",
                table: "AssessmentAttempt");

            migrationBuilder.DropColumn(
                name: "TotalQuestions",
                table: "AssessmentAttempt");

            migrationBuilder.DropColumn(
                name: "TimeSpentSeconds",
                table: "AssessmentAnswer");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestion_Filtering",
                table: "AssessmentQuestion",
                columns: new[] { "Category", "Difficulty", "SeniorityLevel", "IsActive" });
        }
    }
}
