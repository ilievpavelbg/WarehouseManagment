using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class BillOfMaterialLineModel
    {
        public int? Id { get; set; }

        public int? BillOfMaterialsId { get; set; }

        public int? MaterialId { get; set; }

        public string MaterialCode { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string UnitOfMeasureName { get; set; } = string.Empty;

        public int? UnitOfMeasureId { get; set; }

        public decimal? QuantityPerUnit { get; set; }

        public decimal? WastePercent { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
