namespace WarehouseManagment.Models
{
    public class ProductInventoryVariantSelectItemModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
