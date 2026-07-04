using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class InventoryMovementFilterModel
    {
        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int? MaterialId { get; set; }

        public int? WarehouseId { get; set; }

        public int? WarehouseLocationId { get; set; }

        public int? DestinationWarehouseId { get; set; }

        public int? DestinationWarehouseLocationId { get; set; }

        public string? BatchOrLot { get; set; }

        public MovementType? MovementType { get; set; }

        public string? ReferenceType { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}