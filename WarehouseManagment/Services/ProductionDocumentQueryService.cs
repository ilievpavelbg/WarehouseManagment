using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ProductionDocumentQueryService : IProductionDocumentQueryService
    {
        private const string UnknownUser = "Неизвестен потребител";
        private const string PieceUnit = "бр";

        private readonly ApplicationDbContext _dbContext;

        public ProductionDocumentQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductionDocumentModel?> GetDocumentAsync(string documentNumber)
        {
            var normalizedDocumentNumber = NormalizeDocumentNumber(documentNumber);
            if (normalizedDocumentNumber == null)
            {
                return null;
            }

            if (normalizedDocumentNumber.StartsWith(ProductionDocumentPrefix.ProductionMaterialTransfer + "-", StringComparison.OrdinalIgnoreCase))
            {
                return await GetPmtDocumentAsync(normalizedDocumentNumber);
            }

            if (normalizedDocumentNumber.StartsWith(ProductionDocumentPrefix.ProductionMaterialConsumption + "-", StringComparison.OrdinalIgnoreCase))
            {
                return await GetPmcDocumentAsync(normalizedDocumentNumber);
            }

            if (normalizedDocumentNumber.StartsWith(ProductionDocumentPrefix.FinishedGoodsReceipt + "-", StringComparison.OrdinalIgnoreCase))
            {
                return await GetFgrDocumentAsync(normalizedDocumentNumber);
            }

            return null;
        }

        private async Task<ProductionDocumentModel?> GetPmtDocumentAsync(string documentNumber)
        {
            var order = await _dbContext.ProductionOrders
                .AsNoTracking()
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                        .ThenInclude(x => x.SourceWarehouse)
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                        .ThenInclude(x => x.SourceWarehouseLocation)
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                        .ThenInclude(x => x.DestinationWarehouse)
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                        .ThenInclude(x => x.DestinationWarehouseLocation)
                .FirstOrDefaultAsync(x => x.MaterialsTransferDocumentNumber == documentNumber);

            if (order == null)
            {
                return null;
            }

            var allocations = order.Materials
                .SelectMany(material => material.Allocations.Select(allocation => new { material, allocation }))
                .OrderBy(x => x.material.MaterialCodeSnapshot)
                .ThenBy(x => x.allocation.CreatedOn)
                .ToList();

            return new ProductionDocumentModel
            {
                DocumentNumber = documentNumber,
                DocumentType = ProductionDocumentPrefix.ProductionMaterialTransfer,
                Title = "Прехвърляне на материали към производство",
                ProductionOrderId = order.Id,
                ProductionOrderNumber = order.OrderNumber,
                Date = order.MaterialsTransferredOn ?? allocations.Select(x => (DateTime?)x.allocation.CreatedOn).FirstOrDefault(),
                UserName = await ResolveUserNameAsync(order.MaterialsTransferredByUserId),
                SourceWarehouse = JoinDistinct(allocations.Select(x => FormatWarehouse(x.allocation.SourceWarehouse))),
                DestinationWarehouse = JoinDistinct(allocations.Select(x => FormatWarehouse(x.allocation.DestinationWarehouse))),
                Lines = allocations.Select(x => new ProductionDocumentLineModel
                {
                    MaterialCode = x.material.MaterialCodeSnapshot,
                    MaterialName = x.material.MaterialNameSnapshot,
                    BatchLotDisplay = FormatBatchLot(x.allocation.BatchNumberSnapshot, x.allocation.LotNumberSnapshot),
                    SourceLocation = FormatLocation(x.allocation.SourceWarehouseLocation),
                    DestinationLocation = FormatLocation(x.allocation.DestinationWarehouseLocation),
                    Quantity = x.allocation.Quantity,
                    UnitOfMeasure = x.material.UnitNameSnapshot
                }).ToList()
            };
        }

        private async Task<ProductionDocumentModel?> GetPmcDocumentAsync(string documentNumber)
        {
            var consumptions = await _dbContext.ProductionOrderMaterialConsumptions
                .AsNoTracking()
                .Include(x => x.ProductionOrderMaterial)
                    .ThenInclude(x => x.ProductionOrder)
                .Include(x => x.ProductionOrderMaterialAllocation)
                .Include(x => x.Warehouse)
                .Where(x => x.DocumentNumber == documentNumber)
                .OrderBy(x => x.ProductionOrderMaterial.MaterialCodeSnapshot)
                .ThenBy(x => x.CreatedOn)
                .ToListAsync();

            if (!consumptions.Any())
            {
                return null;
            }

            var first = consumptions.First();
            var order = first.ProductionOrderMaterial.ProductionOrder;

            return new ProductionDocumentModel
            {
                DocumentNumber = documentNumber,
                DocumentType = ProductionDocumentPrefix.ProductionMaterialConsumption,
                Title = "Разход на материали за производство",
                ProductionOrderId = order.Id,
                ProductionOrderNumber = order.OrderNumber,
                Date = consumptions.Select(x => (DateTime?)x.CreatedOn).FirstOrDefault(),
                UserName = await ResolveUserNameAsync(first.CreatedByUserId),
                SourceWarehouse = JoinDistinct(consumptions.Select(x => FormatWarehouse(x.Warehouse))),
                Lines = consumptions.Select(x => new ProductionDocumentLineModel
                {
                    MaterialCode = x.ProductionOrderMaterial.MaterialCodeSnapshot,
                    MaterialName = x.ProductionOrderMaterial.MaterialNameSnapshot,
                    BatchLotDisplay = FormatBatchLot(x.BatchNumberSnapshot, x.LotNumberSnapshot),
                    Quantity = x.Quantity,
                    UnitOfMeasure = x.ProductionOrderMaterial.UnitNameSnapshot,
                    Reference = x.ProductionOrderMaterialAllocationId.HasValue
                        ? "PMT разпределение"
                        : string.Empty
                }).ToList()
            };
        }

        private async Task<ProductionDocumentModel?> GetFgrDocumentAsync(string documentNumber)
        {
            var receipt = await _dbContext.ProductionFinishedGoodsReceipts
                .AsNoTracking()
                .Include(x => x.ProductionOrder)
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x => x.DocumentNumber == documentNumber);

            if (receipt == null)
            {
                return null;
            }

            return new ProductionDocumentModel
            {
                DocumentNumber = documentNumber,
                DocumentType = ProductionDocumentPrefix.FinishedGoodsReceipt,
                Title = "Приемане на готова продукция",
                ProductionOrderId = receipt.ProductionOrderId,
                ProductionOrderNumber = receipt.ProductionOrder.OrderNumber,
                Date = receipt.CreatedOn,
                UserName = await ResolveUserNameAsync(receipt.CreatedByUserId),
                DestinationWarehouse = FormatWarehouse(receipt.Warehouse),
                ProductSku = receipt.ProductSkuSnapshot,
                ProductName = receipt.ProductDescriptionSnapshot ?? string.Empty,
                Size = receipt.SizeSnapshot,
                Quantity = receipt.Quantity,
                UnitOfMeasure = PieceUnit
            };
        }

        private async Task<string> ResolveUserNameAsync(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return UnknownUser;
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.UserName ?? x.Email)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(user) ? UnknownUser : user;
        }

        private static string? NormalizeDocumentNumber(string? documentNumber)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                return null;
            }

            var value = documentNumber.Trim();
            return value.StartsWith(ProductionDocumentPrefix.ProductionMaterialTransfer + "-", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith(ProductionDocumentPrefix.ProductionMaterialConsumption + "-", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith(ProductionDocumentPrefix.FinishedGoodsReceipt + "-", StringComparison.OrdinalIgnoreCase)
                ? value
                : null;
        }

        private static string FormatWarehouse(Warehouse? warehouse)
        {
            return warehouse == null ? "-" : $"{warehouse.Code} - {warehouse.Name}";
        }

        private static string FormatLocation(WarehouseLocation? location)
        {
            return location == null ? "-" : $"{location.Code} - {location.Name}";
        }

        private static string FormatBatchLot(string? batchNumber, string? lotNumber)
        {
            var values = new[] { batchNumber, lotNumber }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return values.Any() ? string.Join(" / ", values) : "-";
        }

        private static string JoinDistinct(IEnumerable<string> values)
        {
            var distinctValues = values
                .Where(x => !string.IsNullOrWhiteSpace(x) && x != "-")
                .Distinct()
                .ToList();

            return distinctValues.Any() ? string.Join("; ", distinctValues) : "-";
        }
    }
}
