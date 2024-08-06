using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Repository;

namespace Test.Backend.Services.UserService.Interfaces
{
    public interface IUserService : IRepoService<User> 
    {
        
    }
}
