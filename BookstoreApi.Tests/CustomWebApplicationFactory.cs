using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BookStoreApi.Data;
using BookStoreApi.Options;
using Microsoft.AspNetCore.TestHost;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext configuration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext with In-Memory Database for testing
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            // Configure other services
            services.AddControllers().AddApplicationPart(typeof(TStartup).Assembly);
        });

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.json");
            // Add other configuration sources if needed
        });
    }


}
