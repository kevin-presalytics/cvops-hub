using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace db_migrations.Migrations
{
    public partial class TimescaleDBTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_device_name",
                table: "device");

            migrationBuilder.CreateTable(
                name: "inference_results",
                columns: table => new
                {
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultType = table.Column<int>(type: "integer", nullable: false),
                    Boxes = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Meshes = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Labels = table.Column<JsonDocument>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "platform_events",
                columns: table => new
                {
                    Time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventData = table.Column<JsonDocument>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_inference_results_DeviceId",
                table: "inference_results",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_inference_results_ResultType",
                table: "inference_results",
                column: "ResultType");

            migrationBuilder.CreateIndex(
                name: "IX_inference_results_WorkspaceId",
                table: "inference_results",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_events_DeviceId",
                table: "platform_events",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_events_EventType",
                table: "platform_events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_platform_events_UserId",
                table: "platform_events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_events_WorkspaceId",
                table: "platform_events",
                column: "WorkspaceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inference_results");

            migrationBuilder.DropTable(
                name: "platform_events");

            migrationBuilder.CreateIndex(
                name: "IX_device_name",
                table: "device",
                column: "name",
                unique: true);
        }
    }
}
