using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Catalog.Tests.IntegrationTests
{
    public class CategoryRepositoryIntegrationTests : IDisposable
    {
        private readonly CatalogDbContext _context;
        private readonly CategoryRepository _repository;

        public CategoryRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CatalogDbContext(options);
            _repository = new CategoryRepository(_context);
        }

        [Fact]
        public async Task AddAndGetCategory_ShouldPersistAndReturn()
        {
            var category = new Category { Name = "Home" };
            await _repository.AddAsync(category);

            var result = await _repository.GetAllAsync();
            Assert.Single(result);
            Assert.Equal("Home", result.First().Name);
        }

        [Fact]
        public async Task AddUpdateDeleteCategory_FullCycle_Succeeds()
        {
            var category = new Category { Name = "Garden" };
            await _repository.AddAsync(category);

            category.Name = "Garden & Outdoors";
            await _repository.UpdateAsync(category);

            var updated = await _repository.GetByIdAsync(category.Id);
            Assert.Equal("Garden & Outdoors", updated?.Name);

            await _repository.DeleteAsync(category.Id);
            var deleted = await _repository.GetByIdAsync(category.Id);
            Assert.Null(deleted);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
