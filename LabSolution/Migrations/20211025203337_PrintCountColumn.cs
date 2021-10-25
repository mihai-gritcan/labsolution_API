using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class PrintCountColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrintedTimes",
                table: "ProcessedOrder",
                newName: "PrintCount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrintCount",
                table: "ProcessedOrder",
                newName: "PrintedTimes");
        }
    }
}
