using Microsoft.EntityFrameworkCore;
using System;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Repository;
using WarehouseManagment.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WarehouseManagmentApiCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductInventoryService, ProductInventoryService>();

            return services;
        }

        public static IServiceCollection AddWarehouseManagmentDbContext(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection_localhost");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddScoped<IRepository, Repository>();

            return services;
        }

    }
}
