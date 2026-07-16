using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Models;

namespace WarehouseManagment.Data
{
    public class DocumentSequence
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DocumentType DocumentType { get; set; }

        [Range(2000, 9999)]
        public int Year { get; set; }

        [Range(0, int.MaxValue)]
        public int LastNumber { get; set; }

        public DateTime UpdatedOn { get; set; } = DateTime.Now;
    }
}