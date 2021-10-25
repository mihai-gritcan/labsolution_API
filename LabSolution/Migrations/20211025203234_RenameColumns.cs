using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class RenameColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumericCode",
                table: "ProcessedOrder");

            migrationBuilder.RenameColumn(
                name: "PrefferedLanguage",
                table: "CustomerOrder",
                newName: "TestLanguage");

            migrationBuilder.RenameColumn(
                name: "Placed",
                table: "CustomerOrder",
                newName: "PlacedAt");

            migrationBuilder.AddColumn<int>(
                name: "PrintedTimes",
                table: "ProcessedOrder",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintedTimes",
                table: "ProcessedOrder");

            migrationBuilder.RenameColumn(
                name: "TestLanguage",
                table: "CustomerOrder",
                newName: "PrefferedLanguage");

            migrationBuilder.RenameColumn(
                name: "PlacedAt",
                table: "CustomerOrder",
                newName: "Placed");

            migrationBuilder.AddColumn<long>(
                name: "NumericCode",
                table: "ProcessedOrder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
