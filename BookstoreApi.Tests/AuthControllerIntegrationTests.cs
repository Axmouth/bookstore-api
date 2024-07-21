using System.Net.Http.Json;
using System.Text.Json;
using BookStoreApi.Data;
using BookStoreApi.Requests;
using BookStoreApi.Responses;
using Identity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

public class AuthControllerTests : TestBase
{
    public AuthControllerTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsAreValid()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = TestDataSeeder.TestUserName,
            Password = TestDataSeeder.TestUserPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(loginResponse);
        Assert.False(string.IsNullOrEmpty(loginResponse?.Token));
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUsernameIsInvalid()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "invaliduser",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = TestDataSeeder.TestUserName,
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var store = new Mock<IUserStore<AppUser>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var mockUserManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);
#pragma warning restore CS8625

        mockUserManager.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("An unexpected error occurred."));

        // Replace the services just for this request
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => mockUserManager.Object);
            });
        }).CreateClient();

        var loginRequest = new LoginRequest
        {
            Username = TestDataSeeder.TestUserName,
            Password = TestDataSeeder.TestUserPassword
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var details = JsonSerializer.Deserialize<ErrorDetails>(responseContent);
        Assert.Equal("An unexpected error occurred.", details?.Message);
    }

}
