using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly;
using Refit;
using Polly.Timeout;
using Test.Backend.HtpClient.Configurations;

namespace Test.Backend.HtpClient.Extensions
{
    public static class HttpClientFactory
    {
        public static IServiceCollection AddHttpClientPollyRefit<TInterface>(
            this IServiceCollection services,
            IConfiguration configuration,
            string clientName,
            RefitSettings? refitSettings = null)
            where TInterface : class
        {
            var options = configuration.GetRequiredSection(nameof(HttpRefitPollyOptions)).Get<HttpRefitPollyOptions>();
            var clientOptions = options?.Clients[clientName];

            ArgumentNullException.ThrowIfNull(clientOptions);

            _ = services.AddHttpClient(clientName, client =>
            {
                client.BaseAddress = new Uri(clientOptions.BaseAddress);
                if (clientOptions.Timeout > 0)
                    client.Timeout = TimeSpan.FromSeconds(clientOptions.Timeout);
            })
            .AddTypedClient(c => RestService.For<TInterface>(c, refitSettings));

            return services;
        }

        public static IServiceCollection AddHttpClientPollyRefit<TInterface>(
        this IServiceCollection services,
        IConfiguration configuration,
        RefitSettings? refitSettings = null)
        where TInterface : class
        {
            var options = configuration.GetRequiredSection(nameof(HttpRefitPollyOptions)).Get<HttpRefitPollyOptions>();

            ArgumentNullException.ThrowIfNull(options);

            foreach (var clientOptions in options.Clients)
            {
                var name = clientOptions.Key;
                var httpClientOptions = clientOptions.Value;

                _ = services.AddHttpClient(name, client =>
                {
                    client.BaseAddress = new Uri(httpClientOptions.BaseAddress);
                    if (httpClientOptions.Timeout > 0)
                        client.Timeout = TimeSpan.FromSeconds(httpClientOptions.Timeout);
                })
                .AddTypedClient(c => RestService.For<TInterface>(c, refitSettings));
            }

            return services;
        }
    }
}
