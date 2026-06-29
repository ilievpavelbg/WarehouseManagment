using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class MaterialModel
    {
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
        [Range(1, int.MaxValue)]
        public int MaterialCategoryId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int UnitOfMeasureId { get; set; }

        public int? SupplierId { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal StandardCost { get; set; }

        [StringLength(100)]
        public string? Barcode { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal MinimumStock { get; set; }

        public bool IsBatchTracked { get; set; }

        public bool IsLotTracked { get; set; }

        public bool IsActive { get; set; } = true;
    }
}