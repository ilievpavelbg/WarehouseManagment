using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductProductionProfileModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Изберете артикул.")]
        public int ProductId { get; set; }

        public string ProductDisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Въведете производствено име.")]
        [StringLength(150)]
        public string ProductionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Изберете производствена мерна единица.")]
        public int ProductionUnitOfMeasureId { get; set; }

        public string ProductionUnitOfMeasureName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335", ErrorMessage = "Стандартното количество трябва да бъде по-голямо от нула.")]
        public decimal StandardProductionQuantity { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<ProductionSelectItemModel> Products { get; set; } = new List<ProductionSelectItemModel>();

        public List<ProductionSelectItemModel> UnitsOfMeasure { get; set; } = new List<ProductionSelectItemModel>();
    }
}
