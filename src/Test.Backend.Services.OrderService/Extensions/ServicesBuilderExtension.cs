using Test.Backend.Services.OrderService.DatabaseContext;
using Test.Backend.Services.OrderService.Interfaces;
using Test.Backend.Services.OrderService.Service;

namespace Test.Backend.Services.OrderService.Extensions
{
    public static class ServicesBuilderExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IOrderService, Test.Backend.Services.OrderService.Service.OrderService>();
            services.AddScoped<IOrderEntitesService, OrderEntitesService>();
            services.AddScoped<IOrderProductService, OrderProductService>();
            services.AddScoped<IOrderDbContext, OrderDbContext>();

            return services;
        }
    }
}
