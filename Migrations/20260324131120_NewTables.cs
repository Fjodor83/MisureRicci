using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class NewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniEventi_AspNetUsers_CreatedByUserId",
                table: "CommissioniEventi");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniMisureLinks_AspNetUsers_LinkedByUserId",
                table: "CommissioniMisureLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniMisureLinks_RegistroMisure_MisuraClienteId",
                table: "CommissioniMisureLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniSartoriali_AspNetUsers_CreatedByUserId",
                table: "CommissioniSartoriali");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniSartoriali_Clienti_ClienteId",
                table: "CommissioniSartoriali");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniSartoriali_Negozi_NegozioId",
                table: "CommissioniSartoriali");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicMeasurementRecords_AspNetUsers_CreatedByUserId",
                table: "DynamicMeasurementRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicMeasurementRecords_Clienti_ClienteId",
                table: "DynamicMeasurementRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicMeasurementRecords_MeasurementTypes_MeasurementTypeId",
                table: "DynamicMeasurementRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicMeasurementValues_MeasurementFieldDefinitions_MeasurementFieldDefinitionId",
                table: "DynamicMeasurementValues");

            migrationBuilder.DropForeignKey(
                name: "FK_MeasurementFieldDefinitions_MeasurementTypes_MeasurementTypeId",
                table: "MeasurementFieldDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_MisureAbitoCompleto_MisureGiacca_GiaccaId",
                table: "MisureAbitoCompleto");

            migrationBuilder.DropForeignKey(
                name: "FK_MisureAbitoCompleto_MisurePantalone_PantaloneId",
                table: "MisureAbitoCompleto");

            migrationBuilder.DropIndex(
                name: "IX_RegistroMisure_ClienteId_DataCreazione",
                table: "RegistroMisure");

            migrationBuilder.DropIndex(
                name: "IX_CommissioniSartoriali_ClienteId_DataApertura",
                table: "CommissioniSartoriali");

            migrationBuilder.DropIndex(
                name: "IX_CommissioniSartoriali_CommessaCode",
                table: "CommissioniSartoriali");

            migrationBuilder.DropIndex(
                name: "IX_CommissioniSartoriali_Stato_DataConsegnaPrevista",
                table: "CommissioniSartoriali");

            migrationBuilder.DropIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId_MisuraClienteId",
                table: "CommissioniMisureLinks");

            migrationBuilder.DropIndex(
                name: "IX_Clienti_ClientCode",
                table: "Clienti");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeasurementTypes",
                table: "MeasurementTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeasurementFieldDefinitions",
                table: "MeasurementFieldDefinitions");

            migrationBuilder.RenameTable(
                name: "MeasurementTypes",
                newName: "DynamicMeasurementTypes");

            migrationBuilder.RenameTable(
                name: "MeasurementFieldDefinitions",
                newName: "DynamicFieldDefinitions");

            migrationBuilder.RenameIndex(
                name: "IX_MeasurementTypes_Nome",
                table: "DynamicMeasurementTypes",
                newName: "IX_DynamicMeasurementTypes_Nome");

            migrationBuilder.RenameIndex(
                name: "IX_MeasurementFieldDefinitions_MeasurementTypeId_NomeCampo",
                table: "DynamicFieldDefinitions",
                newName: "IX_DynamicFieldDefinitions_MeasurementTypeId_NomeCampo");

            migrationBuilder.AlterColumn<string>(
                name: "ClientCode",
                table: "Clienti",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComputedColumnSql: "CAST('SR-' + CAST(YEAR([DataRegistrazione]) AS nvarchar(4)) + '-' + RIGHT('00000' + CAST([Id] AS nvarchar(5)), 5) AS nvarchar(20))");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DynamicMeasurementTypes",
                table: "DynamicMeasurementTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DynamicFieldDefinitions",
                table: "DynamicFieldDefinitions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroMisure_ClienteId",
                table: "RegistroMisure",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniSartoriali_ClienteId",
                table: "CommissioniSartoriali",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId",
                table: "CommissioniMisureLinks",
                column: "CommessaSartorialeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniEventi_AspNetUsers_CreatedByUserId",
                table: "CommissioniEventi",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniMisureLinks_AspNetUsers_LinkedByUserId",
                table: "CommissioniMisureLinks",
                column: "LinkedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniMisureLinks_RegistroMisure_MisuraClienteId",
                table: "CommissioniMisureLinks",
                column: "MisuraClienteId",
                principalTable: "RegistroMisure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniSartoriali_AspNetUsers_CreatedByUserId",
                table: "CommissioniSartoriali",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniSartoriali_Clienti_ClienteId",
                table: "CommissioniSartoriali",
                column: "ClienteId",
                principalTable: "Clienti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniSartoriali_Negozi_NegozioId",
                table: "CommissioniSartoriali",
                column: "NegozioId",
                principalTable: "Negozi",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFieldDefinitions_DynamicMeasurementTypes_MeasurementTypeId",
                table: "DynamicFieldDefinitions",
                column: "MeasurementTypeId",
                principalTable: "DynamicMeasurementTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicMeasurementRecords_AspNetUsers_CreatedByUserId",
                table: "DynamicMeasurementRecords",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicMeasurementRecords_Clienti_ClienteId",
                table: "DynamicMeasurementRecords",
                column: "ClienteId",
                principalTable: "Clienti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicMeasurementRecords_DynamicMeasurementTypes_MeasurementTypeId",
                table: "DynamicMeasurementRecords",
                column: "MeasurementTypeId",
                principalTable: "DynamicMeasurementTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicMeasurementValues_DynamicFieldDefinitions_MeasurementFieldDefinitionId",
                table: "DynamicMeasurementValues",
                column: "MeasurementFieldDefinitionId",
                principalTable: "DynamicFieldDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MisureAbitoCompleto_MisureGiacca_GiaccaId",
                table: "MisureAbitoCompleto",
                column: "GiaccaId",
                principalTable: "MisureGiacca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MisureAbitoCompleto_MisurePantalone_PantaloneId",
                table: "MisureAbitoCompleto",
                column: "PantaloneId",
                principalTable: "MisurePantalone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniEventi_AspNetUsers_CreatedByUserId",
                table: "CommissioniEventi");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniMisureLinks_AspNetUsers_LinkedByUserId",
                table: "CommissioniMisureLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniMisureLinks_RegistroMisure_MisuraClienteId",
                table: "CommissioniMisureLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniSartoriali_AspNetUsers_CreatedByUserId",
                table: "CommissioniSartoriali");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniSartoriali_Clienti_ClienteId",
                table: "CommissioniSartoriali");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioniSartoriali_Negozi_NegozioId",
                table: "CommissioniSartoriali");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFieldDefinitions_DynamicMeasurementTypes_MeasurementTypeId",
                table: "DynamicFieldDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicMeasurementRecords_AspNetUsers_CreatedByUserId",
                table: "DynamicMeasurementRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicMeasurementRecords_Clienti_ClienteId",
                table: "DynamicMeasurementRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicMeasurementRecords_DynamicMeasurementTypes_MeasurementTypeId",
                table: "DynamicMeasurementRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicMeasurementValues_DynamicFieldDefinitions_MeasurementFieldDefinitionId",
                table: "DynamicMeasurementValues");

            migrationBuilder.DropForeignKey(
                name: "FK_MisureAbitoCompleto_MisureGiacca_GiaccaId",
                table: "MisureAbitoCompleto");

            migrationBuilder.DropForeignKey(
                name: "FK_MisureAbitoCompleto_MisurePantalone_PantaloneId",
                table: "MisureAbitoCompleto");

            migrationBuilder.DropIndex(
                name: "IX_RegistroMisure_ClienteId",
                table: "RegistroMisure");

            migrationBuilder.DropIndex(
                name: "IX_CommissioniSartoriali_ClienteId",
                table: "CommissioniSartoriali");

            migrationBuilder.DropIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId",
                table: "CommissioniMisureLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DynamicMeasurementTypes",
                table: "DynamicMeasurementTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DynamicFieldDefinitions",
                table: "DynamicFieldDefinitions");

            migrationBuilder.RenameTable(
                name: "DynamicMeasurementTypes",
                newName: "MeasurementTypes");

            migrationBuilder.RenameTable(
                name: "DynamicFieldDefinitions",
                newName: "MeasurementFieldDefinitions");

            migrationBuilder.RenameIndex(
                name: "IX_DynamicMeasurementTypes_Nome",
                table: "MeasurementTypes",
                newName: "IX_MeasurementTypes_Nome");

            migrationBuilder.RenameIndex(
                name: "IX_DynamicFieldDefinitions_MeasurementTypeId_NomeCampo",
                table: "MeasurementFieldDefinitions",
                newName: "IX_MeasurementFieldDefinitions_MeasurementTypeId_NomeCampo");

            migrationBuilder.AlterColumn<string>(
                name: "ClientCode",
                table: "Clienti",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                computedColumnSql: "CAST('SR-' + CAST(YEAR([DataRegistrazione]) AS nvarchar(4)) + '-' + RIGHT('00000' + CAST([Id] AS nvarchar(5)), 5) AS nvarchar(20))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeasurementTypes",
                table: "MeasurementTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeasurementFieldDefinitions",
                table: "MeasurementFieldDefinitions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroMisure_ClienteId_DataCreazione",
                table: "RegistroMisure",
                columns: new[] { "ClienteId", "DataCreazione" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniSartoriali_ClienteId_DataApertura",
                table: "CommissioniSartoriali",
                columns: new[] { "ClienteId", "DataApertura" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniSartoriali_CommessaCode",
                table: "CommissioniSartoriali",
                column: "CommessaCode",
                unique: true,
                filter: "[CommessaCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniSartoriali_Stato_DataConsegnaPrevista",
                table: "CommissioniSartoriali",
                columns: new[] { "Stato", "DataConsegnaPrevista" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId_MisuraClienteId",
                table: "CommissioniMisureLinks",
                columns: new[] { "CommessaSartorialeId", "MisuraClienteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_ClientCode",
                table: "Clienti",
                column: "ClientCode",
                unique: true,
                filter: "[ClientCode] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniEventi_AspNetUsers_CreatedByUserId",
                table: "CommissioniEventi",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniMisureLinks_AspNetUsers_LinkedByUserId",
                table: "CommissioniMisureLinks",
                column: "LinkedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniMisureLinks_RegistroMisure_MisuraClienteId",
                table: "CommissioniMisureLinks",
                column: "MisuraClienteId",
                principalTable: "RegistroMisure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniSartoriali_AspNetUsers_CreatedByUserId",
                table: "CommissioniSartoriali",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniSartoriali_Clienti_ClienteId",
                table: "CommissioniSartoriali",
                column: "ClienteId",
                principalTable: "Clienti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioniSartoriali_Negozi_NegozioId",
                table: "CommissioniSartoriali",
                column: "NegozioId",
                principalTable: "Negozi",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicMeasurementRecords_AspNetUsers_CreatedByUserId",
                table: "DynamicMeasurementRecords",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicMeasurementRecords_Clienti_ClienteId",
                table: "DynamicMeasurementRecords",
                column: "ClienteId",
                principalTable: "Clienti",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicMeasurementRecords_MeasurementTypes_MeasurementTypeId",
                table: "DynamicMeasurementRecords",
                column: "MeasurementTypeId",
                principalTable: "MeasurementTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicMeasurementValues_MeasurementFieldDefinitions_MeasurementFieldDefinitionId",
                table: "DynamicMeasurementValues",
                column: "MeasurementFieldDefinitionId",
                principalTable: "MeasurementFieldDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeasurementFieldDefinitions_MeasurementTypes_MeasurementTypeId",
                table: "MeasurementFieldDefinitions",
                column: "MeasurementTypeId",
                principalTable: "MeasurementTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MisureAbitoCompleto_MisureGiacca_GiaccaId",
                table: "MisureAbitoCompleto",
                column: "GiaccaId",
                principalTable: "MisureGiacca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MisureAbitoCompleto_MisurePantalone_PantaloneId",
                table: "MisureAbitoCompleto",
                column: "PantaloneId",
                principalTable: "MisurePantalone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
