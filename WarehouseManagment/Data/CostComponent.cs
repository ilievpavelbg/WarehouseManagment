using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class CostComponent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDirectCost { get; set; }

        public bool IsSystemCalculated { get; set; }

        public List<ProductCostCalculationLine> ProductCostCalculationLines { get; set; } = new List<ProductCostCalculationLine>();
    }
}
