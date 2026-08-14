using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddBarcodeMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BarcodeGeneratedByUserId",
                table: "ProductInventory",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BarcodeGeneratedByUserNameSnapshot",
                table: "ProductInventory",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BarcodeGeneratedOn",
                table: "ProductInventory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BarcodePrintCount",
                table: "ProductInventory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BarcodePrintedOn",
                table: "ProductInventory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BarcodeType",
                table: "ProductInventory",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarcodeGeneratedByUserId",
                table: "ProductInventory");

            migrationBuilder.DropColumn(
                name: "BarcodeGeneratedByUserNameSnapshot",
                table: "ProductInventory");

            migrationBuilder.DropColumn(
                name: "BarcodeGeneratedOn",
                table: "ProductInventory");

            migrationBuilder.DropColumn(
                name: "BarcodePrintCount",
                table: "ProductInventory");

            migrationBuilder.DropColumn(
                name: "BarcodePrintedOn",
                table: "ProductInventory");

            migrationBuilder.DropColumn(
                name: "BarcodeType",
                table: "ProductInventory");
        }
    }
}
