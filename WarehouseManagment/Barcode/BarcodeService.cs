using IronBarCode;

namespace WarehouseManagment.Barcode
{
    public static class BarcodeService
    {
        public static byte[] GenerateBarcodeImage(string barcodeValue)
        {
            var myBarcode = BarcodeWriter.CreateBarcode(barcodeValue, BarcodeWriterEncoding.Code128);

            myBarcode.ResizeTo(400, 100);

            var barcodeBytes = myBarcode.ToJpegBinaryData();

            return barcodeBytes;

        }
    }
}
