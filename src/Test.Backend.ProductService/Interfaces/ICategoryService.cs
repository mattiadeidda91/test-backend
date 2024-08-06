using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Repository;

namespace Test.Backend.ProductService.Interfaces
{
    public interface ICategoryService : IRepoService<Category>
    {
    }
}
