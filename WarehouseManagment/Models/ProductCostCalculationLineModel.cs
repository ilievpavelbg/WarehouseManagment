using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductCostCalculationLineModel
    {
        public int Id { get; set; }

        public int ProductCostCalculationId { get; set; }

        [Required(ErrorMessage = "Изберете компонент.")]
        public int CostComponentId { get; set; }

        public string CostComponentCode { get; set; } = string.Empty;

        public string CostComponentName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Сумата не може да бъде отрицателна.")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
