namespace WarehouseManagment.Models
{
    public class LowStockRowModel
    {
        public int MaterialId { get; set; }

        public string MaterialCode { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string UnitOfMeasureName { get; set; } = string.Empty;

        public decimal TotalCurrentStock { get; set; }

        public decimal MinimumStock { get; set; }

        public decimal Shortage { get; set; }

        public decimal SuggestedReplenishmentQuantity { get; set; }

        public MaterialStockStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;

        public string PreferredSupplierName { get; set; } = string.Empty;

        public int SortPriority { get; set; }
    }
}