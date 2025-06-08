using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Catalog.Tests.IntegrationTests
{
    public class ProductRepositoryIntegrationTests : IDisposable
    {
        private readonly CatalogDbContext _context;
        private readonly ProductRepository _repository;

        public ProductRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CatalogDbContext(options);
            _repository = new ProductRepository(_context);
        }

        [Fact]
        public async Task AddAndGetProduct_ShouldPersistAndReturn()
        {
            var category = new Category { Name = "Books" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Book A",
                Price = 15.00m,
                Currency = "USD",
                Amount = 3,
                CategoryId = category.Id
            };

            await _repository.AddAsync(product);

            var result = await _repository.GetAllAsync();
            Assert.Single(result);
            Assert.Equal("Book A", result.First().Name);
        }

        [Fact]
        public async Task AddUpdateDeleteProduct_FullCycle_Succeeds()
        {
            var category = new Category { Name = "Gadgets" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Gadget Pro",
                Price = 299.99m,
                Currency = "USD",
                Amount = 1,
                CategoryId = category.Id
            };

            await _repository.AddAsync(product);

            product.Name = "Gadget Ultra";
            await _repository.UpdateAsync(product);

            var updated = await _repository.GetByIdAsync(product.Id);
            Assert.Equal("Gadget Ultra", updated?.Name);

            await _repository.DeleteAsync(product.Id);
            var deleted = await _repository.GetByIdAsync(product.Id);
            Assert.Null(deleted);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}