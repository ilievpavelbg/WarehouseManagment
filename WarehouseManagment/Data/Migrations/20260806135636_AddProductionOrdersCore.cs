using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddProductionOrdersCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductProductionProfileId = table.Column<int>(type: "int", nullable: false),
                    BillOfMaterialsId = table.Column<int>(type: "int", nullable: false),
                    ProductRoutingId = table.Column<int>(type: "int", nullable: false),
                    ProductCostCalculationId = table.Column<int>(type: "int", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ProductionUnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    WipWarehouseId = table.Column<int>(type: "int", nullable: false),
                    FinishedGoodsWarehouseId = table.Column<int>(type: "int", nullable: false),
                    ProductSkuSnapshot = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProductDescriptionSnapshot = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProductionNameSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProductionUnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BillOfMaterialsVersionSnapshot = table.Column<int>(type: "int", nullable: false),
                    ProductRoutingVersionSnapshot = table.Column<int>(type: "int", nullable: false),
                    ProductCostCalculationVersionSnapshot = table.Column<int>(type: "int", nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    StartedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CancelledOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_BillsOfMaterials_BillOfMaterialsId",
                        column: x => x.BillOfMaterialsId,
                        principalTable: "BillsOfMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_ProductCostCalculations_ProductCostCalculationId",
                        column: x => x.ProductCostCalculationId,
                        principalTable: "ProductCostCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_ProductProductionProfiles_ProductProductionProfileId",
                        column: x => x.ProductProductionProfileId,
                        principalTable: "ProductProductionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_ProductRoutings_ProductRoutingId",
                        column: x => x.ProductRoutingId,
                        principalTable: "ProductRoutings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_UnitsOfMeasure_ProductionUnitOfMeasureId",
                        column: x => x.ProductionUnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Warehouses_FinishedGoodsWarehouseId",
                        column: x => x.FinishedGoodsWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Warehouses_WipWarehouseId",
                        column: x => x.WipWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductionOperationId = table.Column<int>(type: "int", nullable: false),
                    ProductRoutingStepId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    OperationCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OperationNameSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RequiredRoleSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StandardTimeMinutesSnapshot = table.Column<int>(type: "int", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CompletedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RejectedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrderOperations_ProductionOperations_ProductionOperationId",
                        column: x => x.ProductionOperationId,
                        principalTable: "ProductionOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderOperations_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionOrderOperations_ProductRoutingSteps_ProductRoutingStepId",
                        column: x => x.ProductRoutingStepId,
                        principalTable: "ProductRoutingSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderOperations_ProductionOperationId",
                table: "ProductionOrderOperations",
                column: "ProductionOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderOperations_ProductionOrderId_ProductionOperationId",
                table: "ProductionOrderOperations",
                columns: new[] { "ProductionOrderId", "ProductionOperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderOperations_ProductionOrderId_Sequence",
                table: "ProductionOrderOperations",
                columns: new[] { "ProductionOrderId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderOperations_ProductRoutingStepId",
                table: "ProductionOrderOperations",
                column: "ProductRoutingStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_BillOfMaterialsId",
                table: "ProductionOrders",
                column: "BillOfMaterialsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_FinishedGoodsWarehouseId",
                table: "ProductionOrders",
                column: "FinishedGoodsWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_OrderNumber",
                table: "ProductionOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_PlannedEndDate",
                table: "ProductionOrders",
                column: "PlannedEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_PlannedStartDate",
                table: "ProductionOrders",
                column: "PlannedStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ProductCostCalculationId",
                table: "ProductionOrders",
                column: "ProductCostCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ProductId",
                table: "ProductionOrders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ProductionUnitOfMeasureId",
                table: "ProductionOrders",
                column: "ProductionUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ProductProductionProfileId",
                table: "ProductionOrders",
                column: "ProductProductionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ProductRoutingId",
                table: "ProductionOrders",
                column: "ProductRoutingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_Status",
                table: "ProductionOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_WipWarehouseId",
                table: "ProductionOrders",
                column: "WipWarehouseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionOrderOperations");

            migrationBuilder.DropTable(
                name: "ProductionOrders");
        }
    }
}
