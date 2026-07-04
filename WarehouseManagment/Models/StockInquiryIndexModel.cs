using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class StockInquiryIndexModel
    {
        public StockInquiryFilterModel Filter { get; set; } = new StockInquiryFilterModel();

        public List<StockInquiryRowModel> Stocks { get; set; } = new List<StockInquiryRowModel>();

        public List<Material> Materials { get; set; } = new List<Material>();

        public List<MaterialCategory> Categories { get; set; } = new List<MaterialCategory>();

        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();

        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        public List<WarehouseLocation> WarehouseLocations { get; set; } = new List<WarehouseLocation>();

        public int TotalItems { get; set; }

        public int TotalPages => Filter.PageSize <= 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(TotalItems / (double)Filter.PageSize));
    }
}