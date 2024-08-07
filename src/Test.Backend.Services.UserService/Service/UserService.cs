using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.UserService.Interfaces;

namespace Test.Backend.Services.UserService.Service
{
    public class UserService : IUserService
    {
        private readonly IUserDbContext userContext;

        public UserService(IUserDbContext userContext)
        {
            this.userContext = userContext;
        }

        public async Task<IEnumerable<User>> GetAsync()
        {
            //var users = await dataContext.GetData<User>()
            //    .Include(u => u.Orders)
            //        .ThenInclude(o => o.OrderProducts)
            //          .ThenInclude(p => p.Product)
            //            .ThenInclude(p => p.Category)
            //    .OrderBy(p => p.LastName)
            //    .ToListAsync();

            var users = await userContext.GetData<User>()
                .OrderBy(p => p.LastName)
                .ToListAsync();

            return users;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            //var user = await dataContext.GetData<User>()
            //    .Include(u => u.Orders)
            //        .ThenInclude(o => o.OrderProducts)
            //          .ThenInclude(p => p.Product)
            //            .ThenInclude(c => c.Category)
            //    .Where(u => u.Id == id)
            //    .FirstOrDefaultAsync();

            var user = await userContext.GetData<User>()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await this.GetByIdAsync(id);

            if (user != null)
            {
                userContext.Delete(user);
                await userContext.SaveAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            if (user != null)
            {
                userContext.Delete(user);
                await userContext.SaveAsync();

                return true;
            }

            return false;
        }

        public async Task UpdateAsync(User user)
        {
            userContext.Update(user);
            await userContext.SaveAsync();
        }

        public async Task SaveAsync(User user)
        {
            userContext.Insert(user);
            await userContext.SaveAsync();
        }
    }
}
