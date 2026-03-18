using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddClienteNegozioId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NegozioId",
                table: "Clienti",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clienti_Negozi_NegozioId",
                table: "Clienti");

            migrationBuilder.DropIndex(
                name: "IX_Clienti_NegozioId",
                table: "Clienti");

            migrationBuilder.DropColumn(
                name: "NegozioId",
                table: "Clienti");
        }
    }
}
