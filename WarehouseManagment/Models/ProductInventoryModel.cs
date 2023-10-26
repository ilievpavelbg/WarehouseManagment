using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class ProductInventoryModel
    {
        public int Id { get; set; }
        public Data.Size Size { get; set; }
        public int Quantity { get; set; }
        public string ProductSKU { get; set; } = null!;
        public int ProductId { get; set; }
        public byte[]? Barcode { get; set; } = null!;
    }
}
