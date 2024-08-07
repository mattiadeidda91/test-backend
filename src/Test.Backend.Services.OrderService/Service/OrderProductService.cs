using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.OrderService.Interfaces;

namespace Test.Backend.Services.OrderService.Service
{
    public class OrderProductService : IOrderProductService
    {
        private readonly IOrderDbContext dataContext;

        public OrderProductService(IOrderDbContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public async Task<IEnumerable<OrderProduct>> GetAsync()
        {
            var orderProducts = await dataContext.GetData<OrderProduct>()
                .Include(o => o.Order)
                .OrderBy(p => p.InsertDate)
                .ToListAsync();

            return orderProducts;
        }

        public async Task<OrderProduct?> GetByIdAsync(Guid id)
        {
            var orderProduct = await dataContext.GetData<OrderProduct>()
                .Include(o => o.Order)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            return orderProduct;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var orderProduct = await this.GetByIdAsync(id);

            if (orderProduct != null)
            {
                dataContext.Delete(orderProduct);
                await dataContext.SaveAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(OrderProduct orderProduct)
        {
            if (orderProduct != null)
            {
                dataContext.Delete(orderProduct);
                await dataContext.SaveAsync();

                return true;
            }

            return false;
        }

        public async Task UpdateAsync(OrderProduct orderProduct)
        {
            dataContext.Update(orderProduct);
            await dataContext.SaveAsync();
        }

        public async Task SaveAsync(OrderProduct orderProduct)
        {
            dataContext.Insert(orderProduct);
            await dataContext.SaveAsync();
        }
    }
}
