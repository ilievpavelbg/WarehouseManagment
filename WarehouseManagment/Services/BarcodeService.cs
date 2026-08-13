using BarcodeStandard;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Services
{
    public class BarcodeService : IBarcodeService
    {
        private const string InternalPrefix = "280123";
        private readonly ApplicationDbContext _dbContext;

        public BarcodeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateBarcodeAsync()
        {
            var reservedBarcodes = await _dbContext.ProductInventory
                .Where(x => x.Barcode != null && x.Barcode.StartsWith(InternalPrefix))
                .Select(x => x.Barcode!)
                .ToListAsync();

            return GenerateBarcode(reservedBarcodes);
        }

        public int CalculateCheckDigit(string firstTwelveDigits)
        {
            if (string.IsNullOrWhiteSpace(firstTwelveDigits) || firstTwelveDigits.Length != 12 || !firstTwelveDigits.All(char.IsDigit))
            {
                throw new ArgumentException("EAN-13 баркодът трябва да има 12 цифри преди контролната цифра.", nameof(firstTwelveDigits));
            }

            var sum = 0;
            for (var i = 0; i < firstTwelveDigits.Length; i++)
            {
                var digit = firstTwelveDigits[i] - '0';
                sum += i % 2 == 0 ? digit : digit * 3;
            }

            return (10 - sum % 10) % 10;
        }

        public bool ValidateBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || barcode.Length != 13 || !barcode.All(char.IsDigit))
            {
                return false;
            }

            return CalculateCheckDigit(barcode[..12]) == barcode[12] - '0';
        }

        public async Task EnsureUniqueAsync(string barcode, int? excludingProductInventoryId = null)
        {
            if (!ValidateBarcode(barcode))
            {
                throw new InvalidOperationException("Баркодът трябва да бъде валиден EAN-13 номер.");
            }

            var exists = await _dbContext.ProductInventory
                .AnyAsync(x => x.Barcode == barcode && (!excludingProductInventoryId.HasValue || x.Id != excludingProductInventoryId.Value));

            if (exists)
            {
                throw new InvalidOperationException("Вече съществува артикул с този баркод.");
            }
        }

        public byte[] RenderBarcodeImage(string barcode)
        {
            if (!ValidateBarcode(barcode))
            {
                throw new InvalidOperationException("Невалиден EAN-13 баркод.");
            }

            var generator = new Barcode
            {
                IncludeLabel = true
            };
            var image = generator.Encode(BarcodeStandard.Type.Ean13, barcode, SKColors.Black, SKColors.White, 260, 90);
            var bitmap = SKBitmap.FromImage(image);

            using var stream = new MemoryStream();
            bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
            return stream.ToArray();
        }

        public async Task<int> GenerateMissingProductInventoryBarcodesAsync()
        {
            var inventories = await _dbContext.ProductInventory
                .Where(x => string.IsNullOrWhiteSpace(x.Barcode))
                .OrderBy(x => x.Id)
                .ToListAsync();

            var reservedBarcodes = await _dbContext.ProductInventory
                .Where(x => !string.IsNullOrWhiteSpace(x.Barcode))
                .Select(x => x.Barcode!)
                .ToListAsync();

            foreach (var inventory in inventories)
            {
                inventory.Barcode = GenerateBarcode(reservedBarcodes);
                reservedBarcodes.Add(inventory.Barcode);
            }

            await _dbContext.SaveChangesAsync();
            return inventories.Count;
        }

        private string GenerateBarcode(ICollection<string> reservedBarcodes)
        {
            var sequence = reservedBarcodes
                .Where(x => x.StartsWith(InternalPrefix) && x.Length == 13)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            var nextNumber = 1;
            if (!string.IsNullOrWhiteSpace(sequence))
            {
                var serialPart = sequence.Substring(InternalPrefix.Length, 12 - InternalPrefix.Length);
                if (int.TryParse(serialPart, out var parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            for (var attempt = 0; attempt < 1000; attempt++)
            {
                var serial = nextNumber + attempt;
                var firstTwelve = InternalPrefix + serial.ToString().PadLeft(12 - InternalPrefix.Length, '0');
                var barcode = firstTwelve + CalculateCheckDigit(firstTwelve);

                if (!reservedBarcodes.Contains(barcode))
                {
                    return barcode;
                }
            }

            throw new InvalidOperationException("Не може да бъде генериран уникален баркод.");
        }
    }
}
