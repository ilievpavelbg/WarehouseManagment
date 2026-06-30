namespace WarehouseManagment.Models
{
    public class MaterialListItemModel
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? CategoryName { get; set; }

        public string? SupplierName { get; set; }

        public string UnitOfMeasureName { get; set; } = null!;

        public decimal CurrentStock { get; set; }

        public decimal StandardCost { get; set; }

        public decimal MinimumStock { get; set; }

        public bool IsBatchTracked { get; set; }

        public bool IsActive { get; set; }
    }
}