using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Test.Backend.Abstractions.Configurations.Swagger;

namespace Test.Backend.Abstractions.Extensions
{
    public static class SwaggerBuilderExtension
    {
        public static IServiceCollection SwaggerBuild(this IServiceCollection services, List<string> apiVersions)
        {
            services.AddSwaggerGen(options =>
            {
                foreach (var version in apiVersions)
                {
                    options.SwaggerDoc(version, new OpenApiInfo { Title = Assembly.GetEntryAssembly()?.GetName().Name + " " + version.ToUpper(), Version = version });
                }

                options.EnableAnnotations();
                options.OperationFilter<OperationFilter>();
            });

            return services;
        }
    }
}
