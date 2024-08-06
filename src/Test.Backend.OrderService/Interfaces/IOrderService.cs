using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Repository;

namespace Test.Backend.OrderService.Interfaces
{
    public interface IOrderService : IRepoService<Order>
    {
    }
}
