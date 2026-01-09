using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseManagment.Data
{
    public class InventoryBalance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Item))]
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        [ForeignKey(nameof(Location))]
        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }
    }
}
