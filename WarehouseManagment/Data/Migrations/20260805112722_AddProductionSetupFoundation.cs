using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagment.Data.Migrations
{
    public partial class AddProductionSetupFoundation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillsOfMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HasBeenActivated = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillsOfMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillsOfMaterials_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CostComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDirectCost = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemCalculated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCostCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HasBeenActivated = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCostCalculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCostCalculations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DefaultSequence = table.Column<int>(type: "int", nullable: false),
                    RequiredRole = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOperations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductionProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProductionUnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    StandardProductionQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductionProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductProductionProfiles_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductProductionProfiles_UnitsOfMeasure_ProductionUnitOfMeasureId",
                        column: x => x.ProductionUnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductRoutings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HasBeenActivated = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRoutings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRoutings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillOfMaterialLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillOfMaterialsId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    QuantityPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    WastePercent = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfMaterialLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillOfMaterialLines_BillsOfMaterials_BillOfMaterialsId",
                        column: x => x.BillOfMaterialsId,
                        principalTable: "BillsOfMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillOfMaterialLines_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillOfMaterialLines_UnitsOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductCostCalculationLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCostCalculationId = table.Column<int>(type: "int", nullable: false),
                    CostComponentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCostCalculationLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCostCalculationLines_CostComponents_CostComponentId",
                        column: x => x.CostComponentId,
                        principalTable: "CostComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductCostCalculationLines_ProductCostCalculations_ProductCostCalculationId",
                        column: x => x.ProductCostCalculationId,
                        principalTable: "ProductCostCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductRoutingSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductRoutingId = table.Column<int>(type: "int", nullable: false),
                    ProductionOperationId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    StandardTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRoutingSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRoutingSteps_ProductionOperations_ProductionOperationId",
                        column: x => x.ProductionOperationId,
                        principalTable: "ProductionOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductRoutingSteps_ProductRoutings_ProductRoutingId",
                        column: x => x.ProductRoutingId,
                        principalTable: "ProductRoutings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialLines_BillOfMaterialsId_MaterialId",
                table: "BillOfMaterialLines",
                columns: new[] { "BillOfMaterialsId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialLines_MaterialId",
                table: "BillOfMaterialLines",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialLines_UnitOfMeasureId",
                table: "BillOfMaterialLines",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfMaterials_ProductId",
                table: "BillsOfMaterials",
                column: "ProductId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_BillsOfMaterials_ProductId_Version",
                table: "BillsOfMaterials",
                columns: new[] { "ProductId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CostComponents_Code",
                table: "CostComponents",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostCalculationLines_CostComponentId",
                table: "ProductCostCalculationLines",
                column: "CostComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostCalculationLines_ProductCostCalculationId_CostComponentId",
                table: "ProductCostCalculationLines",
                columns: new[] { "ProductCostCalculationId", "CostComponentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostCalculations_ProductId",
                table: "ProductCostCalculations",
                column: "ProductId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostCalculations_ProductId_Version",
                table: "ProductCostCalculations",
                columns: new[] { "ProductId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOperations_Code",
                table: "ProductionOperations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductionProfiles_ProductId",
                table: "ProductProductionProfiles",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductionProfiles_ProductionUnitOfMeasureId",
                table: "ProductProductionProfiles",
                column: "ProductionUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRoutings_ProductId",
                table: "ProductRoutings",
                column: "ProductId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRoutings_ProductId_Version",
                table: "ProductRoutings",
                columns: new[] { "ProductId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRoutingSteps_ProductionOperationId",
                table: "ProductRoutingSteps",
                column: "ProductionOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRoutingSteps_ProductRoutingId_ProductionOperationId",
                table: "ProductRoutingSteps",
                columns: new[] { "ProductRoutingId", "ProductionOperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRoutingSteps_ProductRoutingId_Sequence",
                table: "ProductRoutingSteps",
                columns: new[] { "ProductRoutingId", "Sequence" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillOfMaterialLines");

            migrationBuilder.DropTable(
                name: "ProductCostCalculationLines");

            migrationBuilder.DropTable(
                name: "ProductProductionProfiles");

            migrationBuilder.DropTable(
                name: "ProductRoutingSteps");

            migrationBuilder.DropTable(
                name: "BillsOfMaterials");

            migrationBuilder.DropTable(
                name: "CostComponents");

            migrationBuilder.DropTable(
                name: "ProductCostCalculations");

            migrationBuilder.DropTable(
                name: "ProductionOperations");

            migrationBuilder.DropTable(
                name: "ProductRoutings");
        }
    }
}
