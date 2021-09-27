using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AppDevProjectGroup27.Data.Migrations
{
    public partial class AddedAttributesToMenuItemAndOrderHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompleteDateTime",
                table: "OrderHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedTimeComplete",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpOrder",
                table: "OrderHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "OrderHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ETAEstimate",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompleteDateTime",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "EstimatedTimeComplete",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "PickedUpOrder",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "ETAEstimate",
                table: "MenuItems");
        }
    }
}
