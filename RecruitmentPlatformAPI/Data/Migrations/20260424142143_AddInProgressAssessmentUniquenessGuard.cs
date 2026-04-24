using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInProgressAssessmentUniquenessGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"WITH RankedInProgress AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (PARTITION BY JobSeekerId ORDER BY StartedAt DESC, Id DESC) AS rn
                    FROM AssessmentAttempt
                    WHERE Status = 1
                  )
                  UPDATE a
                  SET Status = 3,
                      CompletedAt = COALESCE(a.CompletedAt, SYSUTCDATETIME())
                  FROM AssessmentAttempt a
                  INNER JOIN RankedInProgress r ON a.Id = r.Id
                  WHERE r.rn > 1;");

            migrationBuilder.CreateIndex(
                name: "UX_AssessmentAttempt_JobSeeker_InProgress",
                table: "AssessmentAttempt",
                column: "JobSeekerId",
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_AssessmentAttempt_JobSeeker_InProgress",
                table: "AssessmentAttempt");
        }
    }
}
