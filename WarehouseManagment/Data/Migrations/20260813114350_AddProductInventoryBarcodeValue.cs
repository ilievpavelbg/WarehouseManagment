using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddProductInventoryBarcodeValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BarcodeValue",
                table: "ProductInventory",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductInventory_BarcodeValue",
                table: "ProductInventory",
                column: "BarcodeValue",
                unique: true,
                filter: "[BarcodeValue] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductInventory_BarcodeValue",
                table: "ProductInventory");

            migrationBuilder.DropColumn(
                name: "BarcodeValue",
                table: "ProductInventory");
        }
    }
}
