using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicFieldLayoutMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gruppo",
                table: "MeasurementFieldDefinitions",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HelpText",
                table: "MeasurementFieldDefinitions",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrdineGruppo",
                table: "MeasurementFieldDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Template",
                table: "MeasurementFieldDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gruppo",
                table: "MeasurementFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "HelpText",
                table: "MeasurementFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "OrdineGruppo",
                table: "MeasurementFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "Template",
                table: "MeasurementFieldDefinitions");
        }
    }
}
