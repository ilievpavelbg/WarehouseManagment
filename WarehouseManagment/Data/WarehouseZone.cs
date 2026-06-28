using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseManagment.Data
{
    public class WarehouseZone
    {
        [Key]
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

        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public List<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
        public List<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }
}
