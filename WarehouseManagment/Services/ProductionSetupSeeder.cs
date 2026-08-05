using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Constants;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Services
{
    public class ProductionSetupSeeder : IProductionSetupSeeder
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductionSetupSeeder(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedAsync()
        {
            await SeedCostComponentsAsync();
            await SeedProductionOperationsAsync();
            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedCostComponentsAsync()
        {
            var seeds = new[]
            {
                new CostComponentSeed("FABRIC", "Плат", 10, true),
                new CostComponentSeed("SEWER_LABOR", "Шивач", 20, true),
                new CostComponentSeed("SEWER_INSURANCE", "Осигуровки шивач", 30, true),
                new CostComponentSeed("ADDITIONAL_MATERIALS", "Допълнителни материали", 40, true),
                new CostComponentSeed("FINISHING_OPERATION", "Довършителна операция", 50, true),
                new CostComponentSeed("FINISHING_INSURANCE", "Осигуровки довършителна", 60, true),
                new CostComponentSeed("TOTAL_DIRECT_COST", "Общ пряк разход", 70, true),
                new CostComponentSeed("ELECTRICITY", "Ток", 80, false),
                new CostComponentSeed("WATER", "Вода", 90, false),
                new CostComponentSeed("ADVERTISING", "Реклама", 100, false),
                new CostComponentSeed("COURIERS", "Куриери", 110, false),
                new CostComponentSeed("IT", "IT", 120, false),
                new CostComponentSeed("STORE_PAYROLL", "Заплати и осигуровки на магазина за двама служители", 130, false),
                new CostComponentSeed("MANAGER_PAYROLL", "Заплата и осигуровки на управител", 140, false),
                new CostComponentSeed("MACHINE_DEPRECIATION", "Амортизация на машини", 150, false),
                new CostComponentSeed("PATTERNS", "Кройки", 160, true),
                new CostComponentSeed("PACKAGING_LABELS", "Опаковка и етикети", 170, false)
            };

            foreach (var seed in seeds)
            {
                var component = await _dbContext.CostComponents.FirstOrDefaultAsync(x => x.Code == seed.Code);
                if (component == null)
                {
                    await _dbContext.CostComponents.AddAsync(new CostComponent
                    {
                        Code = seed.Code,
                        Name = seed.Name,
                        DisplayOrder = seed.DisplayOrder,
                        IsActive = true,
                        IsDirectCost = seed.IsDirectCost,
                        IsSystemCalculated = false
                    });
                    continue;
                }

                component.Name = seed.Name;
                component.DisplayOrder = seed.DisplayOrder;
                component.IsDirectCost = seed.IsDirectCost;
                component.IsSystemCalculated = false;
            }
        }

        private async Task SeedProductionOperationsAsync()
        {
            var seeds = new[]
            {
                new ProductionOperationSeed("CUTTING", "Кроене", 10, ApplicationRoles.Cutter),
                new ProductionOperationSeed("SEWING", "Шиене", 20, ApplicationRoles.Sewer),
                new ProductionOperationSeed("FINISHING", "Довършване", 30, ApplicationRoles.Finisher)
            };

            foreach (var seed in seeds)
            {
                var operation = await _dbContext.ProductionOperations.FirstOrDefaultAsync(x => x.Code == seed.Code);
                if (operation == null)
                {
                    await _dbContext.ProductionOperations.AddAsync(new ProductionOperation
                    {
                        Code = seed.Code,
                        Name = seed.Name,
                        DefaultSequence = seed.DefaultSequence,
                        RequiredRole = seed.RequiredRole,
                        IsActive = true
                    });
                    continue;
                }

                operation.Name = seed.Name;
                operation.DefaultSequence = seed.DefaultSequence;
                operation.RequiredRole = seed.RequiredRole;
            }
        }

        private record CostComponentSeed(string Code, string Name, int DisplayOrder, bool IsDirectCost);

        private record ProductionOperationSeed(string Code, string Name, int DefaultSequence, string RequiredRole);
    }
}
