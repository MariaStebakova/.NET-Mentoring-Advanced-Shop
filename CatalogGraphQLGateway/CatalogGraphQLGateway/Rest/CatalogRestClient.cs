using CatalogGraphQLGateway.GraphQL.Inputs;
using CatalogGraphQLGateway.Models;
using System.Text.Json;

namespace CatalogGraphQLGateway.Rest;

public class CatalogRestClient: ICatalogRestClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string BaseUrl = "http://localhost:5001/api";

    public CatalogRestClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(int? categoryId, int? page, int? pageSize)
    {
        AttachToken();
        var query = new List<string>();
        if (categoryId.HasValue) query.Add($"categoryId={categoryId}");
        if (page.HasValue) query.Add($"page={page}");
        if (pageSize.HasValue) query.Add($"pageSize={pageSize}");

        var url = $"{BaseUrl}/products" + (query.Count > 0 ? $"?{string.Join("&", query)}" : "");
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<List<ProductDto>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        AttachToken();
        var response = await _http.GetAsync($"{BaseUrl}/categories");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<List<CategoryDto>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    public async Task<CategoryDto> AddCategoryAsync(CategoryInput input)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/categories", input);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryDto>() ?? throw new Exception("Invalid response");
    }

    public async Task<ProductDto> AddProductAsync(ProductInput input)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/products", input);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>() ?? throw new Exception("Invalid response");
    }

    public async Task<bool> UpdateCategoryAsync(int id, CategoryInput input)
    {
        AttachToken();
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/categories/{id}", input);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProductAsync(int id, ProductInput input)
    {
        AttachToken();
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/products/{id}", input);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        AttachToken();
        var response = await _http.DeleteAsync($"{BaseUrl}/categories/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        AttachToken();
        var response = await _http.DeleteAsync($"{BaseUrl}/products/{id}");
        return response.IsSuccessStatusCode;
    }

    private void AttachToken()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Remove("Authorization");
            _http.DefaultRequestHeaders.Add("Authorization", token);
        }
    }
}
