using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxuryCo.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSeccionToProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "seccion",
                table: "producto",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "seccion",
                table: "producto");
        }
    }
}
