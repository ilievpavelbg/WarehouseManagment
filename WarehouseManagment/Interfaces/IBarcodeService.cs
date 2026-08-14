namespace WarehouseManagment.Interfaces
{
    public interface IBarcodeService
    {
        public const string DefaultBarcodeType = "EAN13";

        Task<string> GenerateBarcodeAsync();
        int CalculateCheckDigit(string firstTwelveDigits);
        bool ValidateBarcode(string barcode);
        Task EnsureUniqueAsync(string barcode, int? excludingProductInventoryId = null);
        byte[] RenderBarcodeImage(string barcode);
        void ApplyGeneratedMetadata(WarehouseManagment.Data.ProductInventory inventory);
        Task<int> GenerateMissingProductInventoryBarcodesAsync();
        Task<int> FillMissingBarcodeMetadataAsync();
        Task RecordLabelsPrintedAsync(int productInventoryId, int quantity);
    }
}
