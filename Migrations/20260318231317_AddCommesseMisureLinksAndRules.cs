using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissioniMisureLinksAndRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_CommessaSartorialeId_MisuraClienteId",
                table: "CommissioniMisureLinks",
                columns: new[] { "CommessaSartorialeId", "MisuraClienteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_LinkedByUserId",
                table: "CommissioniMisureLinks",
                column: "LinkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissioniMisureLinks_MisuraClienteId",
                table: "CommissioniMisureLinks",
                column: "MisuraClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissioniMisureLinks");
        }
    }
}
