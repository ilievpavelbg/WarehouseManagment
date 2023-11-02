using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class ProductModel
    {
        public int Id { get; set; }
        [Required]
        public string SKU { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public double? RetailPrice { get; set; }
        public double? WholesalePrice { get; set; }
        public Data.Color Color { get; set; }
        public Genre? Genre { get; set; }
        public Composition? FirstComposition { get; set; }
        public Composition? SecondComposition { get; set; }
        public Category? Category { get; set; }
        public List<ProductInventoryModel> ProductInventoriesModel { get; set; } = new List<ProductInventoryModel>();
        public Discount Discount { get; set; }

    }
}
