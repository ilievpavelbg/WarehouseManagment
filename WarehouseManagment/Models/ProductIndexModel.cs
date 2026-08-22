namespace WarehouseManagment.Models
{
    public class ProductIndexModel
    {
        public ProductIndexFilterModel Filter { get; set; } = new ProductIndexFilterModel();

        public List<ProductIndexRowModel> Rows { get; set; } = new List<ProductIndexRowModel>();

        public int TotalItems { get; set; }

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)Math.Max(1, Filter.PageSize)));
    }

    public class ProductIndexFilterModel
    {
        public string? SKU { get; set; }

        public string? Search { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 50;
    }

    public class ProductIndexRowModel
    {
        public int Id { get; set; }

        public string SKU { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double? RetailPrice { get; set; }

        public double? WholesalePrice { get; set; }

        public string Category { get; set; } = string.Empty;

        public int VariantCount { get; set; }

        public int TotalQuantity { get; set; }
    }
}
