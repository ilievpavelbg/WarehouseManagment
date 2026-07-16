using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class MaterialStockCardFilterModel
    {
        public int MaterialId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int? WarehouseId { get; set; }

        public MovementType? MovementType { get; set; }

        public string? BatchOrLot { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}