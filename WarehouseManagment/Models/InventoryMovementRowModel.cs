namespace WarehouseManagment.Models
{
    public class InventoryMovementRowModel
    {
        public long Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public string ItemKind { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string MovementTypeName { get; set; } = string.Empty;

        public string MovementTypeCssClass { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public string WarehouseLocationName { get; set; } = string.Empty;

        public string DestinationWarehouseName { get; set; } = string.Empty;

        public string DestinationWarehouseLocationName { get; set; } = string.Empty;

        public string BatchNumber { get; set; } = string.Empty;

        public string LotNumber { get; set; } = string.Empty;

        public string ReferenceType { get; set; } = string.Empty;

        public string ReferenceNumber { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}