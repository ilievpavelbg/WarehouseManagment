using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class MaterialStockChangeModel
    {
        [Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }

        public int? WarehouseLocationId { get; set; }

        public int? DestinationWarehouseId { get; set; }

        public int? DestinationWarehouseLocationId { get; set; }

        public int? MaterialBatchId { get; set; }

        public decimal Quantity { get; set; }

        [Required]
        public MovementType MovementType { get; set; }

        [Required]
        [StringLength(100)]
        public string ReferenceType { get; set; } = null!;

        [Range(typeof(long), "1", "9223372036854775807")]
        public long ReferenceId { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        [StringLength(100)]
        public string? LotNumber { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}