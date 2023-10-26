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
        public byte[]? Barcode { get; set; } = null!;

    }
}
