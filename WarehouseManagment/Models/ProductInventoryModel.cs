namespace WarehouseManagment.Models
{
    public class ProductInventoryModel
    {
        private int quantity;

        public int Id { get; set; }
        public Data.Size Size { get; set; }
        public int Quantity { get => quantity; set => quantity = value; }
        public string ProductSKU { get; set; } = null!;
        public int ProductId { get; set; }
        public byte[]? Barcode { get; set; } = null!;
        public List<string> ExistingSizes { get; set; } = new List<string>();
    }
}
