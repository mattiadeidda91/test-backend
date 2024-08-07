using Refit;
using Test.Backend.Abstractions.Models.Dto.Order;

namespace Test.Backend.HtpClient.Interfaces
{
    public interface IOrderHttpClient
    {
        [Get("/api/v1/order")]
        Task<ApiResponse<List<OrderDto>>> GetOrdersAsync(CancellationToken cancellationToken = default);

        [Get("/api/v1/order/{id}")]
        Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
