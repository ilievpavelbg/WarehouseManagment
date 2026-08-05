using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductProductionProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string ProductionName { get; set; } = null!;

        [Required]
        public int ProductionUnitOfMeasureId { get; set; }
        public UnitOfMeasure ProductionUnitOfMeasure { get; set; } = null!;

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal StandardProductionQuantity { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }
    }
}
