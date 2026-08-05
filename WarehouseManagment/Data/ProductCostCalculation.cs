using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductCostCalculation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Version { get; set; }

        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        public bool IsActive { get; set; }

        public bool HasBeenActivated { get; set; }

        public DateTime? ActivatedOn { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        public decimal TotalCost { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "EUR";

        public List<ProductCostCalculationLine> Lines { get; set; } = new List<ProductCostCalculationLine>();
    }
}
