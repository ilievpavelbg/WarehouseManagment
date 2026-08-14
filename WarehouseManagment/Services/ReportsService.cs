using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ReportsService : IReportsService
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportsService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ReportsLandingModel GetLandingModel()
        {
            return new ReportsLandingModel
            {
                Cards = new List<ReportsCardModel>
                {
                    new() { Title = "Управленско табло", Description = "Обобщени показатели за продажби, производство и склад.", Action = "Dashboard", CssClass = "border-primary" },
                    new() { Title = "Продажби", Description = "POS документи, оборот, оператори и артикули.", Action = "Sales", CssClass = "border-success" },
                    new() { Title = "Склад", Description = "Наличности, движения и складов контрол.", Action = "Warehouse", CssClass = "border-info" },
                    new() { Title = "Производство", Description = "Поръчки, операции, работници и материален разход.", Action = "Production", CssClass = "border-warning" },
                    new() { Title = "Артикули / баркод", Description = "Баркодове, печат и проследимост.", Action = "Barcodes", CssClass = "border-secondary" },
                    new() { Title = "Проследимост", Description = "Търсене по SKU, баркод или документ.", Action = "Traceability", CssClass = "border-dark" }
                }
            };
        }

        public async Task<ReportsIndexModel<SalesReportRowModel>> GetSalesAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var query = ApplySalesFilter(_dbContext.PosSales.AsNoTracking().Include(x => x.Lines), filter);
            var totalItems = await query.CountAsync();
            var rows = await query
                .OrderByDescending(x => x.CreatedOn)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new SalesReportRowModel
                {
                    Id = x.Id,
                    Date = x.CreatedOn,
                    DocumentNumber = x.DocumentNumber,
                    Operator = x.CreatedByUserNameSnapshot ?? "-",
                    Items = x.Lines.Count,
                    Quantity = x.Lines.Sum(l => l.Quantity),
                    Subtotal = x.Subtotal,
                    Discount = x.DiscountTotal,
                    Total = x.Total,
                    Payment = PaymentLabel(x.PaymentMethod),
                    Status = x.Status == PosSaleStatus.Reversed ? "Сторнирана" : "Завършена",
                    IsReversed = x.Status == PosSaleStatus.Reversed
                })
                .ToListAsync();

            var kpiQuery = ApplySalesFilter(_dbContext.PosSales.AsNoTracking(), filter);
            var completed = kpiQuery.Where(x => x.Status == PosSaleStatus.Completed);
            var reversed = kpiQuery.Where(x => x.Status == PosSaleStatus.Reversed);
            var documentCount = await completed.CountAsync();
            var netSales = await completed.SumAsync(x => (decimal?)x.Total) ?? 0;
            var grossSales = await completed.SumAsync(x => (decimal?)x.Subtotal) ?? 0;
            var discounts = await completed.SumAsync(x => (decimal?)x.DiscountTotal) ?? 0;
            var reversedValue = await reversed.SumAsync(x => (decimal?)x.Total) ?? 0;
            var quantity = await completed.SelectMany(x => x.Lines).SumAsync(x => (int?)x.Quantity) ?? 0;

            return new ReportsIndexModel<SalesReportRowModel>
            {
                Filter = filter,
                Rows = rows,
                TotalItems = totalItems,
                Kpis = new List<ReportKpiModel>
                {
                    Kpi("POS документи", documentCount.ToString("N0"), "border-primary"),
                    Kpi("Брутни продажби", Money(grossSales), "border-success"),
                    Kpi("Отстъпки", Money(discounts), "border-warning"),
                    Kpi("Нетни продажби", Money(netSales), "border-success"),
                    Kpi("Количество", $"{quantity:N0} бр.", "border-info"),
                    Kpi("Средна кошница", Money(documentCount == 0 ? 0 : netSales / documentCount), "border-primary"),
                    Kpi("Сторнирани", Money(reversedValue), "border-danger")
                }
            };
        }

        public async Task<ReportsIndexModel<SalesByProductReportRowModel>> GetSalesByProductAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var sales = ApplySalesFilter(_dbContext.PosSales.AsNoTracking(), filter)
                .Where(x => x.Status == PosSaleStatus.Completed);

            var query = sales.SelectMany(x => x.Lines)
                .GroupBy(x => new { x.ProductSKU, x.ProductDescriptionSnapshot, x.SizeSnapshot })
                .Select(x => new SalesByProductReportRowModel
                {
                    SKU = x.Key.ProductSKU,
                    Product = x.Key.ProductDescriptionSnapshot ?? "",
                    Variant = x.Key.SizeSnapshot,
                    QuantitySold = x.Sum(l => l.Quantity),
                    GrossValue = x.Sum(l => l.UnitPrice * l.Quantity),
                    Discount = x.Sum(l => l.DiscountAmount),
                    NetValue = x.Sum(l => l.LineTotal),
                    Documents = x.Select(l => l.PosSaleId).Distinct().Count()
                })
                .OrderByDescending(x => x.NetValue);

            var totalItems = await query.CountAsync();
            var rows = await Page(query, filter).ToListAsync();
            return Wrap(filter, rows, totalItems);
        }

        public async Task<ReportsIndexModel<SalesByOperatorReportRowModel>> GetSalesByOperatorAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var query = ApplySalesFilter(_dbContext.PosSales.AsNoTracking().Include(x => x.Lines), filter)
                .GroupBy(x => x.CreatedByUserNameSnapshot ?? "-")
                .Select(x => new SalesByOperatorReportRowModel
                {
                    Operator = x.Key,
                    Documents = x.Count(s => s.Status == PosSaleStatus.Completed),
                    Items = x.Where(s => s.Status == PosSaleStatus.Completed).SelectMany(s => s.Lines).Count(),
                    Quantity = x.Where(s => s.Status == PosSaleStatus.Completed).SelectMany(s => s.Lines).Sum(l => l.Quantity),
                    NetSales = x.Where(s => s.Status == PosSaleStatus.Completed).Sum(s => s.Total),
                    AverageBasket = x.Count(s => s.Status == PosSaleStatus.Completed) == 0 ? 0 : x.Where(s => s.Status == PosSaleStatus.Completed).Sum(s => s.Total) / x.Count(s => s.Status == PosSaleStatus.Completed),
                    Reversals = x.Count(s => s.Status == PosSaleStatus.Reversed)
                })
                .OrderByDescending(x => x.NetSales);

            var totalItems = await query.CountAsync();
            var rows = await Page(query, filter).ToListAsync();
            return Wrap(filter, rows, totalItems);
        }

        public async Task<ReportsIndexModel<WarehouseStockReportRowModel>> GetWarehouseStockAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var settings = await _dbContext.WarehouseSettings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync();
            var defaultMaterialWarehouseId = settings?.DefaultMaterialWarehouseId;
            var wipWarehouseId = settings?.DefaultWipWarehouseId;

            var query = _dbContext.Materials.AsNoTracking()
                .Include(x => x.MaterialCategory)
                .Include(x => x.UnitOfMeasure)
                .Where(x => string.IsNullOrWhiteSpace(filter.Search) || x.Code.Contains(filter.Search) || x.Name.Contains(filter.Search))
                .Select(x => new WarehouseStockReportRowModel
                {
                    MaterialCode = x.Code,
                    MaterialName = x.Name,
                    Category = x.MaterialCategory.Name,
                    Unit = x.UnitOfMeasure.Symbol ?? x.UnitOfMeasure.Name,
                    DefaultWarehouseQuantity = defaultMaterialWarehouseId.HasValue ? x.MaterialStocks.Where(s => s.WarehouseId == defaultMaterialWarehouseId.Value).Sum(s => s.Quantity) : 0,
                    WipQuantity = wipWarehouseId.HasValue ? x.MaterialStocks.Where(s => s.WarehouseId == wipWarehouseId.Value).Sum(s => s.Quantity) : 0,
                    MinimumStock = x.MinimumStock,
                    Status = !defaultMaterialWarehouseId.HasValue
                        ? "Не е настроен основен склад"
                        : x.MinimumStock <= 0
                            ? "OK"
                            : x.MaterialStocks.Where(s => s.WarehouseId == defaultMaterialWarehouseId.Value).Sum(s => s.Quantity) <= 0
                                ? "Няма наличност"
                                : x.MaterialStocks.Where(s => s.WarehouseId == defaultMaterialWarehouseId.Value).Sum(s => s.Quantity) < x.MinimumStock
                                    ? "Под минимум"
                                    : "OK"
                })
                .OrderBy(x => x.MaterialCode);

            var totalItems = await query.CountAsync();
            var rows = await Page(query, filter).ToListAsync();
            return Wrap(filter, rows, totalItems);
        }

        public async Task<ReportsIndexModel<WarehouseMovementReportRowModel>> GetWarehouseMovementsAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var query = _dbContext.InventoryMovements.AsNoTracking()
                .Include(x => x.Material)
                .Include(x => x.Product)
                .Include(x => x.ProductInventory)
                .Include(x => x.Warehouse)
                .Include(x => x.WarehouseLocation)
                .Include(x => x.DestinationWarehouse)
                .Include(x => x.DestinationWarehouseLocation)
                .Where(x => !filter.DateFrom.HasValue || x.CreatedOn >= filter.DateFrom.Value.Date)
                .Where(x => !filter.DateTo.HasValue || x.CreatedOn < filter.DateTo.Value.Date.AddDays(1))
                .Where(x => !filter.WarehouseId.HasValue || x.WarehouseId == filter.WarehouseId || x.DestinationWarehouseId == filter.WarehouseId)
                .Where(x => !filter.MovementType.HasValue || x.MovementType == filter.MovementType)
                .Where(x => string.IsNullOrWhiteSpace(filter.Search) ||
                    (x.Material != null && (x.Material.Code.Contains(filter.Search) || x.Material.Name.Contains(filter.Search))) ||
                    (x.Product != null && (x.Product.SKU.Contains(filter.Search) || (x.Product.Description != null && x.Product.Description.Contains(filter.Search)))) ||
                    (x.ReferenceNumber != null && x.ReferenceNumber.Contains(filter.Search)));

            var totalItems = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.CreatedOn)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new WarehouseMovementReportRowModel
                {
                    Date = x.CreatedOn,
                    Movement = x.MovementType.ToString(),
                    Item = x.Material != null ? x.Material.Code + " - " + x.Material.Name : x.Product != null ? x.Product.SKU + " - " + x.Product.Description : "-",
                    Quantity = x.Quantity,
                    Source = x.Warehouse != null ? x.Warehouse.Code + " - " + x.Warehouse.Name : "-",
                    Destination = x.DestinationWarehouse != null ? x.DestinationWarehouse.Code + " - " + x.DestinationWarehouse.Name : "-",
                    ReferenceType = x.ReferenceType,
                    DocumentNumber = x.ReferenceNumber,
                    User = x.UserId ?? "-"
                })
                .ToListAsync();
            return Wrap(filter, rows, totalItems);
        }

        public async Task<ReportsIndexModel<FinishedGoodsReportRowModel>> GetFinishedGoodsAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var query = _dbContext.ProductInventory.AsNoTracking()
                .Include(x => x.Product)
                .Where(x => string.IsNullOrWhiteSpace(filter.Search) ||
                    x.Product.SKU.Contains(filter.Search) ||
                    (x.Product.Description != null && x.Product.Description.Contains(filter.Search)) ||
                    (x.BarcodeValue != null && x.BarcodeValue.Contains(filter.Search)))
                .Select(x => new FinishedGoodsReportRowModel
                {
                    SKU = x.Product.SKU,
                    Product = x.Product.Description ?? "",
                    Variant = x.Size.ToString(),
                    Barcode = x.BarcodeValue,
                    CurrentQuantity = x.Quantity,
                    FinishedGoodsWarehouse = _dbContext.WarehouseSettings.Select(s => s.DefaultFinishedGoodsWarehouse!.Code + " - " + s.DefaultFinishedGoodsWarehouse.Name).FirstOrDefault() ?? "-",
                    LatestFgr = _dbContext.ProductionFinishedGoodsReceipts.Where(r => r.ProductInventoryId == x.Id).OrderByDescending(r => r.CreatedOn).Select(r => r.DocumentNumber).FirstOrDefault(),
                    LastReceiptDate = _dbContext.ProductionFinishedGoodsReceipts.Where(r => r.ProductInventoryId == x.Id).OrderByDescending(r => r.CreatedOn).Select(r => (DateTime?)r.CreatedOn).FirstOrDefault(),
                    SoldQuantity = _dbContext.PosSaleLines.Where(l => l.ProductInventoryId == x.Id && l.PosSale.Status == PosSaleStatus.Completed && (!filter.DateFrom.HasValue || l.PosSale.CreatedOn >= filter.DateFrom.Value.Date) && (!filter.DateTo.HasValue || l.PosSale.CreatedOn < filter.DateTo.Value.Date.AddDays(1))).Sum(l => (int?)l.Quantity) ?? 0
                })
                .OrderBy(x => x.SKU);

            var totalItems = await query.CountAsync();
            var rows = await Page(query, filter).ToListAsync();
            return Wrap(filter, rows, totalItems);
        }

        public async Task<ReportsIndexModel<ProductionReportRowModel>> GetProductionAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var query = _dbContext.ProductionOrders.AsNoTracking()
                .Include(x => x.Operations)
                .Include(x => x.ProductInventory)
                .Where(x => !filter.DateFrom.HasValue || (x.PlannedStartDate ?? x.CreatedOn) >= filter.DateFrom.Value.Date)
                .Where(x => !filter.DateTo.HasValue || (x.PlannedStartDate ?? x.CreatedOn) < filter.DateTo.Value.Date.AddDays(1))
                .Where(x => !filter.ProductionStatus.HasValue || x.Status == filter.ProductionStatus)
                .Where(x => string.IsNullOrWhiteSpace(filter.Search) || x.OrderNumber.Contains(filter.Search) || x.ProductSkuSnapshot.Contains(filter.Search) || (x.ProductDescriptionSnapshot != null && x.ProductDescriptionSnapshot.Contains(filter.Search)));

            var totalItems = await query.CountAsync();
            var rows = await query.OrderByDescending(x => x.CreatedOn)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new ProductionReportRowModel
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    Product = x.ProductSkuSnapshot + " - " + x.ProductDescriptionSnapshot,
                    Variant = x.ProductInventory != null ? x.ProductInventory.Size.ToString() : "-",
                    PlannedQuantity = x.PlannedQuantity,
                    GoodQuantity = x.Operations.Sum(o => o.CompletedQuantity),
                    RejectedQuantity = x.Operations.Sum(o => o.RejectedQuantity),
                    Start = x.ActualStartDate,
                    End = x.ActualEndDate,
                    Status = x.Status.ToString(),
                    ProgressPercent = x.PlannedQuantity <= 0 ? 0 : Math.Min(100, x.Operations.Sum(o => o.CompletedQuantity) / x.PlannedQuantity * 100)
                })
                .ToListAsync();

            var active = await query.CountAsync(x => x.Status == ProductionOrderStatus.Released || x.Status == ProductionOrderStatus.InProgress);
            var completed = await query.CountAsync(x => x.Status == ProductionOrderStatus.Completed);
            var produced = await query.SelectMany(x => x.Operations).SumAsync(x => (decimal?)x.CompletedQuantity) ?? 0;
            return new ReportsIndexModel<ProductionReportRowModel>
            {
                Filter = filter,
                Rows = rows,
                TotalItems = totalItems,
                Kpis = new List<ReportKpiModel>
                {
                    Kpi("Активни поръчки", active.ToString("N0"), "border-primary"),
                    Kpi("Завършени", completed.ToString("N0"), "border-success"),
                    Kpi("Произведени единици", QuantityDisplayFormatter.Format(produced, "бр", true), "border-info")
                }
            };
        }

        public async Task<ReportsIndexModel<WorkerOperationReportRowModel>> GetWorkerOperationsAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var query = _dbContext.ProductionWorkEntries.AsNoTracking()
                .Include(x => x.ProductionOrderOperation)
                .Where(x => !filter.DateFrom.HasValue || x.CreatedOn >= filter.DateFrom.Value.Date)
                .Where(x => !filter.DateTo.HasValue || x.CreatedOn < filter.DateTo.Value.Date.AddDays(1))
                .GroupBy(x => new { Worker = x.UserNameSnapshot ?? "-", x.ProductionOrderOperation.RequiredRoleSnapshot, x.ProductionOrderOperation.OperationNameSnapshot })
                .Select(x => new WorkerOperationReportRowModel
                {
                    Worker = x.Key.Worker,
                    Role = x.Key.RequiredRoleSnapshot,
                    Operation = x.Key.OperationNameSnapshot,
                    GoodQuantity = x.Sum(e => e.ReportedCompletedQuantity),
                    RejectedQuantity = x.Sum(e => e.ReportedRejectedQuantity),
                    Reports = x.Count(),
                    AverageMinutes = x.Where(e => e.WorkStartedOn.HasValue && e.WorkEndedOn.HasValue).Any()
                        ? x.Where(e => e.WorkStartedOn.HasValue && e.WorkEndedOn.HasValue).Average(e => (decimal?)EF.Functions.DateDiffMinute(e.WorkStartedOn!.Value, e.WorkEndedOn!.Value))
                        : null
                })
                .OrderByDescending(x => x.GoodQuantity);

            var totalItems = await query.CountAsync();
            var rows = await Page(query, filter).ToListAsync();
            return Wrap(filter, rows, totalItems);
        }

        public async Task<ReportsIndexModel<MaterialConsumptionReportRowModel>> GetMaterialConsumptionAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var query = _dbContext.ProductionOrderMaterials.AsNoTracking()
                .Include(x => x.ProductionOrder)
                .Where(x => string.IsNullOrWhiteSpace(filter.Search) || x.ProductionOrder.OrderNumber.Contains(filter.Search) || x.MaterialCodeSnapshot.Contains(filter.Search) || x.MaterialNameSnapshot.Contains(filter.Search))
                .Select(x => new MaterialConsumptionReportRowModel
                {
                    ProductionOrder = x.ProductionOrder.OrderNumber,
                    Product = x.ProductionOrder.ProductSkuSnapshot + " - " + x.ProductionOrder.ProductDescriptionSnapshot,
                    Material = x.MaterialCodeSnapshot + " - " + x.MaterialNameSnapshot,
                    Transferred = x.TransferredQuantity,
                    Consumed = x.ConsumedQuantity,
                    Returned = x.ReturnedQuantity,
                    Remainder = x.TransferredQuantity - x.ConsumedQuantity - x.ReturnedQuantity,
                    Unit = x.UnitNameSnapshot,
                    PmcDocument = x.ProductionOrder.MaterialConsumptionDocumentNumber
                })
                .OrderBy(x => x.ProductionOrder);

            var totalItems = await query.CountAsync();
            var rows = await Page(query, filter).ToListAsync();
            return Wrap(filter, rows, totalItems);
        }

        public async Task<ReportsIndexModel<BarcodeReportRowModel>> GetBarcodesAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var query = _dbContext.ProductInventory.AsNoTracking()
                .Include(x => x.Product)
                .Where(x => string.IsNullOrWhiteSpace(filter.Search) ||
                    x.Product.SKU.Contains(filter.Search) ||
                    (x.Product.Description != null && x.Product.Description.Contains(filter.Search)) ||
                    (x.BarcodeValue != null && x.BarcodeValue.Contains(filter.Search)))
                .Where(x => !filter.MissingMetadataOnly || !string.IsNullOrWhiteSpace(x.BarcodeValue) && string.IsNullOrWhiteSpace(x.BarcodeType))
                .Where(x => !filter.NeverPrintedOnly || x.BarcodePrintCount == 0)
                .Select(x => new BarcodeReportRowModel
                {
                    SKU = x.Product.SKU,
                    Product = x.Product.Description ?? "",
                    Variant = x.Size.ToString(),
                    BarcodeValue = x.BarcodeValue,
                    BarcodeType = x.BarcodeType,
                    GeneratedOn = x.BarcodeGeneratedOn,
                    GeneratedBy = x.BarcodeGeneratedByUserNameSnapshot,
                    LastPrintedOn = x.BarcodePrintedOn,
                    PrintCount = x.BarcodePrintCount,
                    CurrentStock = x.Quantity
                })
                .OrderBy(x => x.SKU);

            var totalItems = await query.CountAsync();
            var rows = await Page(query, filter).ToListAsync();
            return Wrap(filter, rows, totalItems);
        }

        public async Task<TraceabilityModel> GetTraceabilityAsync(ReportFilterModel filter)
        {
            Normalize(filter);
            var term = filter.Search?.Trim();
            var events = new List<TraceabilityEventModel>();
            if (string.IsNullOrWhiteSpace(term))
            {
                return new TraceabilityModel { Filter = filter, Events = events };
            }

            events.AddRange(await _dbContext.ProductionFinishedGoodsReceipts.AsNoTracking()
                .Where(x => x.DocumentNumber.Contains(term) || x.ProductSkuSnapshot.Contains(term) || (x.ProductInventory.BarcodeValue != null && x.ProductInventory.BarcodeValue.Contains(term)))
                .OrderByDescending(x => x.CreatedOn)
                .Take(30)
                .Select(x => new TraceabilityEventModel
                {
                    Date = x.CreatedOn,
                    EventType = "FGR",
                    Description = "Приемане на готова продукция: " + x.ProductSkuSnapshot + " / " + x.SizeSnapshot,
                    DocumentNumber = x.DocumentNumber,
                    Controller = "ProductionDocuments",
                    Action = "Fgr",
                    RouteId = x.ProductionOrderId
                }).ToListAsync());

            events.AddRange(await _dbContext.PosSales.AsNoTracking()
                .Where(x => x.DocumentNumber.Contains(term) || x.Lines.Any(l => l.ProductSKU.Contains(term) || (l.ProductInventory.BarcodeValue != null && l.ProductInventory.BarcodeValue.Contains(term))))
                .OrderByDescending(x => x.CreatedOn)
                .Take(30)
                .Select(x => new TraceabilityEventModel
                {
                    Date = x.CreatedOn,
                    EventType = x.Status == PosSaleStatus.Reversed ? "POS сторно" : "POS продажба",
                    Description = "POS документ " + x.DocumentNumber,
                    DocumentNumber = x.DocumentNumber,
                    Controller = "Pos",
                    Action = "Details",
                    RouteId = x.Id
                }).ToListAsync());

            events.AddRange(await _dbContext.ProductionOrders.AsNoTracking()
                .Where(x => x.OrderNumber.Contains(term) || x.ProductSkuSnapshot.Contains(term) || (x.MaterialsTransferDocumentNumber != null && x.MaterialsTransferDocumentNumber.Contains(term)) || (x.MaterialConsumptionDocumentNumber != null && x.MaterialConsumptionDocumentNumber.Contains(term)))
                .OrderByDescending(x => x.CreatedOn)
                .Take(30)
                .Select(x => new TraceabilityEventModel
                {
                    Date = x.CreatedOn,
                    EventType = "Производствена поръчка",
                    Description = x.ProductSkuSnapshot + " - " + x.ProductDescriptionSnapshot,
                    DocumentNumber = x.OrderNumber,
                    Controller = "ProductionOrder",
                    Action = "Details",
                    RouteId = x.Id
                }).ToListAsync());

            return new TraceabilityModel
            {
                Filter = filter,
                Events = events.OrderByDescending(x => x.Date).Take(100).ToList()
            };
        }

        public async Task<ManagementDashboardModel> GetManagementDashboardAsync(ReportFilterModel filter)
        {
            NormalizePeriod(filter);
            var completedSales = ApplySalesFilter(_dbContext.PosSales.AsNoTracking(), filter).Where(x => x.Status == PosSaleStatus.Completed);
            var netSales = await completedSales.SumAsync(x => (decimal?)x.Total) ?? 0;
            var docs = await completedSales.CountAsync();
            var units = await completedSales.SelectMany(x => x.Lines).SumAsync(x => (int?)x.Quantity) ?? 0;
            var activeProduction = await _dbContext.ProductionOrders.AsNoTracking().CountAsync(x => x.Status == ProductionOrderStatus.Released || x.Status == ProductionOrderStatus.InProgress);

            return new ManagementDashboardModel
            {
                Filter = filter,
                Kpis = new List<ReportKpiModel>
                {
                    Kpi("Нетни POS продажби", Money(netSales), "border-success"),
                    Kpi("POS документи", docs.ToString("N0"), "border-primary"),
                    Kpi("Средна кошница", Money(docs == 0 ? 0 : netSales / docs), "border-info"),
                    Kpi("Продадени единици", $"{units:N0} бр.", "border-info"),
                    Kpi("Активни производствени поръчки", activeProduction.ToString("N0"), "border-warning")
                },
                SalesByDay = await completedSales
                    .GroupBy(x => x.CreatedOn.Date)
                    .OrderBy(x => x.Key)
                    .Select(x => new ChartPointModel { Label = x.Key.ToString("dd.MM"), Value = x.Sum(s => s.Total) })
                    .ToListAsync(),
                TopProducts = await completedSales.SelectMany(x => x.Lines)
                    .GroupBy(x => x.ProductSKU)
                    .OrderByDescending(x => x.Sum(l => l.LineTotal))
                    .Take(10)
                    .Select(x => new ChartPointModel { Label = x.Key, Value = x.Sum(l => l.LineTotal) })
                    .ToListAsync(),
                ProductionByDay = await _dbContext.ProductionOrders.AsNoTracking()
                    .Where(x => x.ActualEndDate.HasValue && x.ActualEndDate >= filter.DateFrom && x.ActualEndDate < filter.DateTo!.Value.Date.AddDays(1))
                    .GroupBy(x => x.ActualEndDate!.Value.Date)
                    .OrderBy(x => x.Key)
                    .Select(x => new ChartPointModel { Label = x.Key.ToString("dd.MM"), Value = x.Count() })
                    .ToListAsync()
            };
        }

        public async Task<byte[]> ExportAsync(string report, ReportFilterModel filter)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Справка");

            switch ((report ?? string.Empty).ToLowerInvariant())
            {
                case "sales":
                    WriteRows(ws, (await GetSalesAsync(filter)).Rows, new[] { "Дата", "Документ", "Оператор", "Редове", "Количество", "Междинна сума", "Отстъпка", "Общо", "Плащане", "Статус" },
                        r => new object?[] { r.Date, r.DocumentNumber, r.Operator, r.Items, r.Quantity, r.Subtotal, r.Discount, r.Total, r.Payment, r.Status });
                    break;
                case "salesbyproduct":
                    filter.PageSize = 10000;
                    WriteRows(ws, (await GetSalesByProductAsync(filter)).Rows, new[] { "SKU", "Артикул", "Вариант", "Количество", "Брутно", "Отстъпка", "Нетно", "Документи" },
                        r => new object?[] { r.SKU, r.Product, r.Variant, r.QuantitySold, r.GrossValue, r.Discount, r.NetValue, r.Documents });
                    break;
                case "warehouse":
                    filter.PageSize = 10000;
                    WriteRows(ws, (await GetWarehouseStockAsync(filter)).Rows, new[] { "Материал", "Име", "Категория", "Основен склад", "НЗП", "Минимум", "Статус" },
                        r => new object?[] { r.MaterialCode, r.MaterialName, r.Category, r.DefaultWarehouseQuantity, r.WipQuantity, r.MinimumStock, r.Status });
                    break;
                case "movements":
                    filter.PageSize = 10000;
                    WriteRows(ws, (await GetWarehouseMovementsAsync(filter)).Rows, new[] { "Дата", "Движение", "Артикул/материал", "Количество", "Източник", "Получател", "Документ", "Потребител" },
                        r => new object?[] { r.Date, r.Movement, r.Item, r.Quantity, r.Source, r.Destination, r.DocumentNumber, r.User });
                    break;
                case "production":
                    filter.PageSize = 10000;
                    WriteRows(ws, (await GetProductionAsync(filter)).Rows, new[] { "Поръчка", "Артикул", "Вариант", "Планирано", "Годно", "Брак", "Начало", "Край", "Статус", "Прогрес" },
                        r => new object?[] { r.OrderNumber, r.Product, r.Variant, r.PlannedQuantity, r.GoodQuantity, r.RejectedQuantity, r.Start, r.End, r.Status, r.ProgressPercent });
                    break;
                case "consumption":
                    filter.PageSize = 10000;
                    WriteRows(ws, (await GetMaterialConsumptionAsync(filter)).Rows, new[] { "Поръчка", "Артикул", "Материал", "Прехвърлено", "Потребено", "Върнато", "Остатък", "МЕ", "PMC" },
                        r => new object?[] { r.ProductionOrder, r.Product, r.Material, r.Transferred, r.Consumed, r.Returned, r.Remainder, r.Unit, r.PmcDocument });
                    break;
                case "barcodes":
                    filter.PageSize = 10000;
                    WriteRows(ws, (await GetBarcodesAsync(filter)).Rows, new[] { "SKU", "Артикул", "Вариант", "Баркод", "Тип", "Генериран", "Генериран от", "Последен печат", "Брой печат", "Наличност" },
                        r => new object?[] { r.SKU, r.Product, r.Variant, r.BarcodeValue, r.BarcodeType, r.GeneratedOn, r.GeneratedBy, r.LastPrintedOn, r.PrintCount, r.CurrentStock });
                    break;
                default:
                    ws.Cells[1, 1].Value = "Непозната справка.";
                    break;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        private IQueryable<PosSale> ApplySalesFilter(IQueryable<PosSale> query, ReportFilterModel filter)
        {
            return query
                .Where(x => !filter.DateFrom.HasValue || x.CreatedOn >= filter.DateFrom.Value.Date)
                .Where(x => !filter.DateTo.HasValue || x.CreatedOn < filter.DateTo.Value.Date.AddDays(1))
                .Where(x => string.IsNullOrWhiteSpace(filter.DocumentNumber) || x.DocumentNumber.Contains(filter.DocumentNumber.Trim()))
                .Where(x => string.IsNullOrWhiteSpace(filter.Operator) || (x.CreatedByUserNameSnapshot != null && x.CreatedByUserNameSnapshot.Contains(filter.Operator.Trim())))
                .Where(x => !filter.PaymentMethod.HasValue || x.PaymentMethod == filter.PaymentMethod)
                .Where(x => !filter.PosStatus.HasValue || x.Status == filter.PosStatus)
                .Where(x => string.IsNullOrWhiteSpace(filter.Search) || x.Lines.Any(l => l.ProductSKU.Contains(filter.Search) || (l.ProductDescriptionSnapshot != null && l.ProductDescriptionSnapshot.Contains(filter.Search)) || l.SizeSnapshot.Contains(filter.Search)));
        }

        private static void Normalize(ReportFilterModel filter)
        {
            filter.Page = Math.Max(1, filter.Page);
            filter.PageSize = Math.Clamp(filter.PageSize, 10, 10000);
            filter.Search = filter.Search?.Trim();
            filter.DocumentNumber = filter.DocumentNumber?.Trim();
            filter.Operator = filter.Operator?.Trim();
        }

        private static void NormalizePeriod(ReportFilterModel filter)
        {
            Normalize(filter);
            filter.DateFrom ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            filter.DateTo ??= DateTime.Today;
        }

        private static IQueryable<T> Page<T>(IQueryable<T> query, ReportFilterModel filter)
        {
            return query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
        }

        private static ReportsIndexModel<T> Wrap<T>(ReportFilterModel filter, List<T> rows, int totalItems)
        {
            return new ReportsIndexModel<T> { Filter = filter, Rows = rows, TotalItems = totalItems };
        }

        private static ReportKpiModel Kpi(string label, string value, string cssClass)
        {
            return new ReportKpiModel { Label = label, Value = value, CssClass = cssClass };
        }

        private static string Money(decimal value)
        {
            return $"{value:N2} EUR";
        }

        private static string PaymentLabel(PaymentMethod method)
        {
            var member = typeof(PaymentMethod).GetMember(method.ToString()).FirstOrDefault();
            var description = member?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return description?.Description ?? method.ToString();
        }

        private static void WriteRows<T>(ExcelWorksheet ws, IReadOnlyList<T> rows, IReadOnlyList<string> headers, Func<T, object?[]> selector)
        {
            for (var i = 0; i < headers.Count; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
            }

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var values = selector(rows[rowIndex]);
                for (var col = 0; col < values.Length; col++)
                {
                    ws.Cells[rowIndex + 2, col + 1].Value = values[col];
                }
            }
        }
    }
}
