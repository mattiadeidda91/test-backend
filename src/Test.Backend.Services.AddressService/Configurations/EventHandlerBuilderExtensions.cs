using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.Address;
using Test.Backend.Services.AddressService.Handlers;
using Test.Backend.Services.UserService.Handlers;

namespace Test.Backend.Services.AddressService.Configurations
{
    public static class EventHandlerBuilderExtensions
    {
        public static IServiceCollection ConfigureEventHandlerMsgBus(this IServiceCollection services)
        {
            services.AddScoped<IEventHandler<CreateAddressStartedEvent>, CreateAddressStartedHandler>();
            services.AddScoped<IEventHandler<GetAddressesStartedEvent>, GetAddressesStartedHandler>();
            services.AddScoped<IEventHandler<GetAddressStartedEvent>, GetAddressStartedHandler>();
            services.AddScoped<IEventHandler<UpdateAddressStartedEvent>, UpdateAddressStartedHandler>();
            services.AddScoped<IEventHandler<DeleteAddressStartedEvent>, DeleteAddressStartedHandler>();

            return services;
        }
    }
}
