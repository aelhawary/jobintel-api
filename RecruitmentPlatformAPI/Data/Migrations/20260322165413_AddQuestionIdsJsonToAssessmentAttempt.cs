using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionIdsJsonToAssessmentAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestionIdsJson",
                table: "AssessmentAttempt",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionIdsJson",
                table: "AssessmentAttempt");
        }
    }
}
