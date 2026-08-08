using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddProductionFinalizationWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinishedGoodsReceiptDocumentNumber",
                table: "ProductionOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialConsumptionDocumentNumber",
                table: "ProductionOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductInventoryId",
                table: "ProductionOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductionFinalizedByUserId",
                table: "ProductionOrders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProductionFinalizedOn",
                table: "ProductionOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductionFinishedGoodsReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductInventoryId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    InventoryMovementId = table.Column<long>(type: "bigint", nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProductSkuSnapshot = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProductDescriptionSnapshot = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SizeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionFinishedGoodsReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionFinishedGoodsReceipts_InventoryMovements_InventoryMovementId",
                        column: x => x.InventoryMovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionFinishedGoodsReceipts_ProductInventory_ProductInventoryId",
                        column: x => x.ProductInventoryId,
                        principalTable: "ProductInventory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionFinishedGoodsReceipts_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionFinishedGoodsReceipts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionFinishedGoodsReceipts_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionFinishedGoodsReceipts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderMaterialConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderMaterialId = table.Column<int>(type: "int", nullable: false),
                    ProductionOrderMaterialAllocationId = table.Column<int>(type: "int", nullable: true),
                    MaterialBatchId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InventoryMovementId = table.Column<long>(type: "bigint", nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    BatchNumberSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LotNumberSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderMaterialConsumptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialConsumptions_InventoryMovements_InventoryMovementId",
                        column: x => x.InventoryMovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialConsumptions_MaterialBatches_MaterialBatchId",
                        column: x => x.MaterialBatchId,
                        principalTable: "MaterialBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialConsumptions_ProductionOrderMaterialAllocations_ProductionOrderMaterialAllocationId",
                        column: x => x.ProductionOrderMaterialAllocationId,
                        principalTable: "ProductionOrderMaterialAllocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialConsumptions_ProductionOrderMaterials_ProductionOrderMaterialId",
                        column: x => x.ProductionOrderMaterialId,
                        principalTable: "ProductionOrderMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialConsumptions_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialConsumptions_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ProductInventoryId",
                table: "ProductionOrders",
                column: "ProductInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionFinishedGoodsReceipts_CreatedOn",
                table: "ProductionFinishedGoodsReceipts",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionFinishedGoodsReceipts_DocumentNumber",
                table: "ProductionFinishedGoodsReceipts",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionFinishedGoodsReceipts_InventoryMovementId",
                table: "ProductionFinishedGoodsReceipts",
                column: "InventoryMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionFinishedGoodsReceipts_ProductId",
                table: "ProductionFinishedGoodsReceipts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionFinishedGoodsReceipts_ProductInventoryId",
                table: "ProductionFinishedGoodsReceipts",
                column: "ProductInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionFinishedGoodsReceipts_ProductionOrderId",
                table: "ProductionFinishedGoodsReceipts",
                column: "ProductionOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionFinishedGoodsReceipts_WarehouseId",
                table: "ProductionFinishedGoodsReceipts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionFinishedGoodsReceipts_WarehouseLocationId",
                table: "ProductionFinishedGoodsReceipts",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialConsumptions_CreatedOn",
                table: "ProductionOrderMaterialConsumptions",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialConsumptions_DocumentNumber",
                table: "ProductionOrderMaterialConsumptions",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialConsumptions_InventoryMovementId",
                table: "ProductionOrderMaterialConsumptions",
                column: "InventoryMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialConsumptions_MaterialBatchId",
                table: "ProductionOrderMaterialConsumptions",
                column: "MaterialBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialConsumptions_ProductionOrderMaterialAllocationId",
                table: "ProductionOrderMaterialConsumptions",
                column: "ProductionOrderMaterialAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialConsumptions_ProductionOrderMaterialId",
                table: "ProductionOrderMaterialConsumptions",
                column: "ProductionOrderMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialConsumptions_WarehouseId",
                table: "ProductionOrderMaterialConsumptions",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialConsumptions_WarehouseLocationId",
                table: "ProductionOrderMaterialConsumptions",
                column: "WarehouseLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_ProductInventory_ProductInventoryId",
                table: "ProductionOrders",
                column: "ProductInventoryId",
                principalTable: "ProductInventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_ProductInventory_ProductInventoryId",
                table: "ProductionOrders");

            migrationBuilder.DropTable(
                name: "ProductionFinishedGoodsReceipts");

            migrationBuilder.DropTable(
                name: "ProductionOrderMaterialConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrders_ProductInventoryId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "FinishedGoodsReceiptDocumentNumber",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "MaterialConsumptionDocumentNumber",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "ProductInventoryId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "ProductionFinalizedByUserId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "ProductionFinalizedOn",
                table: "ProductionOrders");
        }
    }
}
