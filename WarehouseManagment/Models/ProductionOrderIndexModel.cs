namespace WarehouseManagment.Models
{
    public class ProductionOrderIndexModel
    {
        public ProductionOrderFilterModel Filter { get; set; } = new ProductionOrderFilterModel();

        public List<ProductionOrderRowModel> Rows { get; set; } = new List<ProductionOrderRowModel>();

        public List<ProductionSelectItemModel> Products { get; set; } = new List<ProductionSelectItemModel>();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalRows { get; set; }

        public int TotalPages => TotalRows == 0 ? 1 : (int)Math.Ceiling((decimal)TotalRows / PageSize);
    }
}
