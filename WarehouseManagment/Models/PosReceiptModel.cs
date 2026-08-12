using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class PosReceiptModel
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public string? OperatorName { get; set; }
        public string WarehouseName { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal Total { get; set; }
        public PosSaleStatus Status { get; set; }
        public List<PosReceiptLineModel> Lines { get; set; } = new();
    }
}
