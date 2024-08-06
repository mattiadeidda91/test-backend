using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Database.Repository;

namespace Test.Backend.AddressService.Interfaces
{
    public interface IAddressService : IRepoService<Address>
    {
    }
}
