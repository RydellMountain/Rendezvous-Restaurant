using Microsoft.EntityFrameworkCore.Migrations;

namespace AppDevProjectGroup27.Data.Migrations
{
    public partial class AddedApprovedAndRejectedByInTableBookingHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableBookingDetails");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "TableBookingHeader",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "TableBookingHeader",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "TableBookingHeader");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "TableBookingHeader");

            migrationBuilder.CreateTable(
                name: "TableBookingDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableBooked = table.Column<int>(type: "int", nullable: false),
                    TableBookingHeaderId = table.Column<int>(type: "int", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
        }
    }
}
