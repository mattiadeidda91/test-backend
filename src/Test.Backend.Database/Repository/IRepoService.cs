using Test.Backend.Abstractions.Models.Entities.Common;

namespace Test.Backend.Database.Repository
{
    public interface IRepoService<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteAsync(T entity);
        Task UpdateAsync(T entity);
        Task SaveAsync(T entity);
    }
}
