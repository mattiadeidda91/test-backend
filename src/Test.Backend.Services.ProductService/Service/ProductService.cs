using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.ProductService.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductDbContext dataContext;
        public ProductService(IProductDbContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public async Task<IEnumerable<Product>> GetAsync()
        {
            var products = await dataContext.GetData<Product>()
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            //var products = await dataContext.GetData<Product>()
            //    .Include(p => p.Category)
            //    .Include(p => p.OrderProducts)
            //        .ThenInclude(o => o.Order)
            //    .OrderBy(p => p.InsertDate)
            //    .ToListAsync();

            return products;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            var product = await dataContext.GetData<Product>()
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            //var product = await dataContext.GetData<Product>()
            //    .Include(p => p.Category)
            //    .Include(p => p.OrderProducts)
            //        .ThenInclude(o => o.Order)
            //    .Where(p => p.Id == id)
            //    .FirstOrDefaultAsync();

            return product;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);

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
