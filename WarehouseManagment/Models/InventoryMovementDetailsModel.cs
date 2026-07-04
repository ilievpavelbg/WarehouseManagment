namespace WarehouseManagment.Models
{
    public class InventoryMovementDetailsModel : InventoryMovementRowModel
    {
        public DateTime MovementDate { get; set; }

        public string StockItemTypeName { get; set; } = string.Empty;

        public string ProductInventoryInfo { get; set; } = string.Empty;

        public string ReferenceId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
    }
}