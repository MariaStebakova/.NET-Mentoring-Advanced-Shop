using CartService.API;
using CartService.Infrastructure.Messaging;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CartService.Tests.ApiTests.Helpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public readonly string TestDBFileName = "CartTest.db";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var testSettings = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:CartDatabase"] = $"Filename={TestDBFileName};Connection=shared"
                };
                config.AddInMemoryCollection(initialData: testSettings);
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ImplementationType == typeof(RabbitMqItemUpdateListener));
                if (descriptor != null)
                    services.Remove(descriptor);
            });
        }
    }
}