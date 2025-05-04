using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using CartService.API;

namespace CartService.Tests.ApiTests.Helpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public readonly string TestDBFileName = "CartTest.db";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var testSettings = new Dictionary<string, string>
                {
                    ["ConnectionStrings:CartDatabase"] = $"Filename={TestDBFileName};Connection=shared"
                };
                config.AddInMemoryCollection(testSettings);
            });
        }
    }
}
