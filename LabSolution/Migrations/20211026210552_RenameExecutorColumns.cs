using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class RenameExecutorColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VerifierName",
                table: "ProcessedOrder",
                newName: "ValidatedBy");

            migrationBuilder.RenameColumn(
                name: "ValidatorName",
                table: "ProcessedOrder",
                newName: "ProcessedBy");

            migrationBuilder.RenameColumn(
                name: "ExecutorName",
                table: "ProcessedOrder",
                newName: "CheckedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValidatedBy",
                table: "ProcessedOrder",
                newName: "VerifierName");

            migrationBuilder.RenameColumn(
                name: "ProcessedBy",
                table: "ProcessedOrder",
                newName: "ValidatorName");

            migrationBuilder.RenameColumn(
                name: "CheckedBy",
                table: "ProcessedOrder",
                newName: "ExecutorName");
        }
    }
}
