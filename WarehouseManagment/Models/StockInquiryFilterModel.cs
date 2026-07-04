namespace WarehouseManagment.Models
{
    public class StockInquiryFilterModel
    {
        public int? MaterialId { get; set; }

        public int? MaterialCategoryId { get; set; }

        public int? SupplierId { get; set; }

        public int? WarehouseId { get; set; }

        public int? WarehouseLocationId { get; set; }

        public string? BatchOrLot { get; set; }

        public bool LowStockOnly { get; set; }

        public bool ZeroStockOnly { get; set; }

        public bool ActiveMaterialsOnly { get; set; } = true;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}