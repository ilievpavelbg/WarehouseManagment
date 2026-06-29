using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class MaterialBatch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Required]
        [StringLength(100)]
        public string BatchNumber { get; set; } = null!;

        [StringLength(100)]
        public string? LotNumber { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal? StandardCost { get; set; }

        public bool IsActive { get; set; } = true;

        public List<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }
}