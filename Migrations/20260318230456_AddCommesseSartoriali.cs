using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddCommesseSartoriali : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommesseSartoriali",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommessaCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    NegozioId = table.Column<int>(type: "int", nullable: true),
                    TipoCapo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tessuto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Collezione = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DataApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConsegnaPrevista = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataConsegnaEffettiva = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Stato = table.Column<int>(type: "int", nullable: false),
                    NoteInterne = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommesseSartoriali", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommesseSartoriali_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommesseSartoriali_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommesseSartoriali_Negozi_NegozioId",
                        column: x => x.NegozioId,
                        principalTable: "Negozi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CommesseEventi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommessaSartorialeId = table.Column<int>(type: "int", nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NuovoStato = table.Column<int>(type: "int", nullable: true),
                    Descrizione = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommesseEventi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommesseEventi_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommesseEventi_CommesseSartoriali_CommessaSartorialeId",
                        column: x => x.CommessaSartorialeId,
                        principalTable: "CommesseSartoriali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommesseEventi_CommessaSartorialeId",
                table: "CommesseEventi",
                column: "CommessaSartorialeId");

            migrationBuilder.CreateIndex(
                name: "IX_CommesseEventi_CreatedByUserId",
                table: "CommesseEventi",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommesseSartoriali_ClienteId",
                table: "CommesseSartoriali",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CommesseSartoriali_CommessaCode",
                table: "CommesseSartoriali",
                column: "CommessaCode",
                unique: true,
                filter: "[CommessaCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CommesseSartoriali_CreatedByUserId",
                table: "CommesseSartoriali",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommesseSartoriali_NegozioId",
                table: "CommesseSartoriali",
                column: "NegozioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommesseEventi");

            migrationBuilder.DropTable(
                name: "CommesseSartoriali");
        }
    }
}
