using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Models;

namespace WarehouseManagment.Data
{
    public class ProductionOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string OrderNumber { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        public int ProductProductionProfileId { get; set; }
        public ProductProductionProfile ProductProductionProfile { get; set; } = null!;

        [Required]
        public int BillOfMaterialsId { get; set; }
        public BillOfMaterials BillOfMaterials { get; set; } = null!;

        [Required]
        public int ProductRoutingId { get; set; }
        public ProductRouting ProductRouting { get; set; } = null!;

        public int? ProductCostCalculationId { get; set; }
        public ProductCostCalculation? ProductCostCalculation { get; set; }

        public decimal PlannedQuantity { get; set; }

        [Required]
        public int ProductionUnitOfMeasureId { get; set; }
        public UnitOfMeasure ProductionUnitOfMeasure { get; set; } = null!;

        [Required]
        public int WipWarehouseId { get; set; }
        public Warehouse WipWarehouse { get; set; } = null!;

        [Required]
        public int FinishedGoodsWarehouseId { get; set; }
        public Warehouse FinishedGoodsWarehouse { get; set; } = null!;

        [Required]
        [StringLength(128)]
        public string ProductSkuSnapshot { get; set; } = null!;

        [StringLength(500)]
        public string? ProductDescriptionSnapshot { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductionNameSnapshot { get; set; } = null!;

        [Required]
        [StringLength(64)]
        public string ProductionUnitNameSnapshot { get; set; } = null!;

        public int BillOfMaterialsVersionSnapshot { get; set; }

        public int ProductRoutingVersionSnapshot { get; set; }

        public int? ProductCostCalculationVersionSnapshot { get; set; }

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public ProductionOrderStatus Status { get; set; } = ProductionOrderStatus.Planned;

        public ProductionOrderPriority Priority { get; set; } = ProductionOrderPriority.Normal;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        [StringLength(450)]
        public string? StartedByUserId { get; set; }

        [StringLength(450)]
        public string? CompletedByUserId { get; set; }

        public DateTime? CancelledOn { get; set; }

        [StringLength(450)]
        public string? CancelledByUserId { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime? MaterialsTransferredOn { get; set; }

        [StringLength(450)]
        public string? MaterialsTransferredByUserId { get; set; }

        [StringLength(100)]
        public string? MaterialsTransferDocumentNumber { get; set; }

        public List<ProductionOrderOperation> Operations { get; set; } = new List<ProductionOrderOperation>();

        public List<ProductionOrderMaterial> Materials { get; set; } = new List<ProductionOrderMaterial>();

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
