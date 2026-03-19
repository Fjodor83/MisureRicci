using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddCommessaKpiIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistroMisure_ClienteId",
                table: "RegistroMisure");

            migrationBuilder.DropIndex(
                name: "IX_CommesseSartoriali_ClienteId",
                table: "CommesseSartoriali");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroMisure_ClienteId_DataCreazione",
                table: "RegistroMisure",
                columns: new[] { "ClienteId", "DataCreazione" });

            migrationBuilder.CreateIndex(
                name: "IX_CommesseSartoriali_ClienteId_DataApertura",
                table: "CommesseSartoriali",
                columns: new[] { "ClienteId", "DataApertura" });

            migrationBuilder.CreateIndex(
                name: "IX_CommesseSartoriali_Stato_DataConsegnaPrevista",
                table: "CommesseSartoriali",
                columns: new[] { "Stato", "DataConsegnaPrevista" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistroMisure_ClienteId_DataCreazione",
                table: "RegistroMisure");

            migrationBuilder.DropIndex(
                name: "IX_CommesseSartoriali_ClienteId_DataApertura",
                table: "CommesseSartoriali");

            migrationBuilder.DropIndex(
                name: "IX_CommesseSartoriali_Stato_DataConsegnaPrevista",
                table: "CommesseSartoriali");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroMisure_ClienteId",
                table: "RegistroMisure",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CommesseSartoriali_ClienteId",
                table: "CommesseSartoriali",
                column: "ClienteId");
        }
    }
}
