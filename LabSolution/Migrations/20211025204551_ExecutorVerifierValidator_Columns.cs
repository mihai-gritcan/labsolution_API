using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class ExecutorVerifierValidator_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExecutorName",
                table: "ProcessedOrder",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidatorName",
                table: "ProcessedOrder",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerifierName",
                table: "ProcessedOrder",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutorName",
                table: "ProcessedOrder");

            migrationBuilder.DropColumn(
                name: "ValidatorName",
                table: "ProcessedOrder");

            migrationBuilder.DropColumn(
                name: "VerifierName",
                table: "ProcessedOrder");
        }
    }
}
