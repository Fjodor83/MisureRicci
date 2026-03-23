using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisureRicci.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserAttivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Attivo",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attivo",
                table: "AspNetUsers");
        }
    }
}
