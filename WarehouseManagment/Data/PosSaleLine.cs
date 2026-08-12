namespace WarehouseManagment.Data
{
    public class PosSaleLine
    {
        public int Id { get; set; }
        public int PosSaleId { get; set; }
        public PosSale PosSale { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int ProductInventoryId { get; set; }
        public ProductInventory ProductInventory { get; set; } = null!;
        public string ProductSKU { get; set; } = null!;
        public string? ProductDescriptionSnapshot { get; set; }
        public string SizeSnapshot { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineTotal { get; set; }
    }
}
