namespace WarehouseManagment.Models
{
    public class ProductionWorkTaskIndexModel
    {
        public ProductionWorkTaskFilterModel Filter { get; set; } = new ProductionWorkTaskFilterModel();

        public List<ProductionWorkTaskRowModel> Rows { get; set; } = new List<ProductionWorkTaskRowModel>();

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public int TotalRows { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((decimal)TotalRows / PageSize);
    }
}
