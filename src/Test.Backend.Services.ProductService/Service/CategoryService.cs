using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.Services.ProductService.Interfaces;

namespace Test.Backend.Services.ProductService.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly IProductDbContext dataContext;
        public CategoryService(IProductDbContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public async Task<IEnumerable<Category>> GetAsync()
        {
            var categories = await dataContext.GetData<Category>()
                .Include(p => p.Products)
                .OrderBy(p => p.Name).ToListAsync();

            return categories;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            var address = await dataContext.GetData<Category>()
                .Include(p => p.Products)
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            return address;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await GetByIdAsync(id);

            if (category != null)
            {
                dataContext.Delete(category);
                await dataContext.SaveAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(Category category)
        {
            if (category != null)
            {
                dataContext.Delete(category);
                await dataContext.SaveAsync();

                return true;
            }

            return false;
        }

        public async Task UpdateAsync(Category category)
        {
            dataContext.Update(category);
            await dataContext.SaveAsync();
        }

        public async Task SaveAsync(Category category)
        {
            dataContext.Insert(category);
            await dataContext.SaveAsync();
        }
    }
}
