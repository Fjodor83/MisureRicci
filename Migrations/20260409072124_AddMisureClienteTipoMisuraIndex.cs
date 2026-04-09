using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddMisureClienteTipoMisuraIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistroMisure_ClienteId",
                table: "RegistroMisure");

            migrationBuilder.AlterColumn<string>(
                name: "TipoMisura",
                table: "RegistroMisure",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroMisure_ClienteId_TipoMisura",
                table: "RegistroMisure",
                columns: new[] { "ClienteId", "TipoMisura" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistroMisure_ClienteId_TipoMisura",
                table: "RegistroMisure");

            migrationBuilder.AlterColumn<string>(
                name: "TipoMisura",
                table: "RegistroMisure",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.CreateIndex(
                name: "IX_RegistroMisure_ClienteId",
                table: "RegistroMisure",
                column: "ClienteId");
        }
    }
}
