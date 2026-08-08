using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Models;

namespace WarehouseManagment.Data
{
    public class ProductionOrderMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; } = null!;

        public int? BillOfMaterialLineId { get; set; }
        public BillOfMaterialLine? BillOfMaterialLine { get; set; }

        [Required]
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        [Required]
        public int UnitOfMeasureId { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; } = null!;

        [Required]
        [StringLength(128)]
        public string MaterialCodeSnapshot { get; set; } = null!;

        [Required]
        [StringLength(250)]
        public string MaterialNameSnapshot { get; set; } = null!;

        [Required]
        [StringLength(64)]
        public string UnitNameSnapshot { get; set; } = null!;

        public decimal QuantityPerUnitSnapshot { get; set; }

        public decimal? WastePercentSnapshot { get; set; }

        public decimal RequiredQuantity { get; set; }

        public decimal ReservedQuantity { get; set; }

        public decimal TransferredQuantity { get; set; }

        public decimal ConsumedQuantity { get; set; }

        public decimal ReturnedQuantity { get; set; }

        public ProductionOrderMaterialStatus Status { get; set; } = ProductionOrderMaterialStatus.Planned;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? TransferredOn { get; set; }

        public List<ProductionOrderMaterialAllocation> Allocations { get; set; } = new List<ProductionOrderMaterialAllocation>();
    }
}
