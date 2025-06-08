using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using CartService.Domain.Entities;
using CartService.Tests.ApiTests.Helpers;

namespace CartService.Tests.ApiTests
{
    public class CartControllerV1IntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _testDbPath;

        public CartControllerV1IntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            var token = AuthenticationHelper.GetAccessTokenAsync().GetAwaiter().GetResult();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _testDbPath = factory.TestDBFileName;
        }

        [Fact]
        public async Task AddAndGetCart_ReturnsCartWithItems()
        {
            var cartId = "v1-cart";
            var item = new CartItem
            {
                Id = 301,
                Name = "V1 Test Item",
                Price = 7.99m,
                Quantity = 1
            };

            var postResponse = await _client.PostAsJsonAsync($"/api/v1/carts/{cartId}/items", item);
            postResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/v1/carts/{cartId}");
            getResponse.EnsureSuccessStatusCode();
            var cart = await getResponse.Content.ReadFromJsonAsync<Cart>();

            Assert.NotNull(cart);
            Assert.Equal(cartId, cart!.Id);
            Assert.Single(cart.Items);
            Assert.Equal(301, cart.Items[0].Id);
        }

        [Fact]
        public async Task RemoveCartItem_UpdatesCartCorrectly()
        {
            var cartId = "v1-remove";
            var item = new CartItem
            {
                Id = 302,
                Name = "To Be Removed",
                Price = 12.50m,
                Quantity = 3
            };

            await _client.PostAsJsonAsync($"/api/v1/carts/{cartId}/items", item);
            await _client.PostAsJsonAsync($"/api/v1/carts/{cartId}/items", item); // total = 6

            var deleteResponse = await _client.DeleteAsync($"/api/v1/carts/{cartId}/items/{item.Id}");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/v1/carts/{cartId}");
            var cart = await getResponse.Content.ReadFromJsonAsync<Cart>();

            Assert.NotNull(cart);
            var updatedItem = cart!.Items.FirstOrDefault(i => i.Id == item.Id);
            Assert.NotNull(updatedItem);
            Assert.Equal(5, updatedItem!.Quantity);
        }

        [Fact]
        public async Task RemoveItem_FromNonExistentCart_ReturnsNotFound()
        {
            var cartId = "nonexistent-cart";
            var itemId = 999;

            var deleteResponse = await _client.DeleteAsync($"/api/v1/carts/{cartId}/items/{itemId}");

            Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        public void Dispose()
        {
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
        }
    }
}