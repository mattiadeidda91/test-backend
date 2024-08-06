using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Backend.Kafka.Interfaces;
using Test.Backend.Kafka.Options;
using Test.Backend.Kafka.Services;

namespace Test.Backend.Kafka.Configurations
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventBusService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KafkaOptions>(configuration.GetSection(nameof(KafkaOptions)));
            services.AddScoped<IEventBusService, EventBusService>();

            return services;
        }
    }
}
