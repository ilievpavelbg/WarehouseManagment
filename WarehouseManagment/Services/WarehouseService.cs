using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IRepository _repository;

        public WarehouseService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Warehouse>> GetAllWarehousesAsync()
        {
            return await _repository.All<Warehouse>()
                .Include(x => x.Zones)
                .Include(x => x.Locations)
                .OrderBy(x => x.Code)
                .ToListAsync();
        }

        public async Task<Warehouse> GetWarehouseByIdAsync(int id)
        {
            var warehouse = await _repository.All<Warehouse>()
                .Include(x => x.Zones)
                .Include(x => x.Locations)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (warehouse == null)
            {
                throw new ArgumentNullException(nameof(warehouse));
            }

            return warehouse;
        }

        public async Task CreateWarehouseAsync(WarehouseModel model)
        {
            var code = NormalizeCode(model.Code);
            var warehouseExists = await _repository.AllReadonly<Warehouse>()
                .AnyAsync(x => x.Code == code);

            if (warehouseExists)
            {
                throw new InvalidOperationException("Warehouse with this code already exists.");
            }

            var warehouse = new Warehouse
            {
                Code = code,
                Name = model.Name.Trim(),
                Description = model.Description,
                IsActive = model.IsActive
            };

            await _repository.AddAsync(warehouse);
            await _repository.SaveChangesAsync();
        }

        public async Task CreateZoneAsync(WarehouseZoneModel model)
        {
            var warehouse = await _repository.GetByIdAsync<Warehouse>(model.WarehouseId);

            if (warehouse == null)
            {
                throw new ArgumentNullException(nameof(warehouse));
            }

            var code = NormalizeCode(model.Code);
            var zoneExists = await _repository.AllReadonly<WarehouseZone>()
                .AnyAsync(x => x.WarehouseId == model.WarehouseId && x.Code == code);

            if (zoneExists)
            {
                throw new InvalidOperationException("Zone with this code already exists in the selected warehouse.");
            }

            var zone = new WarehouseZone
            {
                Code = code,
                Name = model.Name.Trim(),
                Description = model.Description,
                WarehouseId = model.WarehouseId,
                IsActive = model.IsActive
            };

            await _repository.AddAsync(zone);
            await _repository.SaveChangesAsync();
        }

        public async Task CreateLocationAsync(WarehouseLocationModel model)
        {
            var warehouse = await _repository.GetByIdAsync<Warehouse>(model.WarehouseId);

            if (warehouse == null)
            {
                throw new ArgumentNullException(nameof(warehouse));
            }

            if (model.WarehouseZoneId.HasValue)
            {
                var zone = await _repository.GetByIdAsync<WarehouseZone>(model.WarehouseZoneId.Value);

                if (zone == null || zone.WarehouseId != model.WarehouseId)
                {
                    throw new InvalidOperationException("Selected zone does not belong to the selected warehouse.");
                }
            }

            var code = NormalizeCode(model.Code);
            var locationExists = await _repository.AllReadonly<WarehouseLocation>()
                .AnyAsync(x => x.WarehouseId == model.WarehouseId && x.Code == code);

            if (locationExists)
            {
                throw new InvalidOperationException("Location with this code already exists in the selected warehouse.");
            }

            var location = new WarehouseLocation
            {
                Code = code,
                Name = model.Name.Trim(),
                Description = model.Description,
                WarehouseId = model.WarehouseId,
                WarehouseZoneId = model.WarehouseZoneId,
                IsActive = model.IsActive
            };

            await _repository.AddAsync(location);
            await _repository.SaveChangesAsync();
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpper();
        }
    }
}
