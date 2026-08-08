using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddProductionMaterialTransfer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaterialsTransferDocumentNumber",
                table: "ProductionOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialsTransferredByUserId",
                table: "ProductionOrders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MaterialsTransferredOn",
                table: "ProductionOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductionOrderMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    BillOfMaterialLineId = table.Column<int>(type: "int", nullable: true),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    MaterialCodeSnapshot = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MaterialNameSnapshot = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    QuantityPerUnitSnapshot = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    WastePercentSnapshot = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TransferredQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ConsumedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReturnedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferredOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterials_BillOfMaterialLines_BillOfMaterialLineId",
                        column: x => x.BillOfMaterialLineId,
                        principalTable: "BillOfMaterialLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterials_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterials_UnitsOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderMaterialAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderMaterialId = table.Column<int>(type: "int", nullable: false),
                    MaterialBatchId = table.Column<int>(type: "int", nullable: true),
                    SourceMaterialStockId = table.Column<int>(type: "int", nullable: true),
                    SourceWarehouseId = table.Column<int>(type: "int", nullable: false),
                    SourceWarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: false),
                    DestinationWarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    BatchNumberSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LotNumberSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InventoryMovementId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderMaterialAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialAllocations_InventoryMovements_InventoryMovementId",
                        column: x => x.InventoryMovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialAllocations_MaterialBatches_MaterialBatchId",
                        column: x => x.MaterialBatchId,
                        principalTable: "MaterialBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialAllocations_MaterialStocks_SourceMaterialStockId",
                        column: x => x.SourceMaterialStockId,
                        principalTable: "MaterialStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialAllocations_ProductionOrderMaterials_ProductionOrderMaterialId",
                        column: x => x.ProductionOrderMaterialId,
                        principalTable: "ProductionOrderMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialAllocations_WarehouseLocations_DestinationWarehouseLocationId",
                        column: x => x.DestinationWarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialAllocations_WarehouseLocations_SourceWarehouseLocationId",
                        column: x => x.SourceWarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialAllocations_Warehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderMaterialAllocations_Warehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialAllocations_DestinationWarehouseId",
                table: "ProductionOrderMaterialAllocations",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialAllocations_DestinationWarehouseLocationId",
                table: "ProductionOrderMaterialAllocations",
                column: "DestinationWarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialAllocations_InventoryMovementId",
                table: "ProductionOrderMaterialAllocations",
                column: "InventoryMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialAllocations_MaterialBatchId",
                table: "ProductionOrderMaterialAllocations",
                column: "MaterialBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialAllocations_ProductionOrderMaterialId",
                table: "ProductionOrderMaterialAllocations",
                column: "ProductionOrderMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialAllocations_SourceMaterialStockId",
                table: "ProductionOrderMaterialAllocations",
                column: "SourceMaterialStockId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialAllocations_SourceWarehouseId",
                table: "ProductionOrderMaterialAllocations",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterialAllocations_SourceWarehouseLocationId",
                table: "ProductionOrderMaterialAllocations",
                column: "SourceWarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterials_BillOfMaterialLineId",
                table: "ProductionOrderMaterials",
                column: "BillOfMaterialLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterials_MaterialId",
                table: "ProductionOrderMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterials_ProductionOrderId_MaterialId",
                table: "ProductionOrderMaterials",
                columns: new[] { "ProductionOrderId", "MaterialId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderMaterials_UnitOfMeasureId",
                table: "ProductionOrderMaterials",
                column: "UnitOfMeasureId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionOrderMaterialAllocations");

            migrationBuilder.DropTable(
                name: "ProductionOrderMaterials");

            migrationBuilder.DropColumn(
                name: "MaterialsTransferDocumentNumber",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "MaterialsTransferredByUserId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "MaterialsTransferredOn",
                table: "ProductionOrders");
        }
    }
}
