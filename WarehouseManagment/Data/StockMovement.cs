using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseManagment.Data
{
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Item))]
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        [ForeignKey(nameof(Location))]
        public int LocationId { get; set; }
        public Location Location { get; set; } = null!;

        public StockMovementType MovementType { get; set; }

        public decimal Quantity { get; set; }

        [MaxLength(64)]
        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        public DateTime OccurredAt { get; set; }

        [MaxLength(256)]
        public string? Notes { get; set; }
    }
}
