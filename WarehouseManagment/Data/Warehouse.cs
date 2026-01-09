using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = null!;

        public List<Zone> Zones { get; set; } = new List<Zone>();
    }
}
