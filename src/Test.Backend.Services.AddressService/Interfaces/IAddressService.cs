using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Repository;

namespace Test.Backend.Services.AddressService.Interfaces
{
    public interface IAddressService : IRepoService<Address>
    {
    }
}
