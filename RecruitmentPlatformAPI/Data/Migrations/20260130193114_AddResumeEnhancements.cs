using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resume_JobSeekerId_Unique",
                table: "Resumes");

            migrationBuilder.RenameColumn(
                name: "FileUrl",
                table: "Resumes",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "Resumes",
                newName: "ContentType");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Resumes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Resumes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "Resumes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Resume_JobSeekerId_Active_Unique",
                table: "Resumes",
                column: "JobSeekerId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Resume_JobSeekerId_IsDeleted",
                table: "Resumes",
                columns: new[] { "JobSeekerId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resume_JobSeekerId_Active_Unique",
                table: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_Resume_JobSeekerId_IsDeleted",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "Resumes");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Resumes",
                newName: "FileUrl");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Resumes",
                newName: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_Resume_JobSeekerId_Unique",
                table: "Resumes",
                column: "JobSeekerId",
                unique: true);
        }
    }
}
