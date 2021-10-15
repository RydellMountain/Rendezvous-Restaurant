using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AppDevProjectGroup27.Data.Migrations
{
    public partial class AddedTimesToTableBookingHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeApproved",
                table: "TableBookingHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeCheckOut",
                table: "TableBookingHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeRejected",
                table: "TableBookingHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeSitIn",
                table: "TableBookingHeader",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeApproved",
                table: "TableBookingHeader");

            migrationBuilder.DropColumn(
                name: "TimeCheckOut",
                table: "TableBookingHeader");

            migrationBuilder.DropColumn(
                name: "TimeRejected",
                table: "TableBookingHeader");

            migrationBuilder.DropColumn(
                name: "TimeSitIn",
                table: "TableBookingHeader");
        }
    }
}
