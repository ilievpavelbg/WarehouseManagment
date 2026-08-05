using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class BillOfMaterialLineModel
    {
        public int Id { get; set; }

        public int BillOfMaterialsId { get; set; }

        [Required(ErrorMessage = "Изберете материал.")]
        public int MaterialId { get; set; }

        public string MaterialCode { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string UnitOfMeasureName { get; set; } = string.Empty;

        public int UnitOfMeasureId { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335", ErrorMessage = "Количеството за единица трябва да бъде по-голямо от нула.")]
        public decimal QuantityPerUnit { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Фирата не може да бъде отрицателна.")]
        public decimal? WastePercent { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
