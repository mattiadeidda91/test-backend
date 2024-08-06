using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Test.Backend.Abstractions.Interfaces;
using Test.Backend.Abstractions.Models.Entities;
using Test.Backend.ProductService.Interfaces;

namespace Test.Backend.ProductService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IApplicationDbContext dataContext;
        public CategoryService(IApplicationDbContext dataContext)
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
            var user = await this.GetByIdAsync(id);

            if (user != null)
            {
                dataContext.Delete(user);
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
