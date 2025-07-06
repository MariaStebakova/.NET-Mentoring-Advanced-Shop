namespace CatalogGraphQLGateway.GraphQL.Inputs;

public class CategoryInput
{
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
}
