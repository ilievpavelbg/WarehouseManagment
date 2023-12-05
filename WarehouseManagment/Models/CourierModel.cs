using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class CourierModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductSKU { get; set; } = null!;
        public int ProductInventoryId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Discount Discount { get; set; }
        public DateTime SendDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        [Required]
        public string ShippmentBill { get; set; } = null!;
        public CourierName CourierName { get; set; }
        public CourierPaymentMethod CourierPaymentMethod { get; set; }
        public bool IsPayed { get; set; }
        public bool IsDeleted { get; set; }
        public string? Description { get; set; } = null!;
        public string? Size { get; set; } = null!;
        public int Availability { get; set; }
        public string? Notes { get; set; } = null!;
        public int QuantityDifference { get; set; }
    }
}
