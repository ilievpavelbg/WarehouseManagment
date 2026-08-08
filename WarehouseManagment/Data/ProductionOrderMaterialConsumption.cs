using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductionOrderMaterialConsumption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderMaterialId { get; set; }
        public ProductionOrderMaterial ProductionOrderMaterial { get; set; } = null!;

        public int? ProductionOrderMaterialAllocationId { get; set; }
        public ProductionOrderMaterialAllocation? ProductionOrderMaterialAllocation { get; set; }

        public int? MaterialBatchId { get; set; }
        public MaterialBatch? MaterialBatch { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public int? WarehouseLocationId { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }

        public decimal Quantity { get; set; }

        public long? InventoryMovementId { get; set; }
        public InventoryMovement? InventoryMovement { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentNumber { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        [StringLength(100)]
        public string? BatchNumberSnapshot { get; set; }

        [StringLength(100)]
        public string? LotNumberSnapshot { get; set; }
    }
}
