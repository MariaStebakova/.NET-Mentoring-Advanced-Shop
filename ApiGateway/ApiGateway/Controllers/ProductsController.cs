using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Authorize(Roles = "Manager,StoreCustomer")]
[Route("gateway/products")]
public class ProductsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("{id}/combined")]
    public async Task<IActionResult> GetProductWithProperties(int id)
    {
        var client = _httpClientFactory.CreateClient();

        var accessToken = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(accessToken))
            return Unauthorized("Missing Authorization header.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Replace("Bearer ", ""));

        var productResponse = await client.GetAsync($"http://catalog-service/api/products/{id}");
        var propsResponse = await client.GetAsync($"http://catalog-service/api/products/{id}/properties");

        if (!productResponse.IsSuccessStatusCode || !propsResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)productResponse.StatusCode, "Failed to get product or properties");
        }

        var productJson = await productResponse.Content.ReadAsStringAsync();
        var propsJson = await propsResponse.Content.ReadAsStringAsync();

        var productElement = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(productJson);
        var propsElement = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(propsJson);

        var product = productElement?.ToDictionary(kvp => kvp.Key, kvp => ConvertJsonElement(kvp.Value));
        var properties = propsElement?.ToDictionary(kvp => kvp.Key, kvp => ConvertJsonElement(kvp.Value));

        var result = new
        {
            product,
            properties
        };

        return Ok(result);
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText()),
            JsonValueKind.Array => JsonSerializer.Deserialize<List<object>>(element.GetRawText()),
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }
}
