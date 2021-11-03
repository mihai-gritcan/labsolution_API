using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class AddTable_ProcessedOrderPdf : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedOrderPdf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreated = table.Column<DateTime>(type: "datetime", nullable: false),
                    PdfBytes = table.Column<byte>(type: "tinyint", nullable: false),
                    ProcessedOrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedOrderPdf", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessedOrderPdf_ProcessedOrder_ProcessedOrderId",
                        column: x => x.ProcessedOrderId,
                        principalTable: "ProcessedOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedOrderPdf_ProcessedOrderId",
                table: "ProcessedOrderPdf",
                column: "ProcessedOrderId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedOrderPdf");
        }
    }
}
