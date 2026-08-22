using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class FinishedGoodsStockIndexModel
    {
        public FinishedGoodsStockFilterModel Filter { get; set; } = new FinishedGoodsStockFilterModel();

        public List<FinishedGoodsStockRowModel> Rows { get; set; } = new List<FinishedGoodsStockRowModel>();

        public List<Product> Products { get; set; } = new List<Product>();

        public string FinishedGoodsWarehouseName { get; set; } = string.Empty;

        public int TotalItems { get; set; }

        public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)Filter.PageSize);
    }
}
