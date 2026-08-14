namespace WarehouseManagment.Models
{
    public class AccountingExportResultModel
    {
        public bool Success { get; set; }
        public string? ExternalReference { get; set; }
        public string Message { get; set; } = string.Empty;

        public static AccountingExportResultModel NotConfigured()
        {
            return new AccountingExportResultModel
            {
                Success = false,
                Message = "Счетоводен експорт не е конфигуриран."
            };
        }
    }
}
