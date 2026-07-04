using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class InventoryMovementIndexModel
    {
        public InventoryMovementFilterModel Filter { get; set; } = new InventoryMovementFilterModel();

        public List<InventoryMovementRowModel> Movements { get; set; } = new List<InventoryMovementRowModel>();

        public List<Material> Materials { get; set; } = new List<Material>();

        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        public List<WarehouseLocation> WarehouseLocations { get; set; } = new List<WarehouseLocation>();

        public List<MovementType> MovementTypes { get; set; } = new List<MovementType>();

        public List<string> ReferenceTypes { get; set; } = new List<string>();

        public int TotalItems { get; set; }

        public int TotalPages => Filter.PageSize <= 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(TotalItems / (double)Filter.PageSize));
    }
}