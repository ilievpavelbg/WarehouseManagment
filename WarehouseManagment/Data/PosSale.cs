namespace WarehouseManagment.Data
{
    public class PosSale
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserNameSnapshot { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal Total { get; set; }
        public PosSaleStatus Status { get; set; } = PosSaleStatus.Completed;
        public string? ReversalReason { get; set; }
        public DateTime? ReversedOn { get; set; }
        public string? ReversedByUserId { get; set; }
        public List<PosSaleLine> Lines { get; set; } = new();
    }
}
