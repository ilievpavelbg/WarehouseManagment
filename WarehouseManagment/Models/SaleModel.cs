using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class SaleModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductSKU { get; set; } = null!;
        public int ProductInventoryId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Discount Discount { get; set; }
        public DateTime SoldDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsDeleted { get; set; }
        public string? Description { get; set; } = null!;
        public string Size { get; set; } = null!;
        public int Availability { get; set; }
        public string? Notes { get; set; } = null!;
        public int QuantityDifference { get; set; }
    }
}
