using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zelo.Modules.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNameAndDefaultHousehold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                schema: "identity",
                table: "households",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "identity",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // Households criados antes desta migration ficam todos com
            // IsDefault=false (valor por omissao da coluna nova) - sem
            // dados, DeleteHousehold nao teria para onde redirecionar os
            // itens de quem ja tem households hoje. Promove o household
            // mais antigo de cada utilizador a predefinido.
            migrationBuilder.Sql("""
                UPDATE identity.households h
                SET "IsDefault" = true
                FROM (
                    SELECT DISTINCT ON (hm."UserId") hm."HouseholdId"
                    FROM identity.household_members hm
                    ORDER BY hm."UserId", hm."JoinedAt" ASC
                ) AS oldest
                WHERE h."Id" = oldest."HouseholdId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                schema: "identity",
                table: "households");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "identity",
                table: "AspNetUsers");
        }
    }
}
