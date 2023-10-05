using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace db_migrations.Migrations
{
    public partial class DeploymentUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "source",
                table: "deployment",
                newName: "model_source");

            migrationBuilder.AddColumn<Guid>(
                name: "deployment_initiator_id",
                table: "deployment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "deployment_initiator_type",
                table: "deployment",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deployment_initiator_id",
                table: "deployment");

            migrationBuilder.DropColumn(
                name: "deployment_initiator_type",
                table: "deployment");

            migrationBuilder.RenameColumn(
                name: "model_source",
                table: "deployment",
                newName: "source");
        }
    }
}
