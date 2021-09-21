using Microsoft.EntityFrameworkCore.Migrations;

namespace AppDevProjectGroup27.Data.Migrations
{
    public partial class AddedOnSpecialQuantityAndAvaQuantityToMenuItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvaQuantity",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvaQuantity",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "MenuItems");
        }
    }
}
