using Test.Backend.HtpClient.Extensions;
using Test.Backend.HtpClient.Interfaces;

namespace Test.Backend.Services.OrderService.Extensions
{
    public static class RefitClientBuilderExtension
    {
        public static IServiceCollection ConfigureRefitClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClientPollyRefit<IOrderHttpClient>(configuration, "OrderClient", null);
            services.AddHttpClientPollyRefit<IUserHttpClient>(configuration, "UserClient", null);
            services.AddHttpClientPollyRefit<IProductHttpClient>(configuration, "ProductClient", null);
            services.AddHttpClientPollyRefit<IAddressHttpClient>(configuration, "AddressClient", null);

            return services;
        }
    }
}
