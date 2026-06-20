using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WarehouseManagment.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            if (this.Database.IsRelational())
            {
                this.Database.Migrate();
            }
            else
            {
                this.Database.EnsureCreated();
            }
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductInventory> ProductInventory { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Courier> Couriers { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<InventoryBalance> InventoryBalances { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<PurchaseReceipt> PurchaseReceipts { get; set; }
        public DbSet<PurchaseReceiptLine> PurchaseReceiptLines { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentLine> ShipmentLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InventoryBalance>()
                .HasIndex(balance => new { balance.ItemId, balance.LocationId })
                .IsUnique();

            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Code = "WH-01", Name = "Main Warehouse" }
            );

            modelBuilder.Entity<Zone>().HasData(
                new Zone { Id = 1, WarehouseId = 1, Code = "REC", Name = "Receiving" },
                new Zone { Id = 2, WarehouseId = 1, Code = "STO", Name = "Storage" },
                new Zone { Id = 3, WarehouseId = 1, Code = "SHP", Name = "Shipping" }
            );

            modelBuilder.Entity<Item>().HasData(
                new Item
                {
                    Id = 1,
                    SKU = "ITEM-001",
                    Name = "Seed Item",
                    ItemType = ItemType.FinishedGood,
                    UnitOfMeasure = "pcs"
                }
            );

            modelBuilder.Entity<Location>().HasData(
                new Location { Id = 1, ZoneId = 1, Code = "REC-01", Name = "Receiving Dock 1" },
                new Location { Id = 2, ZoneId = 2, Code = "STO-A1", Name = "Storage A1" },
                new Location { Id = 3, ZoneId = 2, Code = "STO-A2", Name = "Storage A2" },
                new Location { Id = 4, ZoneId = 2, Code = "STO-B1", Name = "Storage B1" },
                new Location { Id = 5, ZoneId = 3, Code = "SHP-01", Name = "Shipping Dock 1" }
            );

            modelBuilder.Entity<PurchaseReceiptLine>()
        .HasOne(x => x.PurchaseReceipt)
        .WithMany(x => x.Lines)
        .HasForeignKey(x => x.PurchaseReceiptId)
        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PurchaseReceiptLine>()
                .HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PurchaseReceiptLine>()
                .HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ShipmentLine>()
                .HasOne(x => x.Shipment)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.ShipmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ShipmentLine>()
                .HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ShipmentLine>()
                .HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PurchaseReceipt>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Shipment>()
                .HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.NoAction);


        }
    }
}
