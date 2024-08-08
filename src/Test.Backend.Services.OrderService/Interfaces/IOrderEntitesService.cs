using Test.Backend.Abstractions.Models.Dto.Address;
using Test.Backend.Abstractions.Models.Dto.Product;
using Test.Backend.Abstractions.Models.Dto.User;

namespace Test.Backend.Services.OrderService.Interfaces
{
    public interface IOrderEntitesService
    {
        Task<(bool, UserDto?, AddressDto?, List<ProductDto>?)> CheckAndGetExistingEntities(Guid userId, Guid addressId, List<Guid> productsId);
    }
}
