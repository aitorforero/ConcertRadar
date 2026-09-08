using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConcertRadar.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalSourceIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Genres_Name",
                table: "Genres");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Performances",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Performances",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Genres",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Genres",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Events",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Events",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Bands",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Bands",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"Genres\" SET \"Source\" = 'manual', \"ExternalId\" = replace(\"Id\", '-', '')");
            migrationBuilder.Sql("UPDATE \"Bands\" SET \"Source\" = 'manual', \"ExternalId\" = replace(\"Id\", '-', '')");
            migrationBuilder.Sql("UPDATE \"Events\" SET \"Source\" = 'manual', \"ExternalId\" = replace(\"Id\", '-', '')");
            migrationBuilder.Sql("UPDATE \"Performances\" SET \"Source\" = 'manual', \"ExternalId\" = replace(\"Id\", '-', '')");

            migrationBuilder.CreateIndex(
                name: "IX_Performances_Source_ExternalId",
                table: "Performances",
                columns: new[] { "Source", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name",
                table: "Genres",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Source_ExternalId",
                table: "Genres",
                columns: new[] { "Source", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_Source_ExternalId",
                table: "Events",
                columns: new[] { "Source", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bands_Source_ExternalId",
                table: "Bands",
                columns: new[] { "Source", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Performances_Source_ExternalId",
                table: "Performances");

            migrationBuilder.DropIndex(
                name: "IX_Genres_Name",
                table: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_Genres_Source_ExternalId",
                table: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_Events_Source_ExternalId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Bands_Source_ExternalId",
                table: "Bands");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Performances");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Performances");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Bands");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Bands");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name",
                table: "Genres",
                column: "Name",
                unique: true);
        }
    }
}
