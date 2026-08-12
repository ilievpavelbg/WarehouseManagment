using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class PosSaleRowModel
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public string? OperatorName { get; set; }
        public int LineCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal Total { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PosSaleStatus Status { get; set; }
    }
}
