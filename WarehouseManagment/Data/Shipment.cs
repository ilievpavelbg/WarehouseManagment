using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseManagment.Data
{
    public class Shipment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string DocumentNumber { get; set; } = null!;

        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public DocumentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PostedAt { get; set; }

        public List<ShipmentLine> Lines { get; set; } = new List<ShipmentLine>();
    }
}
