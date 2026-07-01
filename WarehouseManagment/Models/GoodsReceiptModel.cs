using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class GoodsReceiptModel
    {
        [Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [ValidateNever]
        public string MaterialCode { get; set; } = string.Empty;

        [ValidateNever]
        public string MaterialName { get; set; } = string.Empty;

        [ValidateNever]
        public string UnitOfMeasureName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Изберете склад.")]
        public int WarehouseId { get; set; }

        public int? WarehouseLocationId { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        [StringLength(100)]
        public string? LotNumber { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335", ErrorMessage = "Въведете получено количество по-голямо от нула.")]
        public decimal ReceivedQuantity { get; set; }

        public int? SupplierId { get; set; }

        [StringLength(100)]
        public string? DocumentNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [ValidateNever]
        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        [ValidateNever]
        public List<WarehouseLocation> WarehouseLocations { get; set; } = new List<WarehouseLocation>();

        [ValidateNever]
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}