using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementTypeImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clienti_ClientCode",
                table: "Clienti");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MeasurementTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_ClientCode",
                table: "Clienti",
                column: "ClientCode",
                unique: true,
                filter: "[ClientCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clienti_ClientCode",
                table: "Clienti");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MeasurementTypes");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_ClientCode",
                table: "Clienti",
                column: "ClientCode",
                unique: true);
        }
    }
}
