using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zelo.Modules.Auto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleInsuranceObligationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceObligationId",
                schema: "auto",
                table: "vehicles",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceObligationId",
                schema: "auto",
                table: "vehicles");
        }
    }
}
