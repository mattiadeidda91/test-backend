using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.UserService.Interfaces;

namespace Test.Backend.UserService.Services
{
    public class UserService : IUserService
    {
        private readonly IApplicationDbContext dataContext;
        public UserService(IApplicationDbContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<IEnumerable<User>> GetAsync()
        {
            return new List<User>();
            //var users = await dataContext.GetData<User>()
            //    .Include(u => u.Orders)
            //        .ThenInclude(o => o.OrderProducts)
            //          .ThenInclude(p => p.Product)
            //            .ThenInclude(p => p.Category)
            //    .OrderBy(p => p.LastName)
            //    .ToListAsync();

            //return users;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return new();
            //var user = await dataContext.GetData<User>()
            //    .Include(u => u.Orders)
            //        .ThenInclude(o => o.OrderProducts)
            //          .ThenInclude(p => p.Product)
            //            .ThenInclude(c => c.Category)
            //    .Where(u => u.Id == id)
            //    .FirstOrDefaultAsync();

            //return user;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await this.GetByIdAsync(id);

            if (user != null)
            {
                dataContext.Delete(user);
                await dataContext.SaveAsync();

                return true;
            }

            return false;
        }

        public async Task UpdateAsync(User address)
        {
            dataContext.Update(address);
            await dataContext.SaveAsync();
        }

        public async Task SaveAsync(User user)
        {
            dataContext.Insert(user);
            await dataContext.SaveAsync();
        }
    }
}
