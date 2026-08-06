namespace WarehouseManagment.Models
{
    public class ProductionWorkDetailsModel
    {
        public ProductionWorkTaskRowModel Task { get; set; } = new ProductionWorkTaskRowModel();

        public List<ProductionWorkEntryRowModel> WorkHistory { get; set; } = new List<ProductionWorkEntryRowModel>();
    }
}
