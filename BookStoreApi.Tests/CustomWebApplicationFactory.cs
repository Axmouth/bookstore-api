using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BookStoreApi.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public string DbName { get; private set; }

    public CustomWebApplicationFactory()
    {
        DbName = Guid.NewGuid().ToString();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext with a unique In-Memory Database for each test
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(DbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)), ServiceLifetime.Transient);

            // Configure other services
            services.AddControllers()
                .AddApplicationPart(typeof(TStartup).Assembly);
        });

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.json");
        });
    }
}
