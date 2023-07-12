using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace db_migrations.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cvops_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    jwt_subject = table.Column<string>(type: "text", nullable: false),
                    default_workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_created = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    date_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_modified = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cvops_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inference_results",
                columns: table => new
                {
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_type = table.Column<int>(type: "integer", nullable: false),
                    boxes = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    meshes = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    labels = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "platform_events",
                columns: table => new
                {
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_data = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "workspace",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_created = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    date_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_modified = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    device_info = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    salt = table.Column<byte[]>(type: "bytea", nullable: false),
                    hash = table.Column<string>(type: "text", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activation_status = table.Column<string>(type: "text", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_created = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    date_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_modified = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device", x => x.id);
                    table.ForeignKey(
                        name: "fk_device_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workspace_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_user_role = table.Column<string>(type: "text", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_created = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    date_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_modified = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_workspace_user_cvops_user_user_id",
                        column: x => x.user_id,
                        principalTable: "cvops_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_workspace_user_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cvops_user_email",
                table: "cvops_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cvops_user_jwt_subject",
                table: "cvops_user",
                column: "jwt_subject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_id",
                table: "device",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_device_workspace_id",
                table: "device",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_inference_results_device_id",
                table: "inference_results",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_inference_results_result_type",
                table: "inference_results",
                column: "result_type");

            migrationBuilder.CreateIndex(
                name: "IX_inference_results_workspace_id",
                table: "inference_results",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_events_device_id",
                table: "platform_events",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_events_event_type",
                table: "platform_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_platform_events_user_id",
                table: "platform_events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_events_workspace_id",
                table: "platform_events",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_user_user_id",
                table: "workspace_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_user_workspace_id",
                table: "workspace_user",
                column: "workspace_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device");

            migrationBuilder.DropTable(
                name: "inference_results");

            migrationBuilder.DropTable(
                name: "platform_events");

            migrationBuilder.DropTable(
                name: "workspace_user");

            migrationBuilder.DropTable(
                name: "cvops_user");

            migrationBuilder.DropTable(
                name: "workspace");
        }
    }
}
