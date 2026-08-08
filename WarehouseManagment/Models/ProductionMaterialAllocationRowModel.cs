namespace WarehouseManagment.Models
{
    public class ProductionMaterialAllocationRowModel
    {
        public string SourceWarehouse { get; set; } = string.Empty;

        public string SourceLocation { get; set; } = string.Empty;

        public string DestinationWarehouse { get; set; } = string.Empty;

        public string DestinationLocation { get; set; } = string.Empty;

        public string? BatchNumber { get; set; }

        public string? LotNumber { get; set; }

        public decimal Quantity { get; set; }

        public long? InventoryMovementId { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
