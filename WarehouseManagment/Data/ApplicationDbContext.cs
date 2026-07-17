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
        }
    }
}