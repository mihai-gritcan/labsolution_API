using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LabSolution.Migrations
{
    public partial class doNotSaveBarcodeAndQRCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "ProcessedOrder");

            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "ProcessedOrder");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Barcode",
                table: "ProcessedOrder",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "QRCode",
                table: "ProcessedOrder",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
