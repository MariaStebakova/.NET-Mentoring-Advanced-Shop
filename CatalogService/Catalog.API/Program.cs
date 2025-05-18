using Catalog.Domain.Interfaces;
using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog.Infrastructure.Messaging;

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

var app = builder.Build();

app.UseHttpsRedirection();

#region Categories API
app.MapGet("/api/categories", async ([FromServices] ICategoryService service, HttpContext http) =>
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
            self = new { href = $"{http.Request.Scheme}://{http.Request.Host}/api/categories/{c.Id}" },
            update = new { href = $"{http.Request.Scheme}://{http.Request.Host}/api/categories/{c.Id}", method = "PUT" },
            delete = new { href = $"{http.Request.Scheme}://{http.Request.Host}/api/categories/{c.Id}", method = "DELETE" }
        }
    });
    return Results.Ok(result);
})
.WithName("GetAllCategories")
.Produces(200);

app.MapPost("/api/categories", async ([FromServices] ICategoryService service, [FromBody] Category category) =>
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
})
.WithName("CreateCategory")
.Produces<Category>(201);

app.MapPut("/api/categories/{id}", async ([FromServices] ICategoryService service, [FromBody] Category category, int id) =>
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
})
.WithName("UpdateCategory")
.Produces(204);

app.MapDelete("/api/categories/{id}", async (
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
})
.WithName("DeleteCategoryAndProducts")
.Produces(204)
.Produces(404);
#endregion

#region Products API
app.MapGet("/api/products", async (
    [FromServices] IProductService service,
    [FromQuery] int? categoryId,
    [FromQuery] int? page,
    [FromQuery] int? pageSize,
    HttpContext http) =>
{
    var all = await service.GetAllAsync();
    var filtered = categoryId.HasValue ? all.Where(p => p.CategoryId == categoryId) : all;
    var paginated = page.HasValue && pageSize.HasValue ? filtered.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value) : filtered;

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
            self = new { href = $"{http.Request.Scheme}://{http.Request.Host}/api/products/{p.Id}" },
            update = new { href = $"{http.Request.Scheme}://{http.Request.Host}/api/products/{p.Id}", method = "PUT" },
            delete = new { href = $"{http.Request.Scheme}://{http.Request.Host}/api/products/{p.Id}", method = "DELETE" }
        }
    });

    return Results.Ok(result);
})
.WithName("GetProducts")
.Produces(200);

app.MapPost("/api/products", async ([FromServices] IProductService service, [FromBody] Product product) =>
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
})
.WithName("CreateProduct")
.Produces<Product>(201);

app.MapPut("/api/products/{id}", async ([FromServices] IProductService service, [FromBody] Product product, int id) =>
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
})
.WithName("UpdateProduct")
.Produces(204);

app.MapDelete("/api/products/{id}", async ([FromServices] IProductService service, int id) =>
{
    try
    {
        var category = await service.GetByIdAsync(id);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }

    await service.DeleteAsync(id);
    return Results.NoContent();
})
.WithName("DeleteProduct")
.Produces(204)
.Produces(404);
#endregion

app.Run();

public partial class Program { }