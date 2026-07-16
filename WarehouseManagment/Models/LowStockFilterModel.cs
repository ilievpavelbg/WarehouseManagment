namespace WarehouseManagment.Models
{
    public class LowStockFilterModel
    {
        public int? MaterialCategoryId { get; set; }

        public int? SupplierId { get; set; }

        public MaterialStockStatus? Status { get; set; }

        public bool ActiveMaterialsOnly { get; set; } = true;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}