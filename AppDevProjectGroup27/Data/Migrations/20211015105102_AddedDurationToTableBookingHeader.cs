using Microsoft.EntityFrameworkCore.Migrations;

namespace AppDevProjectGroup27.Data.Migrations
{
    public partial class AddedDurationToTableBookingHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "TableBookingHeader",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "TableBookingHeader");
        }
    }
}
