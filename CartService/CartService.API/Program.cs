using Microsoft.OpenApi.Models;
using CartService.Application.Interfaces;
using CartService.Infrastructure.Repositories;
using CartService.Infrastructure.Messaging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        builder.Services.AddSingleton<ICartService, Application.Services.CartService>();
        builder.Services.AddLogging();

        builder.Services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Manager", "StoreCustomer")
                .Build();

            options.Filters.Add(new AuthorizeFilter(policy));
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Cart API", Version = "v1" });
            options.SwaggerDoc("v2", new OpenApiInfo { Title = "Cart API", Version = "v2" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        builder.Services.AddHostedService<RabbitMqItemUpdateListener>();

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
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

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart API v1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "Cart API v2");
        });

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.Use(async (context, next) =>
        {
            var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                Console.WriteLine("Access Token Claims:");
                foreach (var claim in jwt.Claims)
                {
                    Console.WriteLine($" - {claim.Type}: {claim.Value}");
                }
            }

            await next();
        });
        app.UseAuthorization();
        app.MapControllers();

        app.Run();

    }
}
