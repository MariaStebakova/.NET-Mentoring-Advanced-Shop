using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using CartService.Domain.Entities;
using CartService.Tests.ApiTests.Helpers;

using LiteDB;

namespace CartService.Tests.ApiTests
{
    public class CartControllerV2IntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _testDbPath;

        public CartControllerV2IntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            var token = AuthenticationHelper.GetAccessTokenAsync().GetAwaiter().GetResult();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _testDbPath = factory.TestDBFileName;
        }

        [Fact]
        public async Task AddAndGetCartItem_ReturnsCorrectItem()
        {
            var cartId = "integration-cart";
            var item = new CartItem
            {
                Id = 101,
                Name = "Integration Item",
                Price = 19.99m,
                Quantity = 2
            };

            var postResponse = await _client.PostAsJsonAsync($"/api/v2/carts/{cartId}/items", item);
            postResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/v2/carts/{cartId}/items");
            getResponse.EnsureSuccessStatusCode();
            var items = await getResponse.Content.ReadFromJsonAsync<List<CartItem>>();

            Assert.NotNull(items);
            Assert.Contains(items, i => i.Id == item.Id && i.Quantity == 2);
        }

        [Fact]
        public async Task RemoveCartItem_DecreasesQuantity()
        {
            var cartId = "integration-remove";
            var item = new CartItem
            {
                Id = 202,
                Name = "Removable Item",
                Price = 5.00m,
                Quantity = 2
            };

            await _client.PostAsJsonAsync($"/api/v2/carts/{cartId}/items", item);
            await _client.PostAsJsonAsync($"/api/v2/carts/{cartId}/items", item);

            var deleteResponse = await _client.DeleteAsync($"/api/v2/carts/{cartId}/items/{item.Id}");
            deleteResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync($"/api/v2/carts/{cartId}/items");
            var items = await getResponse.Content.ReadFromJsonAsync<List<CartItem>>();

            var updatedItem = items?.FirstOrDefault(i => i.Id == item.Id);
            Assert.NotNull(updatedItem);
            Assert.Equal(3, updatedItem!.Quantity);
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