using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class WarehouseModel
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

        public List<WarehouseZone> Zones { get; set; } = new List<WarehouseZone>();
        public List<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
    }
}
