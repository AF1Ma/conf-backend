using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferencePlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class DashboardAuthAndTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDiplomaVerified",
                table: "Speakers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "Users" SET "Email" = LOWER(BTRIM("Email"));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            // Keep the two example conferences on new installations and give them real tracks.
            migrationBuilder.Sql("""
                INSERT INTO "Conferences" ("Id", "Title", "Description", "StartDate", "EndDate", "Location", "OrganizerId")
                SELECT seed.* FROM (VALUES
                    ('d3b07384-d113-4c57-bc9b-a39f64724216', 'Science & IT Conf 2026',
                     'Ежегодная научная конференция по современным IT-технологиям и архитектуре систем.',
                     TIMESTAMPTZ '2026-10-15 10:00:00+00', TIMESTAMPTZ '2026-10-17 18:00:00+00', 'Главный корпус / Онлайн', 'admin-1'),
                    ('e7c10592-8a44-42b1-9b63-c78216147289', 'DevOps & Cloud Days',
                     'Митап практиков по контейнеризации, CI/CD и наблюдаемости распределенных систем.',
                     TIMESTAMPTZ '2026-10-22 11:00:00+00', TIMESTAMPTZ '2026-10-23 17:00:00+00', 'Аудитория 204', 'admin-1')
                ) AS seed
                WHERE NOT EXISTS (SELECT 1 FROM "Conferences")
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO "Tracks" ("Id", "ConferenceId", "Name")
                SELECT seed.* FROM (VALUES
                    ('track-backend', 'd3b07384-d113-4c57-bc9b-a39f64724216', 'Архитектура и backend'),
                    ('track-frontend', 'd3b07384-d113-4c57-bc9b-a39f64724216', 'Интерфейсы и frontend'),
                    ('track-research', 'd3b07384-d113-4c57-bc9b-a39f64724216', 'Прикладные исследования'),
                    ('track-devops', 'e7c10592-8a44-42b1-9b63-c78216147289', 'DevOps и CI/CD'),
                    ('track-cloud', 'e7c10592-8a44-42b1-9b63-c78216147289', 'Облачная инфраструктура'),
                    ('track-observability', 'e7c10592-8a44-42b1-9b63-c78216147289', 'Наблюдаемость')
                ) AS seed("Id", "ConferenceId", "Name")
                WHERE EXISTS (SELECT 1 FROM "Conferences" c WHERE c."Id" = seed."ConferenceId")
                ON CONFLICT ("Id") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDiplomaVerified",
                table: "Speakers");
        }
    }
}
