using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace db_migrations.Migrations
{
    public partial class Deployment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deployment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bucket_name = table.Column<string>(type: "text", nullable: false),
                    object_name = table.Column<string>(type: "text", nullable: false),
                    model_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    devices_status = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    model_metadata = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_created = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    date_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_modified = table.Column<Guid>(type: "uuid", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deployment", x => x.id);
                    table.ForeignKey(
                        name: "fk_deployment_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_deployment_workspace_id",
                table: "deployment",
                column: "workspace_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deployment");
        }
    }
}
