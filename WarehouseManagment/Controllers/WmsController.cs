using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Models.Wms;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class WmsController : Controller
    {
        private readonly IRepository _repository;

        public WmsController(IRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> InventoryByLocation()
        {
            var balances = await _repository.AllReadonly<InventoryBalance>()
                .Include(b => b.Item)
                .Include(b => b.Location)
                    .ThenInclude(l => l.Zone)
                    .ThenInclude(z => z.Warehouse)
                .OrderBy(b => b.Location.Zone.Warehouse.Name)
                .ThenBy(b => b.Location.Zone.Name)
                .ThenBy(b => b.Location.Name)
                .ThenBy(b => b.Item.SKU)
                .ToListAsync();

            var model = new InventoryByLocationViewModel
            {
                Rows = balances.Select(balance => new InventoryByLocationRow
                {
                    Warehouse = balance.Location.Zone.Warehouse.Name,
                    Zone = balance.Location.Zone.Name,
                    Location = balance.Location.Name,
                    ItemSku = balance.Item.SKU,
                    ItemName = balance.Item.Name,
                    Quantity = balance.Quantity
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> StockMovements(DateTime? from, DateTime? to)
        {
            var query = _repository.AllReadonly<StockMovement>()
                .Include(m => m.Item)
                .Include(m => m.Location)
                    .ThenInclude(l => l.Zone)
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(m => m.OccurredAt >= from.Value);
            }

            if (to.HasValue)
            {
                var inclusiveTo = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.OccurredAt <= inclusiveTo);
            }

            var movements = await query
                .OrderByDescending(m => m.OccurredAt)
                .Take(500)
                .ToListAsync();

            var model = new StockMovementsViewModel
            {
                From = from,
                To = to,
                Movements = movements.Select(movement => new StockMovementRow
                {
                    OccurredAt = movement.OccurredAt,
                    MovementType = movement.MovementType.ToString(),
                    ItemSku = movement.Item.SKU,
                    ItemName = movement.Item.Name,
                    Location = $"{movement.Location.Zone.Name} / {movement.Location.Name}",
                    Quantity = movement.Quantity,
                    ReferenceType = movement.ReferenceType,
                    ReferenceId = movement.ReferenceId
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Documents()
        {
            var receipts = await _repository.AllReadonly<PurchaseReceipt>()
                .Include(r => r.Warehouse)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var shipments = await _repository.AllReadonly<Shipment>()
                .Include(s => s.Warehouse)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var model = new DocumentsListViewModel
            {
                Receipts = receipts,
                Shipments = shipments
            };

            return View(model);
        }
    }
}
