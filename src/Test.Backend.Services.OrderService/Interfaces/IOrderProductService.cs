using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Repository;

namespace Test.Backend.Services.OrderService.Interfaces
{
    public interface IOrderProductService : IRepoService<OrderProduct>
    {
    }
}
