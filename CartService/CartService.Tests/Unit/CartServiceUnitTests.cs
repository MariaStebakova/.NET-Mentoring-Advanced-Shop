using Moq;
using AppCartService = CartService.Application.Services.CartService;
using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CartService.Tests.Unit
{
    public class CartServiceUnitTests
    {
        private readonly Mock<ICartRepository> _repoMock;
        private readonly Mock<ILogger<AppCartService>> _loggerMock;
        private readonly AppCartService _service;

        public CartServiceUnitTests()
        {
            _repoMock = new Mock<ICartRepository>();
            _loggerMock = new Mock<ILogger<AppCartService>>();
            _service = new AppCartService(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task AddItemAsync_Should_Add_New_Item_When_Not_Existing()
        {
            _repoMock.Setup(r => r.GetCartAsync(It.IsAny<string>())).ReturnsAsync((Cart?)null);

            var newItem = new CartItem { Id = 1, Name = "Test Item", Price = 10, Quantity = 1 };
            string cartId = "testcart";

            await _service.AddItemAsync(cartId, newItem);

            _repoMock.Verify(r => r.SaveCartAsync(It.Is<Cart>(c => c.Items.Count == 1 && c.Items[0].Id == 1)), Times.Once);
        }

        [Fact]
        public async Task AddItemAsync_Should_Increment_Quantity_If_Item_Already_Exists()
        {
            var existingCart = new Cart
            {
                Id = "testcart",
                Items = new List<CartItem> { new CartItem { Id = 1, Name = "Existing Item", Price = 10, Quantity = 1 } }
            };

            _repoMock.Setup(r => r.GetCartAsync("testcart")).ReturnsAsync(existingCart);

            var newItem = new CartItem { Id = 1, Name = "Existing Item", Price = 10, Quantity = 2 };

            await _service.AddItemAsync("testcart", newItem);

            _repoMock.Verify(r => r.SaveCartAsync(It.Is<Cart>(c => c.Items[0].Quantity == 3)), Times.Once);
        }

        [Fact]
        public async Task RemoveItemAsync_Should_Remove_Item()
        {
            var existingCart = new Cart
            {
                Id = "testcart",
                Items = new List<CartItem> { new CartItem { Id = 1, Name = "Item To Remove", Price = 10, Quantity = 1 } }
            };

            _repoMock.Setup(r => r.GetCartAsync("testcart")).ReturnsAsync(existingCart);

            await _service.RemoveItemAsync("testcart", 1);

            _repoMock.Verify(r => r.SaveCartAsync(It.Is<Cart>(c => c.Items.Count == 0)), Times.Once);
        }
    }
}
