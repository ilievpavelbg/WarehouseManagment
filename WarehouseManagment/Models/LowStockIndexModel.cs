using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class LowStockIndexModel
    {
        public LowStockFilterModel Filter { get; set; } = new LowStockFilterModel();

        public List<LowStockRowModel> Rows { get; set; } = new List<LowStockRowModel>();

        public List<MaterialCategory> Categories { get; set; } = new List<MaterialCategory>();

        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();

        public int TotalItems { get; set; }

        public int TotalPages => Filter.PageSize <= 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(TotalItems / (double)Filter.PageSize));
    }
}