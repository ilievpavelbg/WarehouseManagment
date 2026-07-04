namespace WarehouseManagment.Models
{
    public class StockInquiryRowModel
    {
        public string MaterialCode { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string UnitOfMeasureName { get; set; } = string.Empty;

        public string WarehouseName { get; set; } = string.Empty;

        public string WarehouseLocationName { get; set; } = string.Empty;

        public string BatchNumber { get; set; } = string.Empty;

        public string LotNumber { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public decimal MinimumStock { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;

        public int SortPriority { get; set; }

        public DateTime LastUpdatedOn { get; set; }
    }
}