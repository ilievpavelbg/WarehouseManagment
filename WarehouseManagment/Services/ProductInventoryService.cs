using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class ProductInventoryService : IProductInventoryService
    {
        private readonly IRepository _repository;

        public ProductInventoryService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task<List<ProductInventory>> GetAllProductInventoryByProductIdAsync(int productId)
        {
            var productInventory = await _repository.All<ProductInventory>().Where(x => x.ProductId == productId).ToListAsync();

            return productInventory;
        }
    }
}
