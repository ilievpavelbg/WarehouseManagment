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
        public double? Price { get; set; }
        public int? Quantity { get; set; }
        public Genre? Genre { get; set; }
        public Composition? FirstComposition { get; set; }
        public Composition? SecondComposition { get; set; }
        public Category? Category { get; set; }
        public Size? Size { get; set; }
        public JeansSize? JeansSize { get; set; }
        public byte[]? Barcode { get; set; } = null!;
    }
}
