using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class FixColumnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Passeport",
                table: "Customer",
                newName: "Passport");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Passport",
                table: "Customer",
                newName: "Passeport");
        }
    }
}
