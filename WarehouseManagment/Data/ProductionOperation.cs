using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductionOperation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        public int DefaultSequence { get; set; }

        [Required]
        [StringLength(64)]
        public string RequiredRole { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public List<ProductRoutingStep> ProductRoutingSteps { get; set; } = new List<ProductRoutingStep>();
    }
}
