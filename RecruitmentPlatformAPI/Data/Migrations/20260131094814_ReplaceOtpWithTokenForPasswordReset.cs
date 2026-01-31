using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceOtpWithTokenForPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "PasswordResets");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "PasswordResets",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "PasswordResets");

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "PasswordResets",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");
        }
    }
}
