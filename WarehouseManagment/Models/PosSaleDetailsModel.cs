namespace WarehouseManagment.Models
{
    public class PosSaleDetailsModel : PosReceiptModel
    {
        public string? ReversalReason { get; set; }
        public DateTime? ReversedOn { get; set; }
    }
}
