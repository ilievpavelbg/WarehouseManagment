using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int MaterialCategoryId { get; set; }
        public MaterialCategory MaterialCategory { get; set; } = null!;

        [Required]
        public int UnitOfMeasureId { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; } = null!;

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal StandardCost { get; set; }

        [StringLength(100)]
        public string? Barcode { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal MinimumStock { get; set; }

        public bool IsBatchTracked { get; set; }

        public bool IsLotTracked { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }

        public List<MaterialBatch> MaterialBatches { get; set; } = new List<MaterialBatch>();
        public List<MaterialStock> MaterialStocks { get; set; } = new List<MaterialStock>();
        public List<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }
}