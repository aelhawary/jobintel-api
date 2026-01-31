using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExperienceAndEducationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Educations");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Experiences",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Experiences");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Educations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
