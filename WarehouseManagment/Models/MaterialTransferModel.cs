using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class MaterialTransferModel
    {
        [Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [ValidateNever]
        public string MaterialCode { get; set; } = string.Empty;

        [ValidateNever]
        public string MaterialName { get; set; } = string.Empty;

        [ValidateNever]
        public string UnitOfMeasureName { get; set; } = string.Empty;

        [ValidateNever]
        public decimal CurrentTotalStock { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Изберете наличност източник.")]
        public int SourceMaterialStockId { get; set; }

        public int SourceWarehouseId { get; set; }

        public int? SourceWarehouseLocationId { get; set; }

        public int? MaterialBatchId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Изберете склад получател.")]
        public int DestinationWarehouseId { get; set; }

        public int? DestinationWarehouseLocationId { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335", ErrorMessage = "Въведете количество по-голямо от нула.")]
        public decimal Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [ValidateNever]
        public List<MaterialStock> SourceStockPositions { get; set; } = new List<MaterialStock>();

        [ValidateNever]
        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        [ValidateNever]
        public List<WarehouseLocation> WarehouseLocations { get; set; } = new List<WarehouseLocation>();

        [ValidateNever]
        public List<MaterialBatch> MaterialBatches { get; set; } = new List<MaterialBatch>();
    }
}