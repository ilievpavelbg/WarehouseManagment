using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class WarehouseSettings
    {
        [Key]
        public int Id { get; set; }

        public int? DefaultMaterialWarehouseId { get; set; }
        public Warehouse? DefaultMaterialWarehouse { get; set; }

        public int? DefaultWipWarehouseId { get; set; }
        public Warehouse? DefaultWipWarehouse { get; set; }

        public int? DefaultFinishedGoodsWarehouseId { get; set; }
        public Warehouse? DefaultFinishedGoodsWarehouse { get; set; }

        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? UpdatedByUserId { get; set; }
    }
}