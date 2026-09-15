using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zelo.Modules.Auto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehiclePhotoObjectKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoObjectKey",
                schema: "auto",
                table: "vehicles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoObjectKey",
                schema: "auto",
                table: "vehicles");
        }
    }
}
