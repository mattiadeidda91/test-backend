using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Backend.Abstractions.HealthChecks;

namespace Test.Backend.Abstractions.Extensions
{
    public static class HealthChecksBuilderExtension
    {
        public static IServiceCollection ConfigureHealthChecks<T>(this IServiceCollection services) where T : DbContext
        {
            services.AddHealthChecks()
                .AddCheck<DbContextHealthCheck<T>>("Database Health Check");

            return services;
        }
    }
}
