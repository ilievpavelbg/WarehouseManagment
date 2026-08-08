using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductionFinishedGoodsReceipt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        public int ProductInventoryId { get; set; }
        public ProductInventory ProductInventory { get; set; } = null!;

        [Required]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public int? WarehouseLocationId { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }

        public int Quantity { get; set; }

        public long? InventoryMovementId { get; set; }
        public InventoryMovement? InventoryMovement { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentNumber { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        [Required]
        [StringLength(128)]
        public string ProductSkuSnapshot { get; set; } = null!;

        [StringLength(500)]
        public string? ProductDescriptionSnapshot { get; set; }

        [Required]
        [StringLength(64)]
        public string SizeSnapshot { get; set; } = null!;
    }
}
