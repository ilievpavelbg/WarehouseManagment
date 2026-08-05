using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class BillOfMaterialLine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BillOfMaterialsId { get; set; }
        public BillOfMaterials BillOfMaterials { get; set; } = null!;

        [Required]
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal QuantityPerUnit { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal? WastePercent { get; set; }

        [Required]
        public int UnitOfMeasureId { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; } = null!;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
