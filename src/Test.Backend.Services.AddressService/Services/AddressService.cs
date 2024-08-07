using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.AddressService.Interfaces;

namespace Test.Backend.Services.AddressService.Services;

public class AddressService : IAddressService
{
    private readonly IAddressDbContext dataContext;

    public AddressService(IAddressDbContext dataContext)
    {
        this.dataContext = dataContext;
    }
    public async Task<IEnumerable<Address>> GetAsync()
    {
        //var address = await dataContext.GetData<Address>()
        //    .Include(a => a.Orders)
        //      .ThenInclude(o => o.OrderProducts)
        //        .ThenInclude(o => o.Product)
        //          .ThenInclude(p => p.Category)
        //    .Include(a => a.Orders)
        //        .ThenInclude(o => o.User)
        //    .OrderBy(p => p.InsertDate)
        //    .ToListAsync();

        var address = await dataContext.GetData<Address>()
                .OrderBy(p => p.InsertDate)
                .ToListAsync();

        return address;
    }

    public async Task<Address?> GetByIdAsync(Guid id)
    {
        //var address = await dataContext.GetData<Address>()
        //    .Include(a => a.Orders)
        //      .ThenInclude(o => o.OrderProducts)
        //        .ThenInclude(o => o.Product)
        //          .ThenInclude(p => p.Category)
        //    .Include(a => a.Orders)
        //        .ThenInclude(o => o.User)
        //    .Where(a => a.Id == id)
        //    .FirstOrDefaultAsync();

        var address = await dataContext.GetData<Address>()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();

        return address;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var address = await this.GetByIdAsync(id);

        if (address != null)
        {
            dataContext.Delete(address);
            await dataContext.SaveAsync();

            return true;
        }

        return false;
    }

    public async Task<bool> DeleteAsync(Address address)
    {
        if (address != null)
        {
            dataContext.Delete(address);
            await dataContext.SaveAsync();

            return true;
        }

        return false;
    }

    public async Task UpdateAsync(Address address)
    {
        dataContext.Update(address);
        await dataContext.SaveAsync();
    }

    public async Task SaveAsync(Address address)
    {
        dataContext.Insert(address);
        await dataContext.SaveAsync();
    }
}
