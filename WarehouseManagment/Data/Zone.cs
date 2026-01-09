using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseManagment.Data
{
    public class Zone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = null!;

        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public List<Location> Locations { get; set; } = new List<Location>();
    }
}
