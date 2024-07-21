using Microsoft.Extensions.DependencyInjection;
using BookStoreApi.Data;
using Microsoft.AspNetCore.Identity;
using Identity.Models;
using Xunit.Abstractions;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    protected readonly CustomWebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly ITestOutputHelper _testOutputHelper;
    private readonly IServiceScope _scope;
    protected readonly AppDbContext _dbContext;

    protected TestBase(CustomWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _factory = factory;
        _client = factory.CreateClient();

        var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
        _scope = scopeFactory!.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        SeedTestData().Wait();
    }

    private async Task SeedTestData()
    {
        _dbContext.Database.EnsureDeleted();
        await TestDataSeeder.SeedTestData(_scope, _dbContext);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
