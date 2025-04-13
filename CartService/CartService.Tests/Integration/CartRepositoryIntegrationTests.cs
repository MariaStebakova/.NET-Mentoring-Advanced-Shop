using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using CartService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace CartService.Tests.Integration
{
    public class CartRepositoryIntegrationTests : IClassFixture<LiteDbTestFixture>
    {
        ICartRepository repo;

        public CartRepositoryIntegrationTests(LiteDbTestFixture fixture)
        {
            repo = new CartRepository($"Filename={fixture.DatabaseFilename};Connection=shared", new NullLogger<CartRepository>());
        }

        [Fact]
        public async Task SaveCartAsync_And_GetCartAsync_Should_Work()
        {
            var cartId = Guid.NewGuid().ToString();
            var cart = new Cart
            {
                Id = cartId,
                Items = new List<CartItem>
                {
                    new CartItem { Id = 1, Name = "Integration Item", Price = 99.99m, Quantity = 1 }
                }
            };

            await repo.SaveCartAsync(cart);

            var loadedCart = await repo.GetCartAsync(cartId);

            Assert.NotNull(loadedCart);
            Assert.Single(loadedCart.Items);
            Assert.Equal(1, loadedCart.Items[0].Id);
        }
    }
}
