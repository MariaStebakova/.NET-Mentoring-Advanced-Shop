using CatalogGraphQLGateway.Models;

namespace CatalogGraphQLGateway.Types;

public class CategoryType : ObjectType<CategoryDto>
{
    protected override void Configure(IObjectTypeDescriptor<CategoryDto> descriptor)
    {
        descriptor.Field(c => c.Id);
        descriptor.Field(c => c.Name);
        descriptor.Field(c => c.ImageUrl);
        descriptor.Field(c => c.ParentCategoryId);
    }
}
