using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class AddTableOrderSyncToGov : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderSyncToGov",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateSynched = table.Column<DateTime>(type: "datetime", nullable: false),
                    TestResultSyncStatus = table.Column<bool>(type: "bit", nullable: true),
                    ProcessedOrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSyncToGov", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSyncToGov_ProcessedOrder_ProcessedOrderId",
                        column: x => x.ProcessedOrderId,
                        principalTable: "ProcessedOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderSyncToGov_ProcessedOrderId",
                table: "OrderSyncToGov",
                column: "ProcessedOrderId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderSyncToGov");
        }
    }
}
