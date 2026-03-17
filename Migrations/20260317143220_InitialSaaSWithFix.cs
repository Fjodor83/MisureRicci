using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class InitialSaaSWithFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clienti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cognome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Indirizzo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Citta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatoProvincia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodicePostale = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paese = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataRegistrazione = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clienti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Negozi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Citta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Indirizzo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodiceNegozio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paese = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Negozi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MisureCamicia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Collo = table.Column<double>(type: "float", nullable: false),
                    Spalle = table.Column<double>(type: "float", nullable: false),
                    Torace = table.Column<double>(type: "float", nullable: false),
                    Vita = table.Column<double>(type: "float", nullable: false),
                    Manica = table.Column<double>(type: "float", nullable: false),
                    Polso = table.Column<double>(type: "float", nullable: false),
                    Lunghezza = table.Column<double>(type: "float", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureCamicia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureCamicia_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisureCintura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Lunghezza = table.Column<double>(type: "float", nullable: false),
                    Girovita = table.Column<double>(type: "float", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureCintura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureCintura_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisureCravatta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Lunghezza = table.Column<double>(type: "float", nullable: false),
                    Larghezza = table.Column<double>(type: "float", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureCravatta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureCravatta_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisureGiacca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Spalle = table.Column<double>(type: "float", nullable: false),
                    Torace = table.Column<double>(type: "float", nullable: false),
                    Vita = table.Column<double>(type: "float", nullable: false),
                    Manica = table.Column<double>(type: "float", nullable: false),
                    Lunghezza = table.Column<double>(type: "float", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureGiacca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureGiacca_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisureGilet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Torace = table.Column<double>(type: "float", nullable: false),
                    Vita = table.Column<double>(type: "float", nullable: false),
                    Lunghezza = table.Column<double>(type: "float", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureGilet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureGilet_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisureMaglie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Torace = table.Column<double>(type: "float", nullable: false),
                    Spalle = table.Column<double>(type: "float", nullable: false),
                    Manica = table.Column<double>(type: "float", nullable: false),
                    Lunghezza = table.Column<double>(type: "float", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureMaglie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureMaglie_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisureOutdoor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Torace = table.Column<double>(type: "float", nullable: false),
                    Spalle = table.Column<double>(type: "float", nullable: false),
                    Manica = table.Column<double>(type: "float", nullable: false),
                    Lunghezza = table.Column<double>(type: "float", nullable: false),
                    Fit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureOutdoor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureOutdoor_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisurePantalone",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vita = table.Column<double>(type: "float", nullable: false),
                    Bacino = table.Column<double>(type: "float", nullable: false),
                    Cavallo = table.Column<double>(type: "float", nullable: false),
                    InternoGamba = table.Column<double>(type: "float", nullable: false),
                    Fondo = table.Column<double>(type: "float", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisurePantalone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisurePantalone_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisureScarpe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Taglia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LunghezzaPiede = table.Column<double>(type: "float", nullable: false),
                    Pianta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureScarpe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureScarpe_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MisureAbitoCompleto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaccaId = table.Column<int>(type: "int", nullable: false),
                    PantaloneId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MisureAbitoCompleto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MisureAbitoCompleto_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MisureAbitoCompleto_MisureGiacca_GiaccaId",
                        column: x => x.GiaccaId,
                        principalTable: "MisureGiacca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MisureAbitoCompleto_MisurePantalone_PantaloneId",
                        column: x => x.PantaloneId,
                        principalTable: "MisurePantalone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MisureAbitoCompleto_ClienteId",
                table: "MisureAbitoCompleto",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureAbitoCompleto_GiaccaId",
                table: "MisureAbitoCompleto",
                column: "GiaccaId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureAbitoCompleto_PantaloneId",
                table: "MisureAbitoCompleto",
                column: "PantaloneId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureCamicia_ClienteId",
                table: "MisureCamicia",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureCintura_ClienteId",
                table: "MisureCintura",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureCravatta_ClienteId",
                table: "MisureCravatta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureGiacca_ClienteId",
                table: "MisureGiacca",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureGilet_ClienteId",
                table: "MisureGilet",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureMaglie_ClienteId",
                table: "MisureMaglie",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureOutdoor_ClienteId",
                table: "MisureOutdoor",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisurePantalone_ClienteId",
                table: "MisurePantalone",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MisureScarpe_ClienteId",
                table: "MisureScarpe",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MisureAbitoCompleto");

            migrationBuilder.DropTable(
                name: "MisureCamicia");

            migrationBuilder.DropTable(
                name: "MisureCintura");

            migrationBuilder.DropTable(
                name: "MisureCravatta");

            migrationBuilder.DropTable(
                name: "MisureGilet");

            migrationBuilder.DropTable(
                name: "MisureMaglie");

            migrationBuilder.DropTable(
                name: "MisureOutdoor");

            migrationBuilder.DropTable(
                name: "MisureScarpe");

            migrationBuilder.DropTable(
                name: "Negozi");

            migrationBuilder.DropTable(
                name: "MisureGiacca");

            migrationBuilder.DropTable(
                name: "MisurePantalone");

            migrationBuilder.DropTable(
                name: "Clienti");
        }
    }
}
