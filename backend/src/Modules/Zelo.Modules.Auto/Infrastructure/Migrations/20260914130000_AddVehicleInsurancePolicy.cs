using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zelo.Modules.Auto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleInsurancePolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceRenewal",
                schema: "auto",
                table: "vehicles");

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicyNumber",
                schema: "auto",
                table: "vehicles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "InsurancePeriodStart",
                schema: "auto",
                table: "vehicles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "InsurancePeriodEnd",
                schema: "auto",
                table: "vehicles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsurancePremium",
                schema: "auto",
                table: "vehicles",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsurancePolicyNumber",
                schema: "auto",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "InsurancePeriodStart",
                schema: "auto",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "InsurancePeriodEnd",
                schema: "auto",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "InsurancePremium",
                schema: "auto",
                table: "vehicles");

            migrationBuilder.AddColumn<DateOnly>(
                name: "InsuranceRenewal",
                schema: "auto",
                table: "vehicles",
                type: "date",
                nullable: true);
        }
    }
}
