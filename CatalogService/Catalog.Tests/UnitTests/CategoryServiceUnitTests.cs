using Catalog.Application.Interfaces;
using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Moq;
using Xunit;

namespace Catalog.Tests.UnitTests
{
    public class CategoryServiceUnitTests
    {
        private readonly Mock<ICategoryRepository> _mockRepo = new();
        private readonly CategoryService _service;

        public CategoryServiceUnitTests()
        {
            _service = new CategoryService(_mockRepo.Object);
        }

        [Fact]
        public async Task AddAsync_ValidCategory_CallsRepository()
        {
            var category = new Category { Name = "Electronics" };

            var result = await _service.AddAsync(category);

            _mockRepo.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == "Electronics")), Times.Once);
            Assert.Equal("Electronics", result.Name);
        }

        [Fact]
        public async Task AddAsync_InvalidCategory_ThrowsException()
        {
            var category = new Category { Name = "" };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(category));
        }

        [Fact]
        public async Task GetByIdAsync_CategoryExists_ReturnsCategory()
        {
            var expected = new Category { Id = 1, Name = "Tools" };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expected);

            var result = await _service.GetByIdAsync(1);

            Assert.Equal("Tools", result.Name);
        }

        [Fact]
        public async Task DeleteAsync_CallsRepository()
        {
            await _service.DeleteAsync(99);

            _mockRepo.Verify(r => r.DeleteAsync(99), Times.Once);
        }

    }
}
