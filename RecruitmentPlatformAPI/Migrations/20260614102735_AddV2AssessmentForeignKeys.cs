using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddV2AssessmentForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttemptsV2_JobTitleId",
                table: "AssessmentAttemptsV2",
                column: "JobTitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentAttemptsV2_JobSeekers_JobSeekerId",
                table: "AssessmentAttemptsV2",
                column: "JobSeekerId",
                principalTable: "JobSeekers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentAttemptsV2_JobTitle_JobTitleId",
                table: "AssessmentAttemptsV2",
                column: "JobTitleId",
                principalTable: "JobTitle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentQuestionsV2_Skills_SkillId",
                table: "AssessmentQuestionsV2",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentAttemptsV2_JobSeekers_JobSeekerId",
                table: "AssessmentAttemptsV2");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentAttemptsV2_JobTitle_JobTitleId",
                table: "AssessmentAttemptsV2");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentQuestionsV2_Skills_SkillId",
                table: "AssessmentQuestionsV2");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentAttemptsV2_JobTitleId",
                table: "AssessmentAttemptsV2");
        }
    }
}
