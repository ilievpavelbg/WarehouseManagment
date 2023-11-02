namespace WarehouseManagment.Data
{
    public class Sale
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductSKU { get; set; } = null!;
        public int ProductInventoryId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Discount Discount { get; set; }
        public DateTime SoldDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsDeleted { get; set; }
    }
}
