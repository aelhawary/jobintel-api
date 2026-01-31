using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBioToJobSeeker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "JobSeekers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "JobSeekers");
        }
    }
}
