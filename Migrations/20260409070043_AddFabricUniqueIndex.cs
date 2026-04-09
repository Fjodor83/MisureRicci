using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddFabricUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clienti_Negozi_NegozioId",
                table: "Clienti");

            migrationBuilder.DropIndex(
                name: "IX_DynamicMeasurementValues_DynamicMeasurementRecordId",
                table: "DynamicMeasurementValues");

            migrationBuilder.DropIndex(
                name: "IX_DynamicMeasurementRecords_ClienteId",
                table: "DynamicMeasurementRecords");

            migrationBuilder.DropIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId",
                table: "CommissioniMisureLinks");

            migrationBuilder.DropIndex(
                name: "IX_Clienti_NegozioId",
                table: "Clienti");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Fabrics",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clienti",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clienti",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Cognome",
                table: "Clienti",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Fabrics_Nome",
                table: "Fabrics",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementValues_DynamicMeasurementRecordId_MeasurementFieldDefinitionId",
                table: "DynamicMeasurementValues",
                columns: new[] { "DynamicMeasurementRecordId", "MeasurementFieldDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementRecords_ClienteId_MeasurementTypeId",
                table: "DynamicMeasurementRecords",
                columns: new[] { "ClienteId", "MeasurementTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId_MisuraClienteId",
                table: "CommissioniMisureLinks",
                columns: new[] { "CommessaSartorialeId", "MisuraClienteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Email",
                table: "Clienti",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Negozio_Cognome_Nome",
                table: "Clienti",
                columns: new[] { "NegozioId", "Cognome", "Nome" });

            migrationBuilder.AddForeignKey(
                name: "FK_Clienti_Negozi_NegozioId",
                table: "Clienti",
                column: "NegozioId",
                principalTable: "Negozi",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clienti_Negozi_NegozioId",
                table: "Clienti");

            migrationBuilder.DropIndex(
                name: "IX_Fabrics_Nome",
                table: "Fabrics");

            migrationBuilder.DropIndex(
                name: "IX_DynamicMeasurementValues_DynamicMeasurementRecordId_MeasurementFieldDefinitionId",
                table: "DynamicMeasurementValues");

            migrationBuilder.DropIndex(
                name: "IX_DynamicMeasurementRecords_ClienteId_MeasurementTypeId",
                table: "DynamicMeasurementRecords");

            migrationBuilder.DropIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId_MisuraClienteId",
                table: "CommissioniMisureLinks");

            migrationBuilder.DropIndex(
                name: "IX_Clienti_Email",
                table: "Clienti");

            migrationBuilder.DropIndex(
                name: "IX_Clienti_Negozio_Cognome_Nome",
                table: "Clienti");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Fabrics",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clienti",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clienti",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Cognome",
                table: "Clienti",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementValues_DynamicMeasurementRecordId",
                table: "DynamicMeasurementValues",
                column: "DynamicMeasurementRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementRecords_ClienteId",
                table: "DynamicMeasurementRecords",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId",
                table: "CommissioniMisureLinks",
                column: "CommessaSartorialeId");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_NegozioId",
                table: "Clienti",
                column: "NegozioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clienti_Negozi_NegozioId",
                table: "Clienti",
                column: "NegozioId",
                principalTable: "Negozi",
                principalColumn: "Id");
        }
    }
}
