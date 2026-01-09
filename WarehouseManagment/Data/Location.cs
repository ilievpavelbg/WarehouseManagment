using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseManagment.Data
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(128)]
        public string Name { get; set; } = null!;

        [ForeignKey(nameof(Zone))]
        public int ZoneId { get; set; }
        public Zone Zone { get; set; } = null!;

        public List<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();
    }
}
