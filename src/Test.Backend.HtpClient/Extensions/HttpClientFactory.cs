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

        ///// <summary>
        ///// Create and configure a Refit HttpClient with Polly
        ///// </summary>
        ///// <typeparam name="TInterface">Refit interface to create a Typed Client</typeparam>
        ///// <typeparam name="THandler">HttpClientHandler</typeparam>
        ///// <param name="services">IServiceCollection</param>
        ///// <param name="configuration">IConfiguration</param>
        ///// <param name="refitSettings">RefitSettings</param>
        ///// <returns>IServiceCollection</returns>
        //public static IServiceCollection AddHttpClientPollyRefit<TInterface, THandler>(this IServiceCollection services,
        //    IConfiguration configuration,
        //    RefitSettings? refitSettings = null)
        //        where TInterface : class
        //        where THandler : HttpClientHandler
        //{
        //    var options = configuration.GetRequiredSection(nameof(HttpRefitPollyOptions)).Get<HttpRefitPollyOptions>();

        //    ArgumentNullException.ThrowIfNull(options);
        //    ArgumentNullException.ThrowIfNull(options.HttpClientOptions);

        //    foreach(var client in options.HttpClientOptions.Clients)
        //    {
        //        var httpClient = services.AddHttpClient(client.Name ?? string.Empty, httpClient =>
        //        {
        //            httpClient.BaseAddress = client.BaseAddress != null ? new Uri(client.BaseAddress) : null;
        //            if (client.Timeout > 0)
        //                httpClient.Timeout = TimeSpan.FromSeconds(client.Timeout);
        //        })
        //        .AddTypedClient(c => RestService.For<TInterface>(c, refitSettings))
        //        .ConfigurePrimaryHttpMessageHandler<THandler>();

        //        if (options.PollyOptions?.RetryPolicyEnable ?? false)
        //        {
        //            _ = httpClient.SetDefaultPollyPolicy(options);
        //        }
        //    }

        //    //var httpClient = services.AddHttpClient(options.HttpClientOptions.Name ?? string.Empty, client =>
        //    //{
        //    //    client.BaseAddress = options.HttpClientOptions.BaseAddress != null ? new Uri(options.HttpClientOptions.BaseAddress) : null;
        //    //    if (options.HttpClientOptions.Timeout > 0)
        //    //        client.Timeout = TimeSpan.FromSeconds(options.HttpClientOptions.Timeout);
        //    //})
        //    //.AddTypedClient(c => RestService.For<TInterface>(c, refitSettings))
        //    //.ConfigurePrimaryHttpMessageHandler<THandler>();

        //    //if (options.PollyOptions?.RetryPolicyEnable ?? false)
        //    //{
        //    //    _ = httpClient.SetDefaultPollyPolicy(options);
        //    //}

        //    return services;
        //}

        ///// <summary>
        ///// Create and configure a Refit HttpClient with Polly
        ///// </summary>
        ///// <typeparam name="TInterface">Refit interface to create a Typed Client</typeparam>
        ///// <param name="services"></param>
        ///// <param name="configuration">IConfiguration</param>
        ///// <param name="pollyPolicy">Polly policy, if null use the default policy</param>
        ///// <param name="refitSettings">RefitSettings</param>
        ///// <returns>IServiceCollection</returns>
        //public static IServiceCollection AddHttpClientPollyRefit<TInterface>(this IServiceCollection services,
        //    IConfiguration configuration,
        //    IAsyncPolicy<HttpResponseMessage>? pollyPolicy = null,
        //    RefitSettings? refitSettings = null)
        //        where TInterface : class
        //{
        //    var options = configuration.GetRequiredSection(nameof(HttpRefitPollyOptions)).Get<HttpRefitPollyOptions>();

        //    ArgumentNullException.ThrowIfNull(options);
        //    ArgumentNullException.ThrowIfNull(options.HttpClientOptions);

        //    var httpClient = services.AddHttpClient(options.HttpClientOptions.Name ?? string.Empty, client =>
        //    {
        //        client.BaseAddress = options.HttpClientOptions.BaseAddress != null ? new Uri(options.HttpClientOptions.BaseAddress) : null;
        //        if (options.HttpClientOptions.Timeout > 0)
        //            client.Timeout = TimeSpan.FromSeconds(options.HttpClientOptions.Timeout);
        //    })
        //    .AddTypedClient(c => RestService.For<TInterface>(c, refitSettings)); //Refit client

        //    //Polly configuration
        //    if (options.PollyOptions?.RetryPolicyEnable ?? false)
        //    {
        //        ArgumentNullException.ThrowIfNull(options.PollyOptions);

        //        if (pollyPolicy != null)
        //            httpClient.AddPolicyHandler(pollyPolicy);  //Consumer polly policy
        //        else
        //            httpClient.SetDefaultPollyPolicy(options); //Libary polly default policy
        //    }

        //    return services;
        //}

        ///// <summary>
        ///// Create and configure a Refit HttpClient with Polly
        ///// </summary>
        ///// <typeparam name="TInterface">Refit interface to create a Typed Client</typeparam>
        ///// <typeparam name="THandler">HttpClientHandler</typeparam>
        ///// <param name="services">IServiceCollection</param>
        ///// <param name="configuration">IConfiguration</param>
        ///// <param name="pollyPolicy">Polly policy, if null use the default policy</param>
        ///// <param name="refitSettings">RefitSettings</param>
        ///// <returns>IServiceCollection</returns>
        //public static IServiceCollection AddHttpClientPollyRefit<TInterface, THandler>(this IServiceCollection services,
        //    IConfiguration configuration,
        //    IAsyncPolicy<HttpResponseMessage>? pollyPolicy = null,
        //    RefitSettings? refitSettings = null)
        //        where TInterface : class
        //        where THandler : HttpClientHandler
        //{
        //    var options = configuration.GetRequiredSection(nameof(HttpRefitPollyOptions)).Get<HttpRefitPollyOptions>();

        //    ArgumentNullException.ThrowIfNull(options);
        //    ArgumentNullException.ThrowIfNull(options.HttpClientOptions);

        //    var httpClient = services.AddHttpClient(options.HttpClientOptions.Name ?? string.Empty, client =>
        //    {
        //        client.BaseAddress = options.HttpClientOptions.BaseAddress != null ? new Uri(options.HttpClientOptions.BaseAddress) : null;
        //        if (options.HttpClientOptions.Timeout > 0)
        //            client.Timeout = TimeSpan.FromSeconds(options.HttpClientOptions.Timeout);
        //    })
        //    .AddTypedClient(c => RestService.For<TInterface>(c, refitSettings)) //Refit client
        //    .ConfigurePrimaryHttpMessageHandler<THandler>(); //HttpClientHandler

        //    //Polly configuration
        //    if (options.PollyOptions?.RetryPolicyEnable ?? false)
        //    {
        //        ArgumentNullException.ThrowIfNull(options.PollyOptions);

        //        if (pollyPolicy != null)
        //            httpClient.AddPolicyHandler(pollyPolicy);  //Consumer polly policy
        //        else
        //            httpClient.SetDefaultPollyPolicy(options); //Libary polly default policy
        //    }

        //    return services;
        //}

        ///// <summary>
        ///// Set default library polly policy
        ///// </summary>
        ///// <param name="httpClient"></param>
        ///// <param name="options"></param>
        ///// <returns></returns>
        //private static IHttpClientBuilder SetDefaultPollyPolicy(this IHttpClientBuilder httpClient, HttpRefitPollyOptions options)
        //{
        //    var isOnlyGetMethod = options.PollyOptions?.Configuration?.isOnlyGetMethod ?? false;

        //    //LifeTime
        //    if (options.PollyOptions?.Configuration?.HandlerLifeTime > 0)
        //        httpClient = httpClient.SetHandlerLifetime(TimeSpan.FromMinutes(options.PollyOptions.Configuration.HandlerLifeTime));

        //    //Only GET
        //    if (isOnlyGetMethod)
        //        httpClient.AddPolicyHandler(request => request.Method == HttpMethod.Get ? GetRetryPolicy(options.PollyOptions) : Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>()); //Only for HttpMethod GET
        //    else
        //        httpClient.AddPolicyHandler(GetRetryPolicy(options.PollyOptions)); //For all HttpMethods

        //    //Timeout Policy
        //    if (options.HttpClientOptions!.Timeout > 0)
        //        httpClient.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(options.HttpClientOptions.Timeout)));

        //    return httpClient;
        //}

        ///// <summary>
        ///// Polly retry policy
        ///// </summary>
        ///// <param name="options"></param>
        ///// <param name="isOnlyGetMethod"></param>
        ///// <returns></returns>
        //private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(PollyOptions? options)
        //{
        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError() // Network failures (System.Net.Http.HttpRequestException), HTTP 5XX status codes (server errors), HTTP 408 status code (request timeout)
        //        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        //        .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
        //        .WaitAndRetryAsync(options?.Configuration?.MaxRetry ?? 3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(options?.Configuration?.RetryDelay ?? 2, retryAttempt)));
        //}
    }
}
