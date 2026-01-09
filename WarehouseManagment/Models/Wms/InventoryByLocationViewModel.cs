namespace WarehouseManagment.Models.Wms
{
    public class InventoryByLocationViewModel
    {
        public List<InventoryByLocationRow> Rows { get; set; } = new();
    }

    public class InventoryByLocationRow
    {
        public string Warehouse { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ItemSku { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
