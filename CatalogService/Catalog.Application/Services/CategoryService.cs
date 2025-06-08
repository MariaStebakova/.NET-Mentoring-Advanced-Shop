using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return category ?? throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        public Task<IEnumerable<Category>> GetAllAsync() => _repository.GetAllAsync();

        public async Task<Category> AddAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name) || category.Name.Length > 50)
                throw new ArgumentException("Invalid category name.");

            await _repository.AddAsync(category);
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name) || category.Name.Length > 50)
                throw new ArgumentException("Invalid category name.");

            await _repository.UpdateAsync(category);
        }

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}