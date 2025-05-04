using Microsoft.OpenApi.Models;
using CartService.Application.Interfaces;
using CartService.Infrastructure.Repositories;

namespace CartService.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("CartDatabase");
        builder.Services.AddSingleton<ICartRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("CartDatabase");

            return new CartRepository(connectionString, provider.GetRequiredService<ILogger<CartRepository>>());
        });

        builder.Services.AddSingleton<ICartService, CartService.Application.Services.CartService>();
        builder.Services.AddLogging();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Cart API", Version = "v1" });
            options.SwaggerDoc("v2", new OpenApiInfo { Title = "Cart API", Version = "v2" });
            var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart API v1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "Cart API v2");
        });

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();

    }
}
