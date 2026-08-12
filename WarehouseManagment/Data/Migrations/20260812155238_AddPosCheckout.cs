using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddPosCheckout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PosSales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedByUserNameSnapshot = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReversalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReversedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosSales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosSales_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosSaleLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosSaleId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductInventoryId = table.Column<int>(type: "int", nullable: false),
                    ProductSKU = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProductDescriptionSnapshot = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SizeSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosSaleLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosSaleLines_PosSales_PosSaleId",
                        column: x => x.PosSaleId,
                        principalTable: "PosSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosSaleLines_ProductInventory_ProductInventoryId",
                        column: x => x.ProductInventoryId,
                        principalTable: "ProductInventory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosSaleLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PosSaleLines_PosSaleId",
                table: "PosSaleLines",
                column: "PosSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSaleLines_ProductId",
                table: "PosSaleLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSaleLines_ProductInventoryId",
                table: "PosSaleLines",
                column: "ProductInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSales_CreatedByUserId",
                table: "PosSales",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PosSales_CreatedOn",
                table: "PosSales",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PosSales_DocumentNumber",
                table: "PosSales",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PosSales_WarehouseId",
                table: "PosSales",
                column: "WarehouseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PosSaleLines");

            migrationBuilder.DropTable(
                name: "PosSales");
        }
    }
}
