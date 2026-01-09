using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class DocumentPostingService : IDocumentPostingService
    {
        private readonly IRepository _repository;
        private readonly IInventoryService _inventoryService;

        public DocumentPostingService(IRepository repository, IInventoryService inventoryService)
        {
            _repository = repository;
            _inventoryService = inventoryService;
        }

        public async Task PostPurchaseReceiptAsync(int receiptId)
        {
            var receipt = await _repository.All<PurchaseReceipt>()
                .Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.Id == receiptId);

            if (receipt == null)
            {
                throw new ArgumentNullException(nameof(receipt));
            }

            if (receipt.Status == DocumentStatus.Posted)
            {
                return;
            }

            if (receipt.Lines.Count == 0)
            {
                throw new InvalidOperationException("Cannot post a receipt without lines.");
            }

            var movements = receipt.Lines.Select(line => new StockMovement
            {
                ItemId = line.ItemId,
                LocationId = line.LocationId,
                Quantity = line.Quantity,
                MovementType = StockMovementType.Receipt,
                ReferenceType = nameof(PurchaseReceipt),
                ReferenceId = receipt.Id,
                OccurredAt = DateTime.UtcNow
            });

            await _inventoryService.ApplyMovementsAsync(movements);

            receipt.Status = DocumentStatus.Posted;
            receipt.PostedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync();
        }

        public async Task PostShipmentAsync(int shipmentId)
        {
            var shipment = await _repository.All<Shipment>()
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == shipmentId);

            if (shipment == null)
            {
                throw new ArgumentNullException(nameof(shipment));
            }

            if (shipment.Status == DocumentStatus.Posted)
            {
                return;
            }

            if (shipment.Lines.Count == 0)
            {
                throw new InvalidOperationException("Cannot post a shipment without lines.");
            }

            var movements = shipment.Lines.Select(line => new StockMovement
            {
                ItemId = line.ItemId,
                LocationId = line.LocationId,
                Quantity = -line.Quantity,
                MovementType = StockMovementType.Issue,
                ReferenceType = nameof(Shipment),
                ReferenceId = shipment.Id,
                OccurredAt = DateTime.UtcNow
            });

            await _inventoryService.ApplyMovementsAsync(movements);

            shipment.Status = DocumentStatus.Posted;
            shipment.PostedAt = DateTime.UtcNow;

            await _repository.SaveChangesAsync();
        }
    }
}
