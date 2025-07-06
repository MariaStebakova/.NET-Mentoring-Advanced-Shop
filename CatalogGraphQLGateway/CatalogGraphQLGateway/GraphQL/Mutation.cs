using CatalogGraphQLGateway.GraphQL.Inputs;
using CatalogGraphQLGateway.Models;
using CatalogGraphQLGateway.Rest;

using HotChocolate.Authorization;

namespace CatalogGraphQLGateway.GraphQL;

[Authorize(Roles = new[] { "Manager" })]
public class Mutation
{
    public async Task<CategoryDto> AddCategoryAsync(
        CategoryInput input,
        [Service] ICatalogRestClient client) =>
        await client.AddCategoryAsync(input);

    public async Task<ProductDto> AddProductAsync(
        ProductInput input,
        [Service] ICatalogRestClient client) =>
        await client.AddProductAsync(input);

    public async Task<bool> UpdateCategoryAsync(
        int id,
        CategoryInput input,
        [Service] ICatalogRestClient client) =>
        await client.UpdateCategoryAsync(id, input);

    public async Task<bool> UpdateProductAsync(
        int id,
        ProductInput input,
        [Service] ICatalogRestClient client) =>
        await client.UpdateProductAsync(id, input);

    public async Task<bool> DeleteCategoryAsync(
        int id,
        [Service] ICatalogRestClient client) =>
        await client.DeleteCategoryAsync(id);

    public async Task<bool> DeleteProductAsync(
        int id,
        [Service] ICatalogRestClient client) =>
        await client.DeleteProductAsync(id);
}
