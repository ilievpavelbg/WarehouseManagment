using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductionOrderMaterialAllocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderMaterialId { get; set; }
        public ProductionOrderMaterial ProductionOrderMaterial { get; set; } = null!;

        public int? MaterialBatchId { get; set; }
        public MaterialBatch? MaterialBatch { get; set; }

        public int? SourceMaterialStockId { get; set; }
        public MaterialStock? SourceMaterialStock { get; set; }

        [Required]
        public int SourceWarehouseId { get; set; }
        public Warehouse SourceWarehouse { get; set; } = null!;

        public int? SourceWarehouseLocationId { get; set; }
        public WarehouseLocation? SourceWarehouseLocation { get; set; }

        [Required]
        public int DestinationWarehouseId { get; set; }
        public Warehouse DestinationWarehouse { get; set; } = null!;

        public int? DestinationWarehouseLocationId { get; set; }
        public WarehouseLocation? DestinationWarehouseLocation { get; set; }

        [StringLength(100)]
        public string? BatchNumberSnapshot { get; set; }

        [StringLength(100)]
        public string? LotNumberSnapshot { get; set; }

        public decimal Quantity { get; set; }

        public long? InventoryMovementId { get; set; }
        public InventoryMovement? InventoryMovement { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
