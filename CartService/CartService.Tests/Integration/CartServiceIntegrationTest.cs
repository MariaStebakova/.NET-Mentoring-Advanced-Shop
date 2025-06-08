using CartService.Domain.Entities;
using CartService.Infrastructure.Repositories;

using Microsoft.Extensions.Logging.Abstractions;

using AppCartService = CartService.Application.Services.CartService;

namespace CartService.Tests.Integration
{
    public class CartServiceIntegrationTests : IClassFixture<LiteDbTestFixture>
    {
        private readonly AppCartService _cartService;

        public CartServiceIntegrationTests(LiteDbTestFixture fixture)
        {
            var repository = new CartRepository($"Filename={fixture.DatabaseFilename};Connection=shared", new NullLogger<CartRepository>());
            _cartService = new AppCartService(repository, new NullLogger<AppCartService>());
        }

        [Fact]
        public async Task AddItemAsync_Should_Save_Item_To_Database()
        {
            string cartId = Guid.NewGuid().ToString();
            var item = new CartItem
            {
                Id = 101,
                Name = "Integration Test Item",
                Price = 50,
                Quantity = 2
            };

            await _cartService.AddItemAsync(cartId, item);

            var items = await _cartService.GetCartItemsAsync(cartId);

            Assert.Single(items);
            Assert.Equal(item.Id, items[0].Id);
            Assert.Equal(item.Quantity, items[0].Quantity);
        }

        [Fact]
        public async Task RemoveItemAsync_Should_Remove_Item_From_Database()
        {
            string cartId = Guid.NewGuid().ToString();
            var item = new CartItem
            {
                Id = 202,
                Name = "Remove Test Item",
                Price = 70,
                Quantity = 1
            };

            await _cartService.AddItemAsync(cartId, item);

            await _cartService.RemoveItemAsync(cartId, item.Id);

            var items = await _cartService.GetCartItemsAsync(cartId);

            Assert.Empty(items);
        }

        [Fact]
        public async Task AddSameItemTwice_ThenRemoveOnce_ItemShouldRemainWithUpdatedQuantity()
        {
            string cartId = Guid.NewGuid().ToString();
            var item = new CartItem
            {
                Id = 303,
                Name = "Complex Test Item",
                Price = 99.99m,
                Quantity = 1
            };

            await _cartService.AddItemAsync(cartId, item);
            await _cartService.AddItemAsync(cartId, item);

            await _cartService.RemoveItemAsync(cartId, item.Id);

            var finalItems = await _cartService.GetCartItemsAsync(cartId);

            Assert.Single(finalItems);
            Assert.Equal(303, finalItems[0].Id);
            Assert.Equal(1, finalItems[0].Quantity);
        }
    }
}