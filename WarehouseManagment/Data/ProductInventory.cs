using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseManagment.Data
{
    public class ProductInventory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Size Size { get; set; }
        [Range(0, 10000)]
        public int Quantity { get; set; }
        public string ProductSKU { get; set; } = null!;

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public byte[]? Barcode { get; set; }

        [StringLength(32)]
        public string? BarcodeValue { get; set; }

        [StringLength(32)]
        public string? BarcodeType { get; set; }

        public DateTime? BarcodeGeneratedOn { get; set; }

        [StringLength(450)]
        public string? BarcodeGeneratedByUserId { get; set; }

        [StringLength(256)]
        public string? BarcodeGeneratedByUserNameSnapshot { get; set; }

        public DateTime? BarcodePrintedOn { get; set; }

        public int BarcodePrintCount { get; set; }
    }
}
