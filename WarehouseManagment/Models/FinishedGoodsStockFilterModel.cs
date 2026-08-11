namespace WarehouseManagment.Models
{
    public class FinishedGoodsStockFilterModel
    {
        public int? ProductId { get; set; }

        public int? ProductInventoryId { get; set; }

        public string? Search { get; set; }

        public bool ShowZeroStock { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}
