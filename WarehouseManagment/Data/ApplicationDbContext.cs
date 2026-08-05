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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

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
        }
    }
}
