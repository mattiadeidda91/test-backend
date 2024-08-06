using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Test.Backend.Abstractions.Configurations.JsonSerializer;
using Test.Backend.Abstractions.Configurations.Swagger;

namespace Test.Backend.Abstractions.Extensions
{
    public static class ControllerExtensions
    {
        public static IServiceCollection BuildControllerConfigurations(this IServiceCollection services)
        {
            services.AddControllers(config =>
            {
                config.Conventions.Add(new ProducesResponseTypeConvention());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
            });

            return services;
        }
    }
}
