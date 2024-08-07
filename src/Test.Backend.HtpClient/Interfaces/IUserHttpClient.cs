using Refit;
using Test.Backend.Abstractions.Models.Dto.User;

namespace Test.Backend.HtpClient.Interfaces
{
    public interface IUserHttpClient
    {
        [Get("/api/v1/user")]
        Task<ApiResponse<List<UserDto>>> GetUsersAsync(CancellationToken cancellationToken = default);

        [Get("/api/v1/user/{id}")]
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
