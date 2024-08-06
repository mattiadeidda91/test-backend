using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Backend.Abstractions.Extensions
{
    public static class OptionsBuilderExtensions
    {
        public static T? BuildOptions<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class
        {
            var jwtSection = configuration.GetSection(typeof(T).Name);
            services.Configure<T>(jwtSection);

            return jwtSection.Get<T>();
        }
    }
}
