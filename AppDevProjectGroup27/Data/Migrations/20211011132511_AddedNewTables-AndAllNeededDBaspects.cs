using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AppDevProjectGroup27.Data.Migrations
{
    public partial class AddedNewTablesAndAllNeededDBaspects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeatingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxTables = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TableBookingHeader",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateBookingMade = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SitInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SitInTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableBookingHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableBookingHeader_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableTrack",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    AmtAva = table.Column<int>(type: "int", nullable: false),
                    DateTable = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeTable = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableTrack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableTrack_Table_TableId",
                        column: x => x.TableId,
                        principalTable: "Table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableBookingDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableBookingHeaderId = table.Column<int>(type: "int", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableBooked = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableBookingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableBookingDetails_TableBookingHeader_TableBookingHeaderId",
                        column: x => x.TableBookingHeaderId,
                        principalTable: "TableBookingHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableBookingDetails_TableBookingHeaderId",
                table: "TableBookingDetails",
                column: "TableBookingHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_TableBookingHeader_UserId",
                table: "TableBookingHeader",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TableTrack_TableId",
                table: "TableTrack",
                column: "TableId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableBookingDetails");

            migrationBuilder.DropTable(
                name: "TableTrack");

            migrationBuilder.DropTable(
                name: "TableBookingHeader");

            migrationBuilder.DropTable(
                name: "Table");
        }
    }
}
