using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class RequireSkillIdForAssessmentQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE q
SET [SkillId] = CASE
        WHEN q.[Category] = 2 THEN
                CASE
                        WHEN LOWER(q.[QuestionText]) LIKE '%communicat%'
                            OR LOWER(q.[QuestionText]) LIKE '%criticism%'
                            OR LOWER(q.[QuestionText]) LIKE '%non-technical%' THEN 48
                        WHEN LOWER(q.[QuestionText]) LIKE '%project%'
                            OR LOWER(q.[QuestionText]) LIKE '%scope%'
                            OR LOWER(q.[QuestionText]) LIKE '%deadline%' THEN 49
                        WHEN LOWER(q.[QuestionText]) LIKE '%agile%'
                            OR LOWER(q.[QuestionText]) LIKE '%scrum%'
                            OR LOWER(q.[QuestionText]) LIKE '%code review%'
                            OR LOWER(q.[QuestionText]) LIKE '%mentor%'
                            OR LOWER(q.[QuestionText]) LIKE '%team%' THEN 45
                        ELSE 47
                END
        WHEN LOWER(q.[QuestionText]) LIKE '%graphql%' THEN 44
        WHEN LOWER(q.[QuestionText]) LIKE '%sql%'
            OR LOWER(q.[QuestionText]) LIKE '%database%'
            OR LOWER(q.[QuestionText]) LIKE '%index%'
            OR LOWER(q.[QuestionText]) LIKE '%acid%'
            OR LOWER(q.[QuestionText]) LIKE '%shard%'
            OR LOWER(q.[QuestionText]) LIKE '%locking%' THEN 23
        WHEN LOWER(q.[QuestionText]) LIKE '%jwt%'
            OR LOWER(q.[QuestionText]) LIKE '%http%'
            OR LOWER(q.[QuestionText]) LIKE '%rest%'
            OR LOWER(q.[QuestionText]) LIKE '%api%'
            OR LOWER(q.[QuestionText]) LIKE '%cors%'
            OR LOWER(q.[QuestionText]) LIKE '%idempotent%' THEN 43
        WHEN q.[RoleFamily] = 1
            AND (LOWER(q.[QuestionText]) LIKE '%css%'
            OR LOWER(q.[QuestionText]) LIKE '%dom%'
            OR LOWER(q.[QuestionText]) LIKE '%responsive%'
            OR LOWER(q.[QuestionText]) LIKE '%alt%') THEN 15
        WHEN q.[RoleFamily] = 1
            AND (LOWER(q.[QuestionText]) LIKE '%javascript%'
            OR LOWER(q.[QuestionText]) LIKE '%closure%'
            OR LOWER(q.[QuestionText]) LIKE '%event bubbling%'
            OR LOWER(q.[QuestionText]) LIKE '%web workers%') THEN 2
        WHEN q.[RoleFamily] = 1 THEN 11
        WHEN q.[RoleFamily] = 3
            AND (LOWER(q.[QuestionText]) LIKE '%real-time%'
            OR LOWER(q.[QuestionText]) LIKE '%websocket%'
            OR LOWER(q.[QuestionText]) LIKE '%long polling%'
            OR LOWER(q.[QuestionText]) LIKE '%server-sent%') THEN 18
        WHEN q.[RoleFamily] = 3 THEN 43
        WHEN q.[RoleFamily] = 2 THEN 17
        ELSE 43
END
FROM [AssessmentQuestion] q
WHERE q.[SkillId] IS NULL;
");

        migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [AssessmentQuestion] WHERE [SkillId] IS NULL)
        THROW 50000, 'AssessmentQuestion.SkillId backfill failed before NOT NULL migration.', 1;
");

            migrationBuilder.AlterColumn<int>(
                name: "SkillId",
                table: "AssessmentQuestion",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 1,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 2,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 3,
                column: "SkillId",
                value: 23);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 4,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 5,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 6,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 7,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 8,
                column: "SkillId",
                value: 23);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 9,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 10,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 11,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 12,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 13,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 14,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 15,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 16,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 17,
                column: "SkillId",
                value: 23);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 18,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 19,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 20,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 21,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 22,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 23,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 24,
                column: "SkillId",
                value: 23);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 25,
                column: "SkillId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 26,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 27,
                column: "SkillId",
                value: 15);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 28,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 29,
                column: "SkillId",
                value: 15);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 30,
                column: "SkillId",
                value: 15);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 31,
                column: "SkillId",
                value: 15);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 32,
                column: "SkillId",
                value: 15);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 33,
                column: "SkillId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 34,
                column: "SkillId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 35,
                column: "SkillId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 36,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 37,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 38,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 39,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 40,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 41,
                column: "SkillId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 42,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 43,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 44,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 45,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 46,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 47,
                column: "SkillId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 48,
                column: "SkillId",
                value: 44);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 49,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 50,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 51,
                column: "SkillId",
                value: 18);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 52,
                column: "SkillId",
                value: 43);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 53,
                column: "SkillId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 54,
                column: "SkillId",
                value: 48);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 55,
                column: "SkillId",
                value: 48);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 56,
                column: "SkillId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 57,
                column: "SkillId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 58,
                column: "SkillId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 59,
                column: "SkillId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 60,
                column: "SkillId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 61,
                column: "SkillId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 62,
                column: "SkillId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 63,
                column: "SkillId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 64,
                column: "SkillId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 65,
                column: "SkillId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 66,
                column: "SkillId",
                value: 49);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 67,
                column: "SkillId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 68,
                column: "SkillId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 69,
                column: "SkillId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 70,
                column: "SkillId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 71,
                column: "SkillId",
                value: 45);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 72,
                column: "SkillId",
                value: 47);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 73,
                column: "SkillId",
                value: 45);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SkillId",
                table: "AssessmentQuestion",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 1,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 2,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 3,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 4,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 5,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 6,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 7,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 8,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 9,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 10,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 11,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 12,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 13,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 14,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 15,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 16,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 17,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 18,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 19,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 20,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 21,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 22,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 23,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 24,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 25,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 26,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 27,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 28,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 29,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 30,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 31,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 32,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 33,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 34,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 35,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 36,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 37,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 38,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 39,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 40,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 41,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 42,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 43,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 44,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 45,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 46,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 47,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 48,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 49,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 50,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 51,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 52,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 53,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 54,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 55,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 56,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 57,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 58,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 59,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 60,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 61,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 62,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 63,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 64,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 65,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 66,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 67,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 68,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 69,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 70,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 71,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 72,
                column: "SkillId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 73,
                column: "SkillId",
                value: null);
        }
    }
}
