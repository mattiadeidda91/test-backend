using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Events.User;
using Test.Backend.Services.UserService.Handlers;

namespace Test.Backend.Services.UserService.Configurations
{
    public static class EventHandlerBuilderExtensions
    {
        public static IServiceCollection ConfigureEventHandlerMsgBus(this IServiceCollection services)
        {
            services.AddScoped<IEventHandler<CreateUserStartedEvent>, CreateUserStartedHandler>();
            services.AddScoped<IEventHandler<GetUsersStartedEvent>, GetUsersStartedHandler>();
            services.AddScoped<IEventHandler<GetUserStartedEvent>, GetUserStartedHandler>();
            services.AddScoped<IEventHandler<UpdateUserStartedEvent>, UpdateUserStartedHandler>();
            services.AddScoped<IEventHandler<DeleteUserStartedEvent>, DeleteUserStartedHandler>();

            return services;
        }
    }
}
