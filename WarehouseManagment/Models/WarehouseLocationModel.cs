using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class WarehouseLocationModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue)]
        public int WarehouseId { get; set; }

        public int? WarehouseZoneId { get; set; }
    }
}
