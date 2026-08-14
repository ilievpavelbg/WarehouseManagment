using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WarehouseManagment.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductInventory> ProductInventory { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Courier> Couriers { get; set; }
        public DbSet<PosSale> PosSales { get; set; }
        public DbSet<PosSaleLine> PosSaleLines { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseZone> WarehouseZones { get; set; }
        public DbSet<WarehouseLocation> WarehouseLocations { get; set; }
        public DbSet<WarehouseSettings> WarehouseSettings { get; set; }
        public DbSet<DocumentSequence> DocumentSequences { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialCategory> MaterialCategories { get; set; }
        public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<MaterialBatch> MaterialBatches { get; set; }
        public DbSet<MaterialStock> MaterialStocks { get; set; }
        public DbSet<ProductProductionProfile> ProductProductionProfiles { get; set; }
        public DbSet<CostComponent> CostComponents { get; set; }
        public DbSet<ProductCostCalculation> ProductCostCalculations { get; set; }
        public DbSet<ProductCostCalculationLine> ProductCostCalculationLines { get; set; }
        public DbSet<BillOfMaterials> BillsOfMaterials { get; set; }
        public DbSet<BillOfMaterialLine> BillOfMaterialLines { get; set; }
        public DbSet<ProductionOperation> ProductionOperations { get; set; }
        public DbSet<ProductRouting> ProductRoutings { get; set; }
        public DbSet<ProductRoutingStep> ProductRoutingSteps { get; set; }
        public DbSet<ProductionOrder> ProductionOrders { get; set; } = null!;
        public DbSet<ProductionOrderOperation> ProductionOrderOperations { get; set; } = null!;
        public DbSet<ProductionOrderMaterial> ProductionOrderMaterials { get; set; } = null!;
        public DbSet<ProductionOrderMaterialAllocation> ProductionOrderMaterialAllocations { get; set; } = null!;
        public DbSet<ProductionOrderMaterialConsumption> ProductionOrderMaterialConsumptions { get; set; } = null!;
        public DbSet<ProductionFinishedGoodsReceipt> ProductionFinishedGoodsReceipts { get; set; } = null!;
        public DbSet<ProductionWorkEntry> ProductionWorkEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            builder.Entity<ProductInventory>()
                .HasIndex(x => x.BarcodeValue)
                .IsUnique()
                .HasFilter("[BarcodeValue] IS NOT NULL");

            builder.Entity<ProductInventory>()
                .Property(x => x.BarcodeValue)
                .HasMaxLength(32);

            builder.Entity<ProductInventory>()
                .Property(x => x.BarcodeType)
                .HasMaxLength(32);

            builder.Entity<ProductInventory>()
                .Property(x => x.BarcodeGeneratedByUserId)
                .HasMaxLength(450);

            builder.Entity<ProductInventory>()
                .Property(x => x.BarcodeGeneratedByUserNameSnapshot)
                .HasMaxLength(256);

            builder.Entity<ProductInventory>()
                .Property(x => x.BarcodePrintCount)
                .HasDefaultValue(0);

            builder.Entity<Sale>()
                .HasIndex(x => x.DocumentNumber)
                .IsUnique();

            builder.Entity<Sale>()
                .HasIndex(x => x.SoldDate);

            builder.Entity<Sale>()
                .HasIndex(x => x.ProductSKU);

            builder.Entity<Sale>()
                .Property(x => x.DocumentNumber)
                .HasMaxLength(100);

            builder.Entity<Sale>()
                .Property(x => x.CreatedByUserId)
                .HasMaxLength(450);

            builder.Entity<Sale>()
                .Property(x => x.CreatedByUserNameSnapshot)
                .HasMaxLength(256);

            builder.Entity<Sale>()
                .Property(x => x.ReversalReason)
                .HasMaxLength(500);

            builder.Entity<Sale>()
                .Property(x => x.ReversedByUserId)
                .HasMaxLength(450);

            builder.Entity<Sale>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Courier>()
                .HasIndex(x => x.DocumentNumber)
                .IsUnique();

            builder.Entity<Courier>()
                .HasIndex(x => x.SendDate);

            builder.Entity<Courier>()
                .HasIndex(x => x.ProductSKU);

            builder.Entity<Courier>()
                .HasIndex(x => x.ShippmentBill);

            builder.Entity<Courier>()
                .Property(x => x.DocumentNumber)
                .HasMaxLength(100);

            builder.Entity<Courier>()
                .Property(x => x.CreatedByUserId)
                .HasMaxLength(450);

            builder.Entity<Courier>()
                .Property(x => x.CreatedByUserNameSnapshot)
                .HasMaxLength(256);

            builder.Entity<Courier>()
                .Property(x => x.ReversalReason)
                .HasMaxLength(500);

            builder.Entity<Courier>()
                .Property(x => x.ReversedByUserId)
                .HasMaxLength(450);

            builder.Entity<Courier>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PosSale>()
                .HasIndex(x => x.DocumentNumber)
                .IsUnique();

            builder.Entity<PosSale>()
                .HasIndex(x => x.CreatedOn);

            builder.Entity<PosSale>()
                .HasIndex(x => x.CreatedByUserId);

            builder.Entity<PosSale>()
                .Property(x => x.DocumentNumber)
                .HasMaxLength(100);

            builder.Entity<PosSale>()
                .Property(x => x.CreatedByUserId)
                .HasMaxLength(450);

            builder.Entity<PosSale>()
                .Property(x => x.CreatedByUserNameSnapshot)
                .HasMaxLength(256);

            builder.Entity<PosSale>()
                .Property(x => x.ReversalReason)
                .HasMaxLength(500);

            builder.Entity<PosSale>()
                .Property(x => x.ReversedByUserId)
                .HasMaxLength(450);

            builder.Entity<PosSale>()
                .Property(x => x.Subtotal)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PosSale>()
                .Property(x => x.DiscountTotal)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PosSale>()
                .Property(x => x.Total)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PosSale>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PosSaleLine>()
                .HasIndex(x => x.PosSaleId);

            builder.Entity<PosSaleLine>()
                .HasIndex(x => x.ProductInventoryId);

            builder.Entity<PosSaleLine>()
                .Property(x => x.ProductSKU)
                .HasMaxLength(128);

            builder.Entity<PosSaleLine>()
                .Property(x => x.ProductDescriptionSnapshot)
                .HasMaxLength(500);

            builder.Entity<PosSaleLine>()
                .Property(x => x.SizeSnapshot)
                .HasMaxLength(100);

            builder.Entity<PosSaleLine>()
                .Property(x => x.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PosSaleLine>()
                .Property(x => x.DiscountPercent)
                .HasColumnType("decimal(9,2)");

            builder.Entity<PosSaleLine>()
                .Property(x => x.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PosSaleLine>()
                .Property(x => x.LineTotal)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PosSaleLine>()
                .HasOne(x => x.PosSale)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.PosSaleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PosSaleLine>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PosSaleLine>()
                .HasOne(x => x.ProductInventory)
                .WithMany()
                .HasForeignKey(x => x.ProductInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Warehouse>()
                .HasIndex(w => w.Code)
                .IsUnique();

            builder.Entity<DocumentSequence>()
                .HasIndex(x => new { x.DocumentType, x.Year })
                .IsUnique();

            builder.Entity<AuditLog>()
                .HasIndex(x => x.CreatedOn);

            builder.Entity<AuditLog>()
                .HasIndex(x => x.UserId);

            builder.Entity<AuditLog>()
                .HasIndex(x => x.ActionType);

            builder.Entity<AuditLog>()
                .HasIndex(x => x.EntityType);

            builder.Entity<AuditLog>()
                .HasIndex(x => x.DocumentNumber);

            builder.Entity<WarehouseSettings>()
                .HasOne(x => x.DefaultMaterialWarehouse)
                .WithMany()
                .HasForeignKey(x => x.DefaultMaterialWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseSettings>()
                .HasOne(x => x.DefaultWipWarehouse)
                .WithMany()
                .HasForeignKey(x => x.DefaultWipWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseSettings>()
                .HasOne(x => x.DefaultFinishedGoodsWarehouse)
                .WithMany()
                .HasForeignKey(x => x.DefaultFinishedGoodsWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseZone>()
                .HasIndex(z => new { z.WarehouseId, z.Code })
                .IsUnique();

            builder.Entity<WarehouseLocation>()
                .HasIndex(l => new { l.WarehouseId, l.Code })
                .IsUnique();

            builder.Entity<WarehouseZone>()
                .HasOne(z => z.Warehouse)
                .WithMany(w => w.Zones)
                .HasForeignKey(z => z.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseLocation>()
                .HasOne(l => l.Warehouse)
                .WithMany(w => w.Locations)
                .HasForeignKey(l => l.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseLocation>()
                .HasOne(l => l.WarehouseZone)
                .WithMany(z => z.Locations)
                .HasForeignKey(l => l.WarehouseZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialCategory>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<UnitOfMeasure>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<Supplier>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<Material>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<Material>()
                .HasIndex(x => x.Barcode);

            builder.Entity<Material>()
                .Property(x => x.StandardCost)
                .HasColumnType("decimal(18,4)");

            builder.Entity<Material>()
                .Property(x => x.MinimumStock)
                .HasColumnType("decimal(18,4)");

            builder.Entity<Material>()
                .HasOne(x => x.MaterialCategory)
                .WithMany(x => x.Materials)
                .HasForeignKey(x => x.MaterialCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Material>()
                .HasOne(x => x.UnitOfMeasure)
                .WithMany(x => x.Materials)
                .HasForeignKey(x => x.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Material>()
                .HasOne(x => x.Supplier)
                .WithMany(x => x.Materials)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialBatch>()
                .HasIndex(x => new { x.MaterialId, x.BatchNumber });

            builder.Entity<MaterialBatch>()
                .HasIndex(x => new { x.MaterialId, x.LotNumber });

            builder.Entity<MaterialBatch>()
                .Property(x => x.StandardCost)
                .HasColumnType("decimal(18,4)");

            builder.Entity<MaterialBatch>()
                .HasOne(x => x.Material)
                .WithMany(x => x.MaterialBatches)
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialBatch>()
                .HasOne(x => x.Supplier)
                .WithMany(x => x.MaterialBatches)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialStock>()
                .Property(x => x.Quantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<MaterialStock>()
                .HasIndex(x => new { x.MaterialId, x.WarehouseId, x.WarehouseLocationId, x.MaterialBatchId })
                .IsUnique();

            builder.Entity<MaterialStock>()
                .HasOne(x => x.Material)
                .WithMany(x => x.MaterialStocks)
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialStock>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialStock>()
                .HasOne(x => x.WarehouseLocation)
                .WithMany()
                .HasForeignKey(x => x.WarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialStock>()
                .HasOne(x => x.MaterialBatch)
                .WithMany()
                .HasForeignKey(x => x.MaterialBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .Property(m => m.Quantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.MovementDate);

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.ProductId);

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.ProductInventoryId);

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.MaterialId);

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.MaterialBatchId);

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.WarehouseId);

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.WarehouseLocationId);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.Product)
                .WithMany()
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.ProductInventory)
                .WithMany()
                .HasForeignKey(m => m.ProductInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.Material)
                .WithMany(x => x.InventoryMovements)
                .HasForeignKey(m => m.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.MaterialBatch)
                .WithMany(x => x.InventoryMovements)
                .HasForeignKey(m => m.MaterialBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.Warehouse)
                .WithMany(w => w.InventoryMovements)
                .HasForeignKey(m => m.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.WarehouseZone)
                .WithMany(z => z.InventoryMovements)
                .HasForeignKey(m => m.WarehouseZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.WarehouseLocation)
                .WithMany(l => l.InventoryMovements)
                .HasForeignKey(m => m.WarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.DestinationWarehouse)
                .WithMany()
                .HasForeignKey(m => m.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.DestinationWarehouseZone)
                .WithMany()
                .HasForeignKey(m => m.DestinationWarehouseZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryMovement>()
                .HasOne(m => m.DestinationWarehouseLocation)
                .WithMany()
                .HasForeignKey(m => m.DestinationWarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductProductionProfile>()
                .HasIndex(x => x.ProductId)
                .IsUnique();

            builder.Entity<ProductProductionProfile>()
                .Property(x => x.StandardProductionQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductProductionProfile>()
                .HasOne(x => x.Product)
                .WithOne()
                .HasForeignKey<ProductProductionProfile>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductProductionProfile>()
                .HasOne(x => x.ProductionUnitOfMeasure)
                .WithMany()
                .HasForeignKey(x => x.ProductionUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CostComponent>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<ProductCostCalculation>()
                .HasIndex(x => new { x.ProductId, x.Version })
                .IsUnique();

            builder.Entity<ProductCostCalculation>()
                .HasIndex(x => x.ProductId)
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            builder.Entity<ProductCostCalculation>()
                .Property(x => x.TotalCost)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductCostCalculation>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductCostCalculationLine>()
                .HasIndex(x => new { x.ProductCostCalculationId, x.CostComponentId })
                .IsUnique();

            builder.Entity<ProductCostCalculationLine>()
                .Property(x => x.Amount)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductCostCalculationLine>()
                .HasOne(x => x.ProductCostCalculation)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.ProductCostCalculationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductCostCalculationLine>()
                .HasOne(x => x.CostComponent)
                .WithMany(x => x.ProductCostCalculationLines)
                .HasForeignKey(x => x.CostComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BillOfMaterials>()
                .HasIndex(x => new { x.ProductId, x.Version })
                .IsUnique();

            builder.Entity<BillOfMaterials>()
                .HasIndex(x => x.ProductId)
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            builder.Entity<BillOfMaterials>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BillOfMaterialLine>()
                .HasIndex(x => new { x.BillOfMaterialsId, x.MaterialId })
                .IsUnique();

            builder.Entity<BillOfMaterialLine>()
                .Property(x => x.QuantityPerUnit)
                .HasColumnType("decimal(18,4)");

            builder.Entity<BillOfMaterialLine>()
                .Property(x => x.WastePercent)
                .HasColumnType("decimal(18,4)");

            builder.Entity<BillOfMaterialLine>()
                .HasOne(x => x.BillOfMaterials)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.BillOfMaterialsId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BillOfMaterialLine>()
                .HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BillOfMaterialLine>()
                .HasOne(x => x.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(x => x.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOperation>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<ProductRouting>()
                .HasIndex(x => new { x.ProductId, x.Version })
                .IsUnique();

            builder.Entity<ProductRouting>()
                .HasIndex(x => x.ProductId)
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            builder.Entity<ProductRouting>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductRoutingStep>()
                .HasIndex(x => new { x.ProductRoutingId, x.Sequence })
                .IsUnique();

            builder.Entity<ProductRoutingStep>()
                .HasIndex(x => new { x.ProductRoutingId, x.ProductionOperationId })
                .IsUnique();

            builder.Entity<ProductRoutingStep>()
                .HasOne(x => x.ProductRouting)
                .WithMany(x => x.Steps)
                .HasForeignKey(x => x.ProductRoutingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductRoutingStep>()
                .HasOne(x => x.ProductionOperation)
                .WithMany(x => x.ProductRoutingSteps)
                .HasForeignKey(x => x.ProductionOperationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasIndex(x => x.OrderNumber)
                .IsUnique();

            builder.Entity<ProductionOrder>()
                .HasIndex(x => x.ProductId);

            builder.Entity<ProductionOrder>()
                .HasIndex(x => x.Status);

            builder.Entity<ProductionOrder>()
                .HasIndex(x => x.ProductInventoryId);

            builder.Entity<ProductionOrder>()
                .HasIndex(x => x.PlannedStartDate);

            builder.Entity<ProductionOrder>()
                .HasIndex(x => x.PlannedEndDate);

            builder.Entity<ProductionOrder>()
                .Property(x => x.PlannedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrder>()
                .Property(x => x.RowVersion)
                .IsRowVersion();

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.ProductProductionProfile)
                .WithMany()
                .HasForeignKey(x => x.ProductProductionProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.BillOfMaterials)
                .WithMany()
                .HasForeignKey(x => x.BillOfMaterialsId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.ProductRouting)
                .WithMany()
                .HasForeignKey(x => x.ProductRoutingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.ProductCostCalculation)
                .WithMany()
                .HasForeignKey(x => x.ProductCostCalculationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.ProductInventory)
                .WithMany()
                .HasForeignKey(x => x.ProductInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.ProductionUnitOfMeasure)
                .WithMany()
                .HasForeignKey(x => x.ProductionUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.WipWarehouse)
                .WithMany()
                .HasForeignKey(x => x.WipWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrder>()
                .HasOne(x => x.FinishedGoodsWarehouse)
                .WithMany()
                .HasForeignKey(x => x.FinishedGoodsWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderOperation>()
                .HasIndex(x => new { x.ProductionOrderId, x.Sequence })
                .IsUnique();

            builder.Entity<ProductionOrderOperation>()
                .HasIndex(x => new { x.ProductionOrderId, x.ProductionOperationId })
                .IsUnique();

            builder.Entity<ProductionOrderOperation>()
                .Property(x => x.PlannedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderOperation>()
                .Property(x => x.AvailableQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderOperation>()
                .Property(x => x.CompletedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderOperation>()
                .Property(x => x.RejectedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderOperation>()
                .Property(x => x.RowVersion)
                .IsRowVersion();

            builder.Entity<ProductionOrderOperation>()
                .HasOne(x => x.ProductionOrder)
                .WithMany(x => x.Operations)
                .HasForeignKey(x => x.ProductionOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductionOrderOperation>()
                .HasOne(x => x.ProductionOperation)
                .WithMany()
                .HasForeignKey(x => x.ProductionOperationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderOperation>()
                .HasOne(x => x.ProductRoutingStep)
                .WithMany()
                .HasForeignKey(x => x.ProductRoutingStepId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterial>()
                .HasIndex(x => new { x.ProductionOrderId, x.MaterialId });

            builder.Entity<ProductionOrderMaterial>()
                .Property(x => x.QuantityPerUnitSnapshot)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterial>()
                .Property(x => x.WastePercentSnapshot)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterial>()
                .Property(x => x.RequiredQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterial>()
                .Property(x => x.ReservedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterial>()
                .Property(x => x.TransferredQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterial>()
                .Property(x => x.ConsumedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterial>()
                .Property(x => x.ReturnedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterial>()
                .HasOne(x => x.ProductionOrder)
                .WithMany(x => x.Materials)
                .HasForeignKey(x => x.ProductionOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterial>()
                .HasOne(x => x.BillOfMaterialLine)
                .WithMany()
                .HasForeignKey(x => x.BillOfMaterialLineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterial>()
                .HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterial>()
                .HasOne(x => x.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(x => x.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasIndex(x => x.ProductionOrderMaterialId);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasIndex(x => x.InventoryMovementId);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .Property(x => x.Quantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasOne(x => x.ProductionOrderMaterial)
                .WithMany(x => x.Allocations)
                .HasForeignKey(x => x.ProductionOrderMaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasOne(x => x.MaterialBatch)
                .WithMany()
                .HasForeignKey(x => x.MaterialBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasOne(x => x.SourceMaterialStock)
                .WithMany()
                .HasForeignKey(x => x.SourceMaterialStockId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasOne(x => x.SourceWarehouse)
                .WithMany()
                .HasForeignKey(x => x.SourceWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasOne(x => x.SourceWarehouseLocation)
                .WithMany()
                .HasForeignKey(x => x.SourceWarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasOne(x => x.DestinationWarehouse)
                .WithMany()
                .HasForeignKey(x => x.DestinationWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasOne(x => x.DestinationWarehouseLocation)
                .WithMany()
                .HasForeignKey(x => x.DestinationWarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialAllocation>()
                .HasOne(x => x.InventoryMovement)
                .WithMany()
                .HasForeignKey(x => x.InventoryMovementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasIndex(x => x.ProductionOrderMaterialId);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasIndex(x => x.ProductionOrderMaterialAllocationId);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasIndex(x => x.MaterialBatchId);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasIndex(x => x.InventoryMovementId);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasIndex(x => x.DocumentNumber);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasIndex(x => x.CreatedOn);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .Property(x => x.Quantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasOne(x => x.ProductionOrderMaterial)
                .WithMany()
                .HasForeignKey(x => x.ProductionOrderMaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasOne(x => x.ProductionOrderMaterialAllocation)
                .WithMany()
                .HasForeignKey(x => x.ProductionOrderMaterialAllocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasOne(x => x.MaterialBatch)
                .WithMany()
                .HasForeignKey(x => x.MaterialBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasOne(x => x.WarehouseLocation)
                .WithMany()
                .HasForeignKey(x => x.WarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionOrderMaterialConsumption>()
                .HasOne(x => x.InventoryMovement)
                .WithMany()
                .HasForeignKey(x => x.InventoryMovementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasIndex(x => x.ProductionOrderId)
                .IsUnique();

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasIndex(x => x.ProductInventoryId);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasIndex(x => x.InventoryMovementId);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasIndex(x => x.DocumentNumber);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasIndex(x => x.CreatedOn);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasOne(x => x.ProductionOrder)
                .WithMany(x => x.FinishedGoodsReceipts)
                .HasForeignKey(x => x.ProductionOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasOne(x => x.ProductInventory)
                .WithMany()
                .HasForeignKey(x => x.ProductInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasOne(x => x.WarehouseLocation)
                .WithMany()
                .HasForeignKey(x => x.WarehouseLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionFinishedGoodsReceipt>()
                .HasOne(x => x.InventoryMovement)
                .WithMany()
                .HasForeignKey(x => x.InventoryMovementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductionWorkEntry>()
                .HasIndex(x => x.ProductionOrderOperationId);

            builder.Entity<ProductionWorkEntry>()
                .HasIndex(x => x.UserId);

            builder.Entity<ProductionWorkEntry>()
                .HasIndex(x => x.CreatedOn);

            builder.Entity<ProductionWorkEntry>()
                .HasIndex(x => new { x.ProductionOrderOperationId, x.CreatedOn });

            builder.Entity<ProductionWorkEntry>()
                .Property(x => x.ReportedCompletedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionWorkEntry>()
                .Property(x => x.ReportedRejectedQuantity)
                .HasColumnType("decimal(18,4)");

            builder.Entity<ProductionWorkEntry>()
                .HasOne(x => x.ProductionOrderOperation)
                .WithMany(x => x.WorkEntries)
                .HasForeignKey(x => x.ProductionOrderOperationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
