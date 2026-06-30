using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class MaterialStockAdjustmentModel
    {
        [Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        public string MaterialCode { get; set; } = null!;

        public string MaterialName { get; set; } = null!;

        public string UnitOfMeasureName { get; set; } = null!;

        public decimal CurrentTotalStock { get; set; }

        public decimal CurrentSelectedStock { get; set; }

        public decimal Difference { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Изберете склад.")]
        public int WarehouseId { get; set; }

        public int? WarehouseLocationId { get; set; }

        public int? MaterialBatchId { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Новата наличност не може да бъде отрицателна.")]
        public decimal NewQuantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        public List<WarehouseLocation> WarehouseLocations { get; set; } = new List<WarehouseLocation>();

        public List<MaterialBatch> MaterialBatches { get; set; } = new List<MaterialBatch>();
    }
}