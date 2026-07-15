namespace WarehouseManagment.Models
{
    public class MaterialWarehouseStockModel
    {
        public int MaterialId { get; set; }

        public int WarehouseId { get; set; }

        public string WarehouseCode { get; set; } = string.Empty;

        public string WarehouseName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
    }
}