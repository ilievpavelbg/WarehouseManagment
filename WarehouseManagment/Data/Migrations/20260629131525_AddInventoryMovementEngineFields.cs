using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddInventoryMovementEngineFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProductInventoryId",
                table: "InventoryMovements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
     name: "CreatedOn",
     table: "InventoryMovements",
     type: "datetime2",
     nullable: false,
     defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<long>(
                name: "ReferenceId",
                table: "InventoryMovements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "InventoryMovements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "InventoryMovements");

            migrationBuilder.AlterColumn<int>(
                name: "ProductInventoryId",
                table: "InventoryMovements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
