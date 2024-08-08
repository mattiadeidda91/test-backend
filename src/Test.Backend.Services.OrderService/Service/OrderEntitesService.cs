using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.User;
using Test.Backend.HtpClient.Interfaces;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Service
{
    public class OrderEntitesService : IOrderEntitesService
    {
        private readonly IUserHttpClient userHttpClient;
        private readonly IProductHttpClient productHttpClient;
        private readonly IAddressHttpClient addressHttpClient;

        public OrderEntitesService(IUserHttpClient userHttpClient, IProductHttpClient productHttpClient, IAddressHttpClient addressHttpClient)
        {
            this.productHttpClient = productHttpClient;
            this.addressHttpClient = addressHttpClient;
            this.userHttpClient = userHttpClient;
        }

        public async Task<(bool, UserDto?, AddressDto?, List<ProductDto>?)> CheckAndGetExistingEntities(Guid userId, Guid addressId, List<Guid> productsId)
        {
            List<ProductDto>? productsDto = null;

            var userDB = await userHttpClient.GetUserByIdAsync(userId);

            if (userDB.Content == null) return (false, null, null, null);

            var addressDb = await addressHttpClient.GetAddressByIdAsync(addressId);

            if (addressDb.Content == null) return (false, null, null, null);

            if (!productsId.Any()) return (false, null, null, null);

            foreach (var productId in productsId)
            {
                var productDb = await productHttpClient.GetProductByIdAsync(productId);

                if (productDb.Content == null) return (false, null, null, null);

                productsDto ??= new List<ProductDto>();

                productsDto.Add(productDb.Content);
            }

            return (true, userDB.Content, addressDb.Content, productsDto);
        }
    }
}
