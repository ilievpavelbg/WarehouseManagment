namespace WarehouseManagment.Models
{
    public class MaterialImportSummaryModel
    {
        public int Created { get; set; }

        public int Updated { get; set; }

        public int Skipped { get; set; }

        public List<string> Warnings { get; set; } = new List<string>();

        public List<string> Errors { get; set; } = new List<string>();
    }
}