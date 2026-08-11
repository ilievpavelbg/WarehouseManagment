namespace WarehouseManagment.Models
{
    public class MaterialStockCardWarehouseModel
    {
        public string WarehouseName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;
    }
}
