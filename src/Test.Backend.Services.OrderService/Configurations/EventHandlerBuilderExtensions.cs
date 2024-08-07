using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.Order;
using Test.Backend.Services.OrderService.Handlers;

namespace Test.Backend.Services.OrderService.Configurations
{
    public static class EventHandlerBuilderExtensions
    {
        public static IServiceCollection ConfigureEventHandlerMsgBus(this IServiceCollection services)
        {
            services.AddScoped<IEventHandler<CreateOrderStartedEvent>, CreateOrderStartedHandler>();
            services.AddScoped<IEventHandler<GetOrdersStartedEvent>, GetOrdersStartedHandler>();
            services.AddScoped<IEventHandler<GetOrderStartedEvent>, GetOrderStartedHandler>();
            services.AddScoped<IEventHandler<UpdateOrderStartedEvent>, UpdateOrderStartedHandler>();
            services.AddScoped<IEventHandler<DeleteOrderStartedEvent>, DeleteOrderStartedHandler>();

            return services;
        }
    }
}
