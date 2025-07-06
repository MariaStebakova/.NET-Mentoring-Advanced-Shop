using CatalogGraphQLGateway.DataLoaders;
using CatalogGraphQLGateway.Models;

namespace CatalogGraphQLGateway.Types;

public class ProductType : ObjectType<ProductDto>
{
    protected override void Configure(IObjectTypeDescriptor<ProductDto> descriptor)
    {
        descriptor.Field(p => p.Id);
        descriptor.Field(p => p.Name);
        descriptor.Field(p => p.Description);
        descriptor.Field(p => p.ImageUrl);
        descriptor.Field(p => p.CategoryId);
        descriptor.Field(p => p.Price);
        descriptor.Field(p => p.Currency);
        descriptor.Field(p => p.Amount);

        descriptor
            .Field("category")
            .Resolve(async (context, ct) =>
            {
                var loader = context.DataLoader<CategoryByIdDataLoader>();
                var product = context.Parent<ProductDto>();
                return await loader.LoadAsync(product.CategoryId, ct);
            });
    }
}
