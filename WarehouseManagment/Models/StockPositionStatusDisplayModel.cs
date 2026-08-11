namespace WarehouseManagment.Models
{
    public class StockPositionStatusDisplayModel
    {
        public MaterialStockStatus Status { get; set; }

        public string StatusCode { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;

        public int SortPriority { get; set; }

        public bool IsDefaultMaterialWarehouse { get; set; }

        public bool IsWipWarehouse { get; set; }
    }
}
