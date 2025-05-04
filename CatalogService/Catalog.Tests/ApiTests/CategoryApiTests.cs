using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Catalog.Tests.ApiTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Catalog.Tests.ApiTests
{
    public class CategoryApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CatalogDbContext _dbContext;

        public CategoryApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        }

        [Fact]
        public async Task PostCategory_ReturnsCreated()
        {
            var category = new Category { Name = "Books" };
            var response = await _client.PostAsJsonAsync("/api/categories", category);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PostCategory_ReturnsBadRequest_WhenNameMissing()
        {
            var category = new Category { Name = "" };
            var response = await _client.PostAsJsonAsync("/api/categories", category);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutCategory_UpdatesSuccessfully()
        {
            var category = new Category { Name = "Initial" };
            var createResp = await _client.PostAsJsonAsync("/api/categories", category);
            var created = await createResp.Content.ReadFromJsonAsync<Category>();

            created!.Name = "Updated";
            var updateResp = await _client.PutAsJsonAsync($"/api/categories/{created.Id}", created);
            Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNotFound_WhenInvalidId()
        {
            var response = await _client.DeleteAsync("/api/categories/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
