using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderDbContext dataContext;

        public OrderService(IOrderDbContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public async Task<IEnumerable<Order>> GetAsync()
        {
            var products = await dataContext.GetData<Order>()
                .Include(o => o.OrderProducts)
                .OrderBy(p => p.OrderDate)
                .ToListAsync();

            //var products = await dataContext.GetData<Order>()
            //    .Include(u => u.User)
            //    .Include(a => a.DeliveryAddress)
            //    .Include(p => p.OrderProducts)
            //        .ThenInclude(p => p.Product)
            //            .ThenInclude(p => p.Category)
            //    .OrderBy(p => p.OrderDate).ToListAsync();

            return products;
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            var order = await dataContext.GetData<Order>()
                .Include(o => o.OrderProducts)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            //var address = await dataContext.GetData<Order>()
            //    .Include(u => u.User)
            //    .Include(a => a.DeliveryAddress)
            //    .Include(p => p.OrderProducts)
            //        .ThenInclude(p => p.Product)
            //            .ThenInclude(p => p.Category)
            //    .Where(o => o.Id == id)
            //    .FirstOrDefaultAsync();

            return order;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var order = await this.GetByIdAsync(id);

            if (order != null)
            {
                dataContext.Delete(order);
                await dataContext.SaveAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(Order order)
        {
            if (order != null)
            {
                dataContext.Delete(order);
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
