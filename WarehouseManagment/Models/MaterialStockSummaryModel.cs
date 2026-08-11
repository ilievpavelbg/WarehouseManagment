namespace WarehouseManagment.Models
{
    public class MaterialStockSummaryModel
    {
        public int MaterialId { get; set; }

        public string MaterialCode { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public decimal MinimumStock { get; set; }

        public decimal TotalQuantity { get; set; }

        public int? ReplenishmentWarehouseId { get; set; }

        public string ReplenishmentWarehouseName { get; set; } = string.Empty;

        public bool IsReplenishmentWarehouseConfigured { get; set; }

        public MaterialStockStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;

        public int SortPriority { get; set; }
    }
}
