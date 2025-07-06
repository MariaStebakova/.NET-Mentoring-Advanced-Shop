using System.Security.Claims;

using CatalogGraphQLGateway.DataLoaders;
using CatalogGraphQLGateway.GraphQL;
using CatalogGraphQLGateway.Rest;
using CatalogGraphQLGateway.Types;

using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<ICatalogRestClient, CatalogRestClient>();
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<ProductType>()
    .AddType<CategoryType>()
    .AddDataLoader<CategoryByIdDataLoader>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication().AddJwtBearer("Bearer", options =>
{
    options.Authority = "http://localhost:8080/realms/MicroservicesRealm";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        RoleClaimType = ClaimTypes.Role
    };
    options.RequireHttpsMetadata = false;
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapGraphQL();

app.Run();