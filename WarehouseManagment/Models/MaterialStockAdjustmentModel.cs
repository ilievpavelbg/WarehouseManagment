using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class MaterialStockAdjustmentModel
    {
        [Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [ValidateNever]
        public string MaterialCode { get; set; } = string.Empty;

        [ValidateNever]
        public string MaterialName { get; set; } = string.Empty;

        [ValidateNever]
        public string UnitOfMeasureName { get; set; } = string.Empty;

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

        [ValidateNever]
        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        [ValidateNever]
        public List<WarehouseLocation> WarehouseLocations { get; set; } = new List<WarehouseLocation>();

        [ValidateNever]
        public List<MaterialBatch> MaterialBatches { get; set; } = new List<MaterialBatch>();
    }
}