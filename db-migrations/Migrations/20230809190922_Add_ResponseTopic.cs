using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace db_migrations.Migrations
{
    public partial class Add_ResponseTopic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "response_topic",
                table: "platform_events",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "response_topic",
                table: "platform_events");
        }
    }
}
