using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zelo.Modules.Auto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "auto",
                table: "vehicles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                schema: "auto",
                table: "vehicles");
        }
    }
}
