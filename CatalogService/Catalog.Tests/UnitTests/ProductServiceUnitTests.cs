using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Catalog.Application.Interfaces;
using Moq;
using Xunit;

namespace Catalog.Tests.UnitTests
{
    public class ProductServiceUnitTests
    {
        private readonly Mock<IProductRepository> _mockRepo = new();
        private readonly ProductService _service;

        public ProductServiceUnitTests()
        {
            _service = new ProductService(_mockRepo.Object);
        }

        [Fact]
        public async Task AddAsync_ValidProduct_CallsRepository()
        {
            var product = new Product
            {
                Name = "Test Product",
                Price = 12.99m,
                Currency = "USD",
                Amount = 10,
                CategoryId = 1
            };

            var result = await _service.AddAsync(product);

            _mockRepo.Verify(r => r.AddAsync(It.Is<Product>(p => p.Name == "Test Product")), Times.Once);
            Assert.Equal("Test Product", result.Name);
        }

        [Fact]
        public async Task AddAsync_InvalidProduct_ThrowsException()
        {
            var product = new Product { Name = "", Price = -1, Currency = "", Amount = -5 };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(product));
        }

        [Fact]
        public async Task GetByIdAsync_ProductExists_ReturnsProduct()
        {
            var expected = new Product { Id = 1, Name = "Found Product" };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expected);

            var result = await _service.GetByIdAsync(1);

            Assert.Equal("Found Product", result.Name);
        }

        [Fact]
        public async Task DeleteAsync_CallsRepository()
        {
            await _service.DeleteAsync(42);

            _mockRepo.Verify(r => r.DeleteAsync(42), Times.Once);
        }

    }
}
