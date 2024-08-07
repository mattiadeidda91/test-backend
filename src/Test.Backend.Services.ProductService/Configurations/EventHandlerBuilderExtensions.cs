using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.Category;
using Test.Backend.Abstractions.Models.Events.Product;
using Test.Backend.Services.ProductService.Handlers.Categories;
using Test.Backend.Services.ProductService.Handlers.Products;

namespace Test.Backend.Services.ProductService.Configurations
{
    public static class EventHandlerBuilderExtensions
    {
        public static IServiceCollection ConfigureEventHandlerMsgBus(this IServiceCollection services)
        {
            services.AddScoped<IEventHandler<CreateProductStartedEvent>, CreateProductStartedHandler>();
            services.AddScoped<IEventHandler<GetProductsStartedEvent>, GetProductsStartedHandler>();
            services.AddScoped<IEventHandler<GetProductStartedEvent>, GetProductStartedHandler>();
            services.AddScoped<IEventHandler<UpdateProductStartedEvent>, UpdateProductStartedHandler>();
            services.AddScoped<IEventHandler<DeleteProductStartedEvent>, DeleteProductStartedHandler>();

            services.AddScoped<IEventHandler<CreateCategoryStartedEvent>, CreateCategoryStartedHandler>();
            services.AddScoped<IEventHandler<GetCategoriesStartedEvent>, GetCategoriesStartedHandler>();
            services.AddScoped<IEventHandler<GetCategoryStartedEvent>, GetCategoryStartedHandler>();
            services.AddScoped<IEventHandler<UpdateCategoryStartedEvent>, UpdateCategoryStartedHandler>();
            services.AddScoped<IEventHandler<DeleteCategoryStartedEvent>, DeleteCategoryStartedHandler>();

            return services;
        }
    }
}
