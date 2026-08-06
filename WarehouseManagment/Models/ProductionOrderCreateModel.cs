using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductionOrderCreateModel
    {
        [Required(ErrorMessage = "Изберете артикул.")]
        public int ProductId { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335", ErrorMessage = "Планираното количество трябва да бъде по-голямо от нула.")]
        public decimal PlannedQuantity { get; set; } = 1;

        [DataType(DataType.Date)]
        public DateTime? PlannedStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PlannedEndDate { get; set; }

        public ProductionOrderPriority Priority { get; set; } = ProductionOrderPriority.Normal;

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<ProductionSelectItemModel> Products { get; set; } = new List<ProductionSelectItemModel>();

        public ProductionOrderReadinessModel? Readiness { get; set; }

        public string ProductionUnit => Readiness?.ProductionUnit ?? string.Empty;
    }
}
