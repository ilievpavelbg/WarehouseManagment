using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class StabilizePosCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProductSKU",
                table: "Sales",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Sales",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserNameSnapshot",
                table: "Sales",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Sales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Sales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversalReason",
                table: "Sales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversedByUserId",
                table: "Sales",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedOn",
                table: "Sales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippmentBill",
                table: "Couriers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ProductSKU",
                table: "Couriers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Couriers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserNameSnapshot",
                table: "Couriers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Couriers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Couriers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversalReason",
                table: "Couriers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversedByUserId",
                table: "Couriers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedOn",
                table: "Couriers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Couriers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_DocumentNumber",
                table: "Sales",
                column: "DocumentNumber",
                unique: true,
                filter: "[DocumentNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_ProductSKU",
                table: "Sales",
                column: "ProductSKU");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SoldDate",
                table: "Sales",
                column: "SoldDate");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_WarehouseId",
                table: "Sales",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_DocumentNumber",
                table: "Couriers",
                column: "DocumentNumber",
                unique: true,
                filter: "[DocumentNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_ProductSKU",
                table: "Couriers",
                column: "ProductSKU");

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_SendDate",
                table: "Couriers",
                column: "SendDate");

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_ShippmentBill",
                table: "Couriers",
                column: "ShippmentBill");

            migrationBuilder.CreateIndex(
                name: "IX_Couriers_WarehouseId",
                table: "Couriers",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Couriers_Warehouses_WarehouseId",
                table: "Couriers",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Warehouses_WarehouseId",
                table: "Sales",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Couriers_Warehouses_WarehouseId",
                table: "Couriers");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Warehouses_WarehouseId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_DocumentNumber",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_ProductSKU",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_SoldDate",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_WarehouseId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Couriers_DocumentNumber",
                table: "Couriers");

            migrationBuilder.DropIndex(
                name: "IX_Couriers_ProductSKU",
                table: "Couriers");

            migrationBuilder.DropIndex(
                name: "IX_Couriers_SendDate",
                table: "Couriers");

            migrationBuilder.DropIndex(
                name: "IX_Couriers_ShippmentBill",
                table: "Couriers");

            migrationBuilder.DropIndex(
                name: "IX_Couriers_WarehouseId",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedByUserNameSnapshot",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ReversalReason",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ReversedByUserId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "ReversedOn",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserNameSnapshot",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "ReversalReason",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "ReversedByUserId",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "ReversedOn",
                table: "Couriers");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Couriers");

            migrationBuilder.AlterColumn<string>(
                name: "ProductSKU",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ShippmentBill",
                table: "Couriers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProductSKU",
                table: "Couriers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
