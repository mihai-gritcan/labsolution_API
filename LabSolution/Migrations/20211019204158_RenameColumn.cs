using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class RenameColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "ProcessedOrder");

            migrationBuilder.AddColumn<int>(
                name: "TestResult",
                table: "ProcessedOrder",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestResult",
                table: "ProcessedOrder");

            migrationBuilder.AddColumn<bool>(
                name: "Result",
                table: "ProcessedOrder",
                type: "bit",
                nullable: true);
        }
    }
}
