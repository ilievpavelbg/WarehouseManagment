using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class MaterialStock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        [Required]
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public int? WarehouseLocationId { get; set; }
        public WarehouseLocation? WarehouseLocation { get; set; }

        public int? MaterialBatchId { get; set; }
        public MaterialBatch? MaterialBatch { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Quantity { get; set; }

        public DateTime LastUpdatedOn { get; set; } = DateTime.Now;
    }
}