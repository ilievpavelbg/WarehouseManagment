using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class MaterialStockCardModel
    {
        public MaterialStockCardFilterModel Filter { get; set; } = new MaterialStockCardFilterModel();

        public int MaterialId { get; set; }

        public string MaterialCode { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string UnitOfMeasureName { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;

        public decimal TotalCurrentStock { get; set; }

        public decimal MinimumStock { get; set; }

        public MaterialStockStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;

        public decimal StandardCost { get; set; }

        public bool IsActive { get; set; }

        public List<MaterialStockCardPositionModel> Positions { get; set; } = new List<MaterialStockCardPositionModel>();

        public List<MaterialStockCardWarehouseModel> StockByWarehouse { get; set; } = new List<MaterialStockCardWarehouseModel>();

        public List<MaterialStockCardMovementModel> Movements { get; set; } = new List<MaterialStockCardMovementModel>();

        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        public int TotalMovementItems { get; set; }

        public int TotalMovementPages => Filter.PageSize <= 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(TotalMovementItems / (double)Filter.PageSize));
    }
}