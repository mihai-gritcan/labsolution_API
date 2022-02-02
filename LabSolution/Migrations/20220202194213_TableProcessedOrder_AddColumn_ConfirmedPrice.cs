using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class TableProcessedOrder_AddColumn_ConfirmedPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConfirmedPrice",
                table: "ProcessedOrder",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedPrice",
                table: "ProcessedOrder");
        }
    }
}
