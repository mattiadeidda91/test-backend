using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.ProductService.Interfaces;

namespace Test.Backend.ProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly IApplicationDbContext dataContext;
        public ProductService(IApplicationDbContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public async Task<IEnumerable<Product>> GetAsync()
        {
            var products = await dataContext.GetData<Product>()
                .Include(p => p.Category)
                .Include(p => p.OrderProducts)
                    .ThenInclude(o => o.Order)
                .OrderBy(p => p.InsertDate)
                .ToListAsync();

            return products;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            var product = await dataContext.GetData<Product>()
                .Include(p => p.Category)
                .Include(p => p.OrderProducts)
                    .ThenInclude(o => o.Order)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            return product;
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

        public async Task UpdateAsync(Product product)
        {
            dataContext.Update(product);
            await dataContext.SaveAsync();
        }

        public async Task SaveAsync(Product product)
        {
            dataContext.Insert(product);
            await dataContext.SaveAsync();
        }
    }
}
