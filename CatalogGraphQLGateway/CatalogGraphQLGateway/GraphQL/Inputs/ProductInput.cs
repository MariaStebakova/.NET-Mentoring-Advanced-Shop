namespace CatalogGraphQLGateway.GraphQL.Inputs;

public class ProductInput
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int Amount { get; set; }
}
