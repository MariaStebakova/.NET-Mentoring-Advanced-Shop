using CatalogGraphQLGateway.Models;
using CatalogGraphQLGateway.Rest;

namespace CatalogGraphQLGateway.DataLoaders;

public class CategoryByIdDataLoader : BatchDataLoader<int, CategoryDto>
{
    private readonly ICatalogRestClient _client;

    public CategoryByIdDataLoader(
        ICatalogRestClient client,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _client = client;
    }

    protected override async Task<IReadOnlyDictionary<int, CategoryDto>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var allCategories = await _client.GetCategoriesAsync();
        var dict = allCategories
            .Where(c => keys.Contains(c.Id))
            .ToDictionary(c => c.Id);
        return dict;
    }
}
