using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductRoutingStep
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductRoutingId { get; set; }
        public ProductRouting ProductRouting { get; set; } = null!;

        [Required]
        public int ProductionOperationId { get; set; }
        public ProductionOperation ProductionOperation { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Sequence { get; set; }

        [Range(0, int.MaxValue)]
        public int? StandardTimeMinutes { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
