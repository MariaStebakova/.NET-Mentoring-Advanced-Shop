using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces;

public interface ICategoryService
{
    Task<Category> GetByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}