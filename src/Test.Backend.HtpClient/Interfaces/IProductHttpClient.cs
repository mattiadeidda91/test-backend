using Refit;
using Test.Backend.Abstractions.Models.Dto.Product;

namespace Test.Backend.HtpClient.Interfaces
{
    public interface IProductHttpClient
    {
        [Get("/api/v1/product")]
        Task<ApiResponse<List<ProductDto>>> GetProductAsync(CancellationToken cancellationToken = default);

        [Get("/api/v1/product/{id}")]
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
