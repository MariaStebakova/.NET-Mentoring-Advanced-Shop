using CatalogGraphQLGateway.GraphQL.Inputs;
using CatalogGraphQLGateway.Models;

namespace CatalogGraphQLGateway.Rest;

public interface ICatalogRestClient
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(int? categoryId, int? page, int? pageSize);
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto> AddCategoryAsync(CategoryInput input);
    Task<ProductDto> AddProductAsync(ProductInput input);
    Task<bool> UpdateCategoryAsync(int id, CategoryInput input);
    Task<bool> UpdateProductAsync(int id, ProductInput input);
    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> DeleteProductAsync(int id);
}
