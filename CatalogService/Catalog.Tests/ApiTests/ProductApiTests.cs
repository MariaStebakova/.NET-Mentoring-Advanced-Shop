using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Catalog.Tests.ApiTests.Helpers;

namespace Catalog.Tests.ApiTests
{
    public class ProductApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CatalogDbContext _dbContext;

        public ProductApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        }

        [Fact]
        public async Task PostProduct_ReturnsCreated_WhenValid()
        {
            var category = new Category { Name = "Electronics" };
            var categoryResponse = await _client.PostAsJsonAsync("/api/categories", category);
            var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<Category>();

            var product = new Product
            {
                Name = "Smartphone",
                Price = 299.99m,
                Currency = "USD",
                Amount = 10,
                CategoryId = createdCategory!.Id
            };

            var response = await _client.PostAsJsonAsync("/api/products", product);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<Product>();
            Assert.Equal("Smartphone", result?.Name);
        }

        [Fact]
        public async Task PutProduct_ReturnsNoContent_WhenValid()
        {
            var category = new Category { Name = "UpdateCat" };
            var categoryResp = await _client.PostAsJsonAsync("/api/categories", category);
            var cat = await categoryResp.Content.ReadFromJsonAsync<Category>();

            var product = new Product
            {
                Name = "Old",
                Price = 9.99m,
                Currency = "USD",
                Amount = 1,
                CategoryId = cat!.Id
            };
            var createResp = await _client.PostAsJsonAsync("/api/products", product);
            var created = await createResp.Content.ReadFromJsonAsync<Product>();

            created!.Name = "Updated Name";
            var updateResp = await _client.PutAsJsonAsync($"/api/products/{created.Id}", created);

            Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);
        }

        [Fact]
        public async Task GetProducts_FilterByCategory_ReturnsFiltered()
        {
            var category = new Category { Name = "FilterCat" };
            var categoryResponse = await _client.PostAsJsonAsync("/api/categories", category);
            categoryResponse.EnsureSuccessStatusCode();
            var createdCategory = await categoryResponse.Content.ReadFromJsonAsync<Category>();

            var product = new Product
            {
                Name = "FilteredProduct",
                Price = 199.99m,
                Currency = "USD",
                Amount = 5,
                CategoryId = createdCategory!.Id
            };
            await _client.PostAsJsonAsync("/api/products", product);

            var response = await _client.GetAsync($"/api/products?categoryId={createdCategory.Id}&page=1&pageSize=10");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("FilteredProduct", content);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenInvalidId()
        {
            var response = await _client.DeleteAsync("/api/products/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
