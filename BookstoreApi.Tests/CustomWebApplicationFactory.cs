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
    });

    builder.ConfigureAppConfiguration((context, configBuilder) =>
    {
        configBuilder.AddJsonFile("appsettings.json");
        // Add other configuration sources if needed
    });

    builder.ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var bookStoreConfig = configuration.Get<BookStoreConfiguration>() ?? throw new InvalidOperationException("Failed to load the BookStore configuration");
        
        services.AddSingleton(bookStoreConfig.JwtSettings);
        services.AddSingleton(bookStoreConfig.AdminSettings);
    });

    builder.ConfigureTestServices(services =>
    {
        using var scope = services.BuildServiceProvider().CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure the database is clean before seeding
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        // Seed the database with test data
        TestDataSeeder.SeedTestData(db);
    });
}

}
