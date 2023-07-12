using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace db_migrations.Migrations
{
    public partial class CreateHyperTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SELECT create_hypertable(
                    'inference_results',
                    'time'
                );
                SELECT create_hypertable(
                    'platform_events',
                    'time'
                );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
