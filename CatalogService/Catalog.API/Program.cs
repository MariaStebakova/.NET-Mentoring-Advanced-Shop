using System.Security.Claims;

using Catalog.Application.Services;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Messaging;
using Catalog.Infrastructure.Repositories;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<CatalogDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDb")));
}

var publisher = new RabbitMqMessagePublisher();
await publisher.InitializeAsync();
builder.Services.AddSingleton<IMessagePublisher>(publisher);

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

MapCategoryEndpoints(app);
MapProductEndpoints(app);

await app.RunAsync();

// CategoryEndpoints.cs
static void MapCategoryEndpoints(WebApplication app)
{
    app.MapGet("/api/categories", [Authorize(Roles = "Manager,StoreCustomer")] async (
        [FromServices] ICategoryService service, HttpContext http) =>
    {
        var categories = await service.GetAllAsync();
        var result = categories.Select(c => new
        {
            c.Id,
            c.Name,
            c.ImageUrl,
            c.ParentCategoryId,
            _links = new
            {
                self = new { href = GetCategoryUrl(http, c.Id) },
                update = new { href = GetCategoryUrl(http, c.Id), method = "PUT" },
                delete = new { href = GetCategoryUrl(http, c.Id), method = "DELETE" }
            }
        });
        return Results.Ok(result);
    });

    app.MapPost("/api/categories", [Authorize(Roles = "Manager")] async (
        [FromServices] ICategoryService service, [FromBody] Catalog.Domain.Entities.Category category) =>
    {
        try
        {
            var result = await service.AddAsync(category);
            return Results.Created($"/api/categories/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    });

    app.MapPut("/api/categories/{id}", [Authorize(Roles = "Manager")] async (
        [FromServices] ICategoryService service, [FromBody] Catalog.Domain.Entities.Category category, int id) =>
    {
        try
        {
            category.Id = id;
            await service.UpdateAsync(category);
            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    });

    app.MapDelete("/api/categories/{id}", [Authorize(Roles = "Manager")] async (
        [FromServices] ICategoryService categoryService,
        [FromServices] IProductService productService,
        int id) =>
    {
        try
        {
            var category = await categoryService.GetByIdAsync(id);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        var products = await productService.GetAllAsync();
        var related = products.Where(p => p.CategoryId == id);
        foreach (var product in related)
        {
            await productService.DeleteAsync(product.Id);
        }

        await categoryService.DeleteAsync(id);
        return Results.NoContent();
    });
}

// ProductEndpoints.cs
static void MapProductEndpoints(WebApplication app)
{
    app.MapGet("/api/products", GetProductsEndpoint);

    app.MapPost("/api/products", [Authorize(Roles = "Manager")] async (
        [FromServices] IProductService service, [FromBody] Catalog.Domain.Entities.Product product) =>
    {
        try
        {
            var result = await service.AddAsync(product);
            return Results.Created($"/api/products/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    });

    app.MapPut("/api/products/{id}", [Authorize(Roles = "Manager")] async (
        [FromServices] IProductService service, [FromBody] Catalog.Domain.Entities.Product product, int id) =>
    {
        try
        {
            product.Id = id;
            await service.UpdateAsync(product);
            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    });

    app.MapDelete("/api/products/{id}", [Authorize(Roles = "Manager")] async (
        [FromServices] IProductService service, int id) =>
    {
        try
        {
            var _ = await service.GetByIdAsync(id);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }

        await service.DeleteAsync(id);
        return Results.NoContent();
    });
}

static async Task<IResult> GetProductsEndpoint(
    [FromServices] IProductService service,
    [FromQuery] int? categoryId,
    [FromQuery] int? page,
    [FromQuery] int? pageSize,
    HttpContext http)
{
    var all = await service.GetAllAsync();
    var filtered = categoryId.HasValue ? all.Where(p => p.CategoryId == categoryId) : all;
    var paginated = page.HasValue && pageSize.HasValue
        ? filtered.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value)
        : filtered;

    var result = paginated.Select(p => new
    {
        p.Id,
        p.Name,
        p.Description,
        p.ImageUrl,
        p.CategoryId,
        p.Price,
        p.Currency,
        p.Amount,
        _links = new
        {
            self = new { href = GetProductUrl(http, p.Id) },
            update = new { href = GetProductUrl(http, p.Id), method = "PUT" },
            delete = new { href = GetProductUrl(http, p.Id), method = "DELETE" }
        }
    });

    return Results.Ok(result);
}

// UrlHelpers.cs
static string GetCategoryUrl(HttpContext http, int id) =>
    $"{http.Request.Scheme}://{http.Request.Host}/api/categories/{id}";

static string GetProductUrl(HttpContext http, int id) =>
    $"{http.Request.Scheme}://{http.Request.Host}/api/products/{id}";

public partial class Program {
    protected Program() { }
}
