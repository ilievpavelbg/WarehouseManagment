using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class WarehouseSettingsModel
    {
        public int Id { get; set; }

        public int? DefaultMaterialWarehouseId { get; set; }

        public int? DefaultWipWarehouseId { get; set; }

        public int? DefaultFinishedGoodsWarehouseId { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public string? UpdatedByUserId { get; set; }

        [ValidateNever]
        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    }
}