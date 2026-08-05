using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductCostCalculationLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductCostCalculationId { get; set; }
        public ProductCostCalculation ProductCostCalculation { get; set; } = null!;

        [Required]
        public int CostComponentId { get; set; }
        public CostComponent CostComponent { get; set; } = null!;

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
