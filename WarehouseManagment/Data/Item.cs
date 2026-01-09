using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string SKU { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = null!;

        public ItemType ItemType { get; set; }

        [MaxLength(16)]
        public string? UnitOfMeasure { get; set; }

        public List<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();
    }
}
