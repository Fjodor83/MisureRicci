using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DynamicMeasurementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicMeasurementTypes", x => x.Id);
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DynamicFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeasurementTypeId = table.Column<int>(type: "int", nullable: false),
                    NomeCampo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Etichetta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gruppo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    OrdineGruppo = table.Column<int>(type: "int", nullable: false),
                    TipoDato = table.Column<int>(type: "int", nullable: false),
                    Template = table.Column<int>(type: "int", nullable: false),
                    UnitaMisura = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Placeholder = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HelpText = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Obbligatorio = table.Column<bool>(type: "bit", nullable: false),
                    Ordine = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFieldDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicFieldDefinitions_DynamicMeasurementTypes_MeasurementTypeId",
                        column: x => x.MeasurementTypeId,
                        principalTable: "DynamicMeasurementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NomeCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ruolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NegozioId = table.Column<int>(type: "int", nullable: true),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Negozi_NegozioId",
                        column: x => x.NegozioId,
                        principalTable: "Negozi",
                        principalColumn: "Id");
                });

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
                    DataRegistrazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NegozioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clienti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clienti_Negozi_NegozioId",
                        column: x => x.NegozioId,
                        principalTable: "Negozi",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommissioniSartoriali",
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
                    table.PrimaryKey("PK_CommissioniSartoriali", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissioniSartoriali_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommissioniSartoriali_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommissioniSartoriali_Negozi_NegozioId",
                        column: x => x.NegozioId,
                        principalTable: "Negozi",
                        principalColumn: "Id");
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DynamicMeasurementRecords_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicMeasurementRecords_DynamicMeasurementTypes_MeasurementTypeId",
                        column: x => x.MeasurementTypeId,
                        principalTable: "DynamicMeasurementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "RegistroMisure",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    TipoMisura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDynamic = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistroMisure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistroMisure_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommissioniEventi",
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
                    table.PrimaryKey("PK_CommissioniEventi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissioniEventi_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommissioniEventi_CommissioniSartoriali_CommessaSartorialeId",
                        column: x => x.CommessaSartorialeId,
                        principalTable: "CommissioniSartoriali",
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
                        name: "FK_DynamicMeasurementValues_DynamicFieldDefinitions_MeasurementFieldDefinitionId",
                        column: x => x.MeasurementFieldDefinitionId,
                        principalTable: "DynamicFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicMeasurementValues_DynamicMeasurementRecords_DynamicMeasurementRecordId",
                        column: x => x.DynamicMeasurementRecordId,
                        principalTable: "DynamicMeasurementRecords",
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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MisureAbitoCompleto_MisurePantalone_PantaloneId",
                        column: x => x.PantaloneId,
                        principalTable: "MisurePantalone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommissioniMisureLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommessaSartorialeId = table.Column<int>(type: "int", nullable: false),
                    MisuraClienteId = table.Column<int>(type: "int", nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissioniMisureLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissioniMisureLinks_AspNetUsers_LinkedByUserId",
                        column: x => x.LinkedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommissioniMisureLinks_CommissioniSartoriali_CommessaSartorialeId",
                        column: x => x.CommessaSartorialeId,
                        principalTable: "CommissioniSartoriali",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommissioniMisureLinks_RegistroMisure_MisuraClienteId",
                        column: x => x.MisuraClienteId,
                        principalTable: "RegistroMisure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NegozioId",
                table: "AspNetUsers",
                column: "NegozioId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_NegozioId",
                table: "Clienti",
                column: "NegozioId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniEventi_CommessaSartorialeId",
                table: "CommissioniEventi",
                column: "CommessaSartorialeId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniEventi_CreatedByUserId",
                table: "CommissioniEventi",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId",
                table: "CommissioniMisureLinks",
                column: "CommessaSartorialeId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_LinkedByUserId",
                table: "CommissioniMisureLinks",
                column: "LinkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_MisuraClienteId",
                table: "CommissioniMisureLinks",
                column: "MisuraClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniSartoriali_ClienteId",
                table: "CommissioniSartoriali",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniSartoriali_CreatedByUserId",
                table: "CommissioniSartoriali",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniSartoriali_NegozioId",
                table: "CommissioniSartoriali",
                column: "NegozioId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFieldDefinitions_MeasurementTypeId_NomeCampo",
                table: "DynamicFieldDefinitions",
                columns: new[] { "MeasurementTypeId", "NomeCampo" },
                unique: true);

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
                name: "IX_DynamicMeasurementTypes_Nome",
                table: "DynamicMeasurementTypes",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementValues_DynamicMeasurementRecordId",
                table: "DynamicMeasurementValues",
                column: "DynamicMeasurementRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicMeasurementValues_MeasurementFieldDefinitionId",
                table: "DynamicMeasurementValues",
                column: "MeasurementFieldDefinitionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_RegistroMisure_ClienteId",
                table: "RegistroMisure",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CommissioniEventi");

            migrationBuilder.DropTable(
                name: "CommissioniMisureLinks");

            migrationBuilder.DropTable(
                name: "DynamicMeasurementValues");

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
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CommissioniSartoriali");

            migrationBuilder.DropTable(
                name: "RegistroMisure");

            migrationBuilder.DropTable(
                name: "DynamicFieldDefinitions");

            migrationBuilder.DropTable(
                name: "DynamicMeasurementRecords");

            migrationBuilder.DropTable(
                name: "MisureGiacca");

            migrationBuilder.DropTable(
                name: "MisurePantalone");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DynamicMeasurementTypes");

            migrationBuilder.DropTable(
                name: "Clienti");

            migrationBuilder.DropTable(
                name: "Negozi");
        }
    }
}
