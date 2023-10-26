using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string SKU { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public double? RetailPrice { get; set; }
        public double? WholesalePrice { get; set; }
        public Color Color { get; set; }
        public Genre? Genre { get; set; }
        public Composition? FirstComposition { get; set; }
        public Composition? SecondComposition { get; set; }
        public Category? Category { get; set; }
        public List<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();

    }
}
