using CatalogGraphQLGateway.Models;
using CatalogGraphQLGateway.Rest;

using HotChocolate.Authorization;

namespace CatalogGraphQLGateway.GraphQL;

[Authorize(Roles = new[] { "Manager", "StoreCustomer" })]
public class Query
{
    public Task<IEnumerable<ProductDto>> GetProducts(
        int? categoryId, int? page, int? pageSize,
        [Service] ICatalogRestClient client) =>
        client.GetProductsAsync(categoryId, page, pageSize);

    public Task<IEnumerable<CategoryDto>> GetCategories(
        [Service] ICatalogRestClient client) =>
        client.GetCategoriesAsync();
}
