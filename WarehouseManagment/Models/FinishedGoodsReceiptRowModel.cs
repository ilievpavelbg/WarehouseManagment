namespace WarehouseManagment.Models
{
    public class FinishedGoodsReceiptRowModel
    {
        public string DocumentNumber { get; set; } = string.Empty;

        public int ProductInventoryId { get; set; }

        public string Size { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public DateTime CreatedOn { get; set; }

        public string ProductionOrderNumber { get; set; } = string.Empty;

        public string WarehouseName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
    }
}
