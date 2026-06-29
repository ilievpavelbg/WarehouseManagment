using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class InventoryMovement
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public MovementType MovementType { get; set; }

        [Required]
        public StockItemType StockItemType { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public int? ProductInventoryId { get; set; }
        public ProductInventory? ProductInventory { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        public DateTime MovementDate { get; set; } = DateTime.Now;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public int? WarehouseZoneId { get; set; }
        public WarehouseZone? WarehouseZone { get; set; }

        public int? WarehouseLocationId { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }

        public int? DestinationWarehouseId { get; set; }
        public Warehouse? DestinationWarehouse { get; set; }

        public int? DestinationWarehouseZoneId { get; set; }
        public WarehouseZone? DestinationWarehouseZone { get; set; }

        public int? DestinationWarehouseLocationId { get; set; }
        public WarehouseLocation? DestinationWarehouseLocation { get; set; }

        [Required]
        [StringLength(100)]
        public string ReferenceType { get; set; } = null!;

        [Range(typeof(long), "1", "9223372036854775807")]
        public long ReferenceId { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        [StringLength(100)]
        public string? LotNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }
    }
}
