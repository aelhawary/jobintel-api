using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeJobTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recommendations_JobSeekerId",
                table: "Recommendations");

            migrationBuilder.AlterColumn<string>(
                name: "AiReasoning",
                table: "Recommendations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsViewed",
                table: "Recommendations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MatchedSkillsJson",
                table: "Recommendations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissingSkillsJson",
                table: "Recommendations",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JobId",
                table: "ProfileView",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JobTitleId",
                table: "Jobs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_JobSeeker_Date",
                table: "Recommendations",
                columns: new[] { "JobSeekerId", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileView_Dedup",
                table: "ProfileView",
                columns: new[] { "JobSeekerId", "ViewerRecruiterId", "ViewType", "ViewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileView_ViewerRecruiterId",
                table: "ProfileView",
                column: "ViewerRecruiterId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobTitleId",
                table: "Jobs",
                column: "JobTitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_JobTitle_JobTitleId",
                table: "Jobs",
                column: "JobTitleId",
                principalTable: "JobTitle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileView_Recruiters_ViewerRecruiterId",
                table: "ProfileView",
                column: "ViewerRecruiterId",
                principalTable: "Recruiters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_JobTitle_JobTitleId",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileView_Recruiters_ViewerRecruiterId",
                table: "ProfileView");

            migrationBuilder.DropIndex(
                name: "IX_Recommendation_JobSeeker_Date",
                table: "Recommendations");

            migrationBuilder.DropIndex(
                name: "IX_ProfileView_Dedup",
                table: "ProfileView");

            migrationBuilder.DropIndex(
                name: "IX_ProfileView_ViewerRecruiterId",
                table: "ProfileView");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobTitleId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsViewed",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "MatchedSkillsJson",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "MissingSkillsJson",
                table: "Recommendations");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "ProfileView");

            migrationBuilder.DropColumn(
                name: "JobTitleId",
                table: "Jobs");

            migrationBuilder.AlterColumn<string>(
                name: "AiReasoning",
                table: "Recommendations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_JobSeekerId",
                table: "Recommendations",
                column: "JobSeekerId");
        }
    }
}
