using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.OrderService.Interfaces;

namespace Test.Backend.OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IApplicationDbContext dataContext;

        public OrderService(IApplicationDbContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public async Task<IEnumerable<Order>> GetAsync()
        {
            var products = await dataContext.GetData<Order>()
                .Include(u => u.User)
                .Include(a => a.DeliveryAddress)
                .Include(p => p.OrderProducts)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(p => p.Category)
                .OrderBy(p => p.OrderDate).ToListAsync();

            return products;
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            var address = await dataContext.GetData<Order>()
                .Include(u => u.User)
                .Include(a => a.DeliveryAddress)
                .Include(p => p.OrderProducts)
                    .ThenInclude(p => p.Product)
                        .ThenInclude(p => p.Category)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            return address;
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

        public async Task UpdateAsync(Order order)
        {
            dataContext.Update(order);
            await dataContext.SaveAsync();
        }

        public async Task SaveAsync(Order order)
        {
            dataContext.Insert(order);
            await dataContext.SaveAsync();
        }
    }
}
