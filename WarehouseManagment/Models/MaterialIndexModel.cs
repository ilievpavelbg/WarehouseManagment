using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class MaterialIndexModel
    {
        public int? CategoryId { get; set; }

        public int? SupplierId { get; set; }

        public bool LowStockOnly { get; set; }

        public bool ActiveOnly { get; set; } = true;

        public List<MaterialListItemModel> Materials { get; set; } = new List<MaterialListItemModel>();

        public List<MaterialCategory> Categories { get; set; } = new List<MaterialCategory>();

        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}