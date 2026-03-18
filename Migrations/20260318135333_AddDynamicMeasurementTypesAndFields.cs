using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicMeasurementTypesAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeasurementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DynamicMeasurementRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    MeasurementTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicMeasurementRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicMeasurementRecords_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DynamicMeasurementRecords_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DynamicMeasurementRecords_MeasurementTypes_MeasurementTypeId",
                        column: x => x.MeasurementTypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeasurementTypeId = table.Column<int>(type: "int", nullable: false),
                    NomeCampo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Etichetta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoDato = table.Column<int>(type: "int", nullable: false),
                    UnitaMisura = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Placeholder = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Obbligatorio = table.Column<bool>(type: "bit", nullable: false),
                    Ordine = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementFieldDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasurementFieldDefinitions_MeasurementTypes_MeasurementTypeId",
                        column: x => x.MeasurementTypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicMeasurementValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DynamicMeasurementRecordId = table.Column<int>(type: "int", nullable: false),
                    MeasurementFieldDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Valore = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicMeasurementValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicMeasurementValues_DynamicMeasurementRecords_DynamicMeasurementRecordId",
                        column: x => x.DynamicMeasurementRecordId,
                        principalTable: "DynamicMeasurementRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicMeasurementValues_MeasurementFieldDefinitions_MeasurementFieldDefinitionId",
                        column: x => x.MeasurementFieldDefinitionId,
                        principalTable: "MeasurementFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementRecords_ClienteId",
                table: "DynamicMeasurementRecords",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementRecords_CreatedByUserId",
                table: "DynamicMeasurementRecords",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementRecords_MeasurementTypeId",
                table: "DynamicMeasurementRecords",
                column: "MeasurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementValues_DynamicMeasurementRecordId",
                table: "DynamicMeasurementValues",
                column: "DynamicMeasurementRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementValues_MeasurementFieldDefinitionId",
                table: "DynamicMeasurementValues",
                column: "MeasurementFieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementFieldDefinitions_MeasurementTypeId_NomeCampo",
                table: "MeasurementFieldDefinitions",
                columns: new[] { "MeasurementTypeId", "NomeCampo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementTypes_Nome",
                table: "MeasurementTypes",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicMeasurementValues");

            migrationBuilder.DropTable(
                name: "DynamicMeasurementRecords");

            migrationBuilder.DropTable(
                name: "MeasurementFieldDefinitions");

            migrationBuilder.DropTable(
                name: "MeasurementTypes");
        }
    }
}
