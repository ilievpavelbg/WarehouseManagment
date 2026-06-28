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
        public DbSet<InventoryMovement> InventoryMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            builder.Entity<Warehouse>()
                .HasIndex(w => w.Code)
                .IsUnique();

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

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.MovementDate);

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.ProductId);

            builder.Entity<InventoryMovement>()
                .HasIndex(m => m.ProductInventoryId);

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