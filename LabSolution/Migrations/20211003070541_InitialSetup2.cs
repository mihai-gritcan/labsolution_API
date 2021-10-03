using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class InitialSetup2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Passeport = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PersonalNumber = table.Column<long>(type: "bigint", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Scheduled = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Placed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TestType = table.Column<short>(type: "smallint", nullable: false),
                    PrefferedLanguage = table.Column<short>(type: "smallint", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerOrder_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrder_CustomerId",
                table: "CustomerOrder",
                column: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerOrder");

            migrationBuilder.DropTable(
                name: "Customer");
        }
    }
}
