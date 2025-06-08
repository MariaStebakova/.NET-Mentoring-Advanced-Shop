using CartService.API.Controllers.V2;
using CartService.Application.Interfaces;
using CartService.Domain.Entities;

using Microsoft.AspNetCore.Mvc;

using Moq;

namespace CartService.Tests.Unit.ContollerTests
{
    public class CartControllerV2Tests
    {
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly CartController _controller;

        public CartControllerV2Tests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _controller = new CartController(_cartServiceMock.Object);
        }

        [Fact]
        public async Task GetItems_ReturnsOkResult_WithCartItems()
        {
            // Arrange
            var cartId = "abc123";
            var expectedItems = new List<CartItem> { new CartItem { Id = 1, Name = "Item", Price = 9.99M, Quantity = 1 } };
            _cartServiceMock.Setup(s => s.GetCartItemsAsync(cartId)).ReturnsAsync(expectedItems);

            // Act
            var result = await _controller.GetItems(cartId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<CartItem>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal(1, returnValue[0].Id);
        }

        [Fact]
        public async Task AddItem_CallsService_AndReturnsOk()
        {
            // Arrange
            var cartId = "cart456";
            var item = new CartItem { Id = 2, Name = "New", Price = 5, Quantity = 1 };

            // Act
            var result = await _controller.AddItem(cartId, item);

            // Assert
            _cartServiceMock.Verify(s => s.AddItemAsync(cartId, item), Times.Once);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task RemoveItem_CallsService_AndReturnsOk()
        {
            // Arrange
            var cartId = "cart789";
            var itemId = 3;

            // Act
            var result = await _controller.RemoveItem(cartId, itemId);

            // Assert
            _cartServiceMock.Verify(s => s.RemoveItemAsync(cartId, itemId), Times.Once);
            Assert.IsType<OkResult>(result);
        }
    }
}