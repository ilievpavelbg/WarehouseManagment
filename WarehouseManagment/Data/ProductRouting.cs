using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductRouting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Version { get; set; }

        public bool IsActive { get; set; }

        public bool HasBeenActivated { get; set; }

        public DateTime? ActivatedOn { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }

        public List<ProductRoutingStep> Steps { get; set; } = new List<ProductRoutingStep>();
    }
}
