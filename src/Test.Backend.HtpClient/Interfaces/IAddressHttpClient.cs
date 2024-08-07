using Refit;
using Test.Backend.Abstractions.Models.Dto.Address;

namespace Test.Backend.HtpClient.Interfaces
{
    public interface IAddressHttpClient
    {
        [Get("/api/v1/address")]
        Task<ApiResponse<List<AddressDto>>> GetAddressAsync(CancellationToken cancellationToken = default);

        [Get("/api/v1/address/{id}")]
        Task<ApiResponse<AddressDto>> GetAddressByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
