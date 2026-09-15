using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zelo.Modules.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_logs",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObligationId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    DaysUntilDue = table.Column<int>(type: "integer", nullable: false),
                    TriggeredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                schema: "core",
                columns: table => new
                {
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    DaysWarning = table.Column<int>(type: "integer", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.HouseholdId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_HouseholdId_AcknowledgedAt",
                schema: "core",
                table: "notification_logs",
                columns: new[] { "HouseholdId", "AcknowledgedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_ObligationId",
                schema: "core",
                table: "notification_logs",
                column: "ObligationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_logs",
                schema: "core");

            migrationBuilder.DropTable(
                name: "notification_preferences",
                schema: "core");
        }
    }
}
