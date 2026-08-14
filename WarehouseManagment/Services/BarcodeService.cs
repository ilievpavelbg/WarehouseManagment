using BarcodeStandard;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class BarcodeService : IBarcodeService
    {
        private const string InternalPrefix = "280123";
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogService _auditLogService;

        public BarcodeService(
            ApplicationDbContext dbContext,
            ICurrentUserService currentUserService,
            IAuditLogService auditLogService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
        }

        public async Task<string> GenerateBarcodeAsync()
        {
            var reservedBarcodes = await _dbContext.ProductInventory
                .Where(x => x.BarcodeValue != null && x.BarcodeValue.StartsWith(InternalPrefix))
                .Select(x => x.BarcodeValue!)
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
                .AnyAsync(x => x.BarcodeValue == barcode && (!excludingProductInventoryId.HasValue || x.Id != excludingProductInventoryId.Value));

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

        public void ApplyGeneratedMetadata(ProductInventory inventory)
        {
            inventory.BarcodeType = IBarcodeService.DefaultBarcodeType;
            inventory.BarcodeGeneratedOn = DateTime.Now;
            inventory.BarcodeGeneratedByUserId = _currentUserService.UserId;
            inventory.BarcodeGeneratedByUserNameSnapshot = _currentUserService.UserName;
        }

        public async Task<int> GenerateMissingProductInventoryBarcodesAsync()
        {
            var inventories = await _dbContext.ProductInventory
                .Include(x => x.Product)
                .Where(x => string.IsNullOrWhiteSpace(x.BarcodeValue))
                .OrderBy(x => x.Id)
                .ToListAsync();

            var reservedBarcodes = await _dbContext.ProductInventory
                .Where(x => !string.IsNullOrWhiteSpace(x.BarcodeValue))
                .Select(x => x.BarcodeValue!)
                .ToListAsync();

            foreach (var inventory in inventories)
            {
                inventory.BarcodeValue = GenerateBarcode(reservedBarcodes);
                ApplyGeneratedMetadata(inventory);
                reservedBarcodes.Add(inventory.BarcodeValue);

                await AddBarcodeAuditAsync(AuditActionType.BarcodeGenerated, inventory, "Генериран POS баркод.");
            }

            await _dbContext.SaveChangesAsync();
            return inventories.Count;
        }

        public async Task<int> FillMissingBarcodeMetadataAsync()
        {
            var inventories = await _dbContext.ProductInventory
                .Include(x => x.Product)
                .Where(x => !string.IsNullOrWhiteSpace(x.BarcodeValue) && string.IsNullOrWhiteSpace(x.BarcodeType))
                .OrderBy(x => x.Id)
                .ToListAsync();

            var updated = 0;
            foreach (var inventory in inventories)
            {
                if (!ValidateBarcode(inventory.BarcodeValue!))
                {
                    continue;
                }

                inventory.BarcodeType = IBarcodeService.DefaultBarcodeType;
                updated++;

                await AddBarcodeAuditAsync(AuditActionType.BarcodeMetadataUpdated, inventory, "Попълнен тип на съществуващ POS баркод.");
            }

            await _dbContext.SaveChangesAsync();
            return updated;
        }

        public async Task RecordLabelsPrintedAsync(int productInventoryId, int quantity)
        {
            if (quantity < 1)
            {
                throw new InvalidOperationException("Броят етикети трябва да бъде по-голям от нула.");
            }

            if (quantity > 500)
            {
                throw new InvalidOperationException("Не може да бъдат отбелязани повече от 500 етикета наведнъж.");
            }

            var inventory = await _dbContext.ProductInventory
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == productInventoryId);

            if (inventory == null || string.IsNullOrWhiteSpace(inventory.BarcodeValue))
            {
                throw new InvalidOperationException("Размерът / вариантът няма генериран POS баркод.");
            }

            inventory.BarcodePrintedOn = DateTime.Now;
            inventory.BarcodePrintCount += quantity;

            await AddBarcodeAuditAsync(
                AuditActionType.BarcodeLabelsPrinted,
                inventory,
                $"Отбелязани като отпечатани {quantity} етикета.");

            await _dbContext.SaveChangesAsync();
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

        private async Task AddBarcodeAuditAsync(AuditActionType actionType, ProductInventory inventory, string description)
        {
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = actionType,
                EntityType = nameof(ProductInventory),
                EntityId = inventory.Id,
                Description = description,
                NewValues = $"SKU={inventory.Product?.SKU ?? inventory.ProductSKU}; Size={inventory.Size}; Barcode={inventory.BarcodeValue}; BarcodeType={inventory.BarcodeType}; PrintCount={inventory.BarcodePrintCount}; Operator={_currentUserService.UserName}; Timestamp={DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            });
        }
    }
}
