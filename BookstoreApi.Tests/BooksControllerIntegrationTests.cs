using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Options;
using BookStoreApi.Requests;
using BookStoreApi.Responses;
using BookStoreApi.Services;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

public class BooksControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JwtSettings _jwtSettings;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly AppDbContext _dbContext;
    private readonly ITestOutputHelper _testOutputHelper;

    public BooksControllerIntegrationTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _factory = factory;
        _client = _factory.CreateClient();
        _jwtSettings = _factory.Services.GetRequiredService<JwtSettings>();

        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, "Admin");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        ResetDatabase();
    }

    private void SeedDatabase()
    {
        // Seed the database with initial test data
        TestDataSeeder.SeedTestData(_dbContext);
        _dbContext.SaveChanges();
    }

    private void ResetDatabase()
    {
        // Ensure the database is clean and then re-seed if necessary
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
        SeedDatabase();  // Call a method to seed the database, if needed
    }

    protected void Dispose()
    {
        // Optionally clean up the database after each test if needed
        _dbContext.Database.EnsureDeleted();
    }

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private HttpClient CreateClientWithRole(string role)
    {
        var client = _factory.CreateClient();
        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, role);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        return client;
    }

    [Fact]
    public async Task GetBook_ReturnsOk_WhenBookExists()
    {
        // Arrange
        var bookId = 3;

        // Act
        var response = await _client.GetAsync($"/api/v1/books/{bookId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var book = JsonSerializer.Deserialize<Book>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(book);
        Assert.Equal(bookId, book?.Id);
        Assert.Equal("Third Test Book", book?.Title);
    }

    [Fact]
    public async Task UpdateBook_ReturnsOk_WhenBookExists_AndVerifyUpdate()
    {
        // Arrange
        var bookId = 2;
        var request = new UpdateBookRequest
        {
            Title = "Updated Test Book",
            ISBN = "1234567890123",
            Author = "Updated Author",
            PublishedDate = DateOnly.Parse("2002-01-01"),
            Price = 11.99m,
            Quantity = 50
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/books/{bookId}", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        // Fetch the book again to verify the update
        response = await _client.GetAsync($"/api/v1/books/{bookId}");
        response.EnsureSuccessStatusCode();

        var bookContent = await response.Content.ReadAsStringAsync();
        var book = JsonSerializer.Deserialize<Book>(bookContent, GetJsonSerializerOptions());
        var responseContent = JsonSerializer.Deserialize<UpdateBookResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        Assert.Equal("Updated Test Book", book?.Title);  // Verify the title was updated correctly.

        Assert.Equal(request.Title, responseContent?.Title);
        Assert.Equal(request.Author, responseContent?.Author);
        Assert.Equal(request.Price, responseContent?.Price);
        Assert.Equal(request.ISBN, responseContent?.ISBN);
        Assert.Equal(request.Quantity, responseContent?.Quantity);
        Assert.Equal(request.PublishedDate, responseContent?.PublishedDate);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreated_WhenUserIsAdmin()
    {
        // Arrange
        var book = new
        {
            Title = "Test Book",
            ISBN = "1234567890123",
            Author = "Author",
            PublishedDate = "2020-01-01",
            Price = 10.99m,
            Quantity = 100
        };

        var content = new StringContent(JsonSerializer.Serialize(book), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/books")
        {
            Content = content
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        var client = _factory.CreateClient();  // No JWT Token

        var book = new
        {
            Title = "Unauthorized Test Book",
            ISBN = "1234567890123",
            Author = "Unauthorized Author",
            PublishedDate = "2020-01-01",
            Price = 10.99m,
            Quantity = 100
        };

        var content = new StringContent(JsonSerializer.Serialize(book), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/books")
        {
            Content = content
        };

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreated_WhenUserIsAuthorized()
    {
        // Arrange
        var book = new
        {
            Title = "Authorized Test Book",
            ISBN = "9876543210987",
            Author = "Authorized Author",
            PublishedDate = "2021-01-01",
            Price = 12.99m,
            Quantity = 50
        };

        var content = new StringContent(JsonSerializer.Serialize(book), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/books")
        {
            Content = content
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        var client = _factory.CreateClient();  // No JWT Token
        var request = new UpdateBookRequest
        {
            Title = "Unauthorized Update",
            ISBN = "1234567890123",
            Author = "Test Author",
            PublishedDate = DateOnly.Parse("2020-01-01"),
            Price = 15.99m,
            Quantity = 5
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/books/1", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        var client = _factory.CreateClient();  // No JWT Token

        // Act
        var response = await client.DeleteAsync("/api/v1/books/1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsOk_WhenUserIsAuthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBook_ReturnsOk_WhenUserIsAuthorized()
    {
        // Arrange
        var bookId = 2;

        // Act
        var response = await _client.GetAsync($"/api/v1/books/{bookId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var book = JsonSerializer.Deserialize<Book>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(book);
        Assert.Equal(bookId, book.Id);
    }

    [Fact]
    public async Task UpdateBook_ReturnsOk_WhenUserIsAdmin()
    {
        // Arrange
        var request = new UpdateBookRequest
        {
            Title = "Updated Test Book",
            ISBN = "1234567890123",
            Author = "Updated Author",
            PublishedDate = DateOnly.Parse("2020-01-01"),
            Price = 11.99m,
            Quantity = 50
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/books/1", request);
        var responseContent = JsonSerializer.Deserialize<UpdateBookResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(request.Title, responseContent?.Title);
        Assert.Equal(request.Author, responseContent?.Author);
        Assert.Equal(request.Price, responseContent?.Price);
        Assert.Equal(request.ISBN, responseContent?.ISBN);
        Assert.Equal(request.Quantity, responseContent?.Quantity);
        Assert.Equal(request.PublishedDate, responseContent?.PublishedDate);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenUserIsAdmin()
    {
        // Act
        var response = await _client.DeleteAsync("/api/v1/books/1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        // Arrange
        var client = CreateClientWithRole("User");  // Non-admin role
        var request = new CreateBookRequest
        {
            Title = "Forbidden Test Book",
            ISBN = "1234567890123",
            Author = "Unauthorized Author",
            PublishedDate = DateOnly.Parse("2020-01-01"),
            Price = 10.99m,
            Quantity = 100
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/books", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        // Arrange
        var client = CreateClientWithRole("User");  // Non-admin role
        var request = new UpdateBookRequest
        {
            Title = "Unauthorized Update",
            ISBN = "1234567890123",
            Author = "Test Author",
            PublishedDate = DateOnly.Parse("2020-01-01"),
            Price = 15.99m,
            Quantity = 5
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/books/1", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        // Arrange
        var client = CreateClientWithRole("User");  // Non-admin role

        // Act
        var response = await client.DeleteAsync("/api/v1/books/1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetBooks_ReturnsCorrectBooks_ByTitle()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?Title=Initial Test Book");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(books);
        Assert.Single(books);
        Assert.Equal("Initial Test Book", books[0].Title);
    }

    [Fact]
    public async Task GetBooks_ReturnsCorrectBooks_ByPartialTitle()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?Title=Filtered");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(books);
        Assert.Equal(2, books.Count);
        Assert.Equal("First Filtered Test Book", books[0].Title);
        Assert.Equal("Second Filtered Test Book", books[1].Title);
    }

    [Fact]
    public async Task GetBooks_ReturnsCorrectBooks_ByAuthor()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?Author=Fourth Author");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(books);
        Assert.Equal(3, books.Count);
    }

    [Fact]
    public async Task GetBooks_ReturnsCorrectBooks_ByPartialAuthor()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?Author=Filter Test");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(books);
        Assert.Single(books);
    }

    [Fact]
    public async Task GetBooks_PaginatesCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?PageNumber=1&PageSize=2");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(books);
        Assert.Equal(2, books.Count); // Check if only two books are returned
    }

    [Fact]
    public async Task GetBooks_SortsByPublishedDate_Descending()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(books);

        // Use LINQ to ensure each book's published date is greater than or equal to the next one's
        bool isSortedDescending = books.Select(b => b.PublishedDate)
                                        .SequenceEqual(books.Select(b => b.PublishedDate).OrderByDescending(d => d));

        Assert.True(isSortedDescending, "Books are not sorted by published date in descending order.");
    }

    [Fact]
    public async Task GetBooks_ReturnsEmpty_WhenNoMatchFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?Title=Nonexistent Book");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetBooks_ReturnsBadRequest_WhenPageNumberIsInvalid()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?PageNumber=-1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    // These next tests rely on the descending by PublishedDate sorting to work. And not deleting/updating books that come up first using that
    [Fact]
    public async Task GetBooks_ReturnsFirstPageCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?PageNumber=1&PageSize=2");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        Assert.NotNull(books);
        Assert.Equal(2, books.Count);
        Assert.Equal("Fourth Test Book", books[0].Title);
        Assert.Equal("Third Test Book", books[1].Title);
    }

    [Fact]
    public async Task GetBooks_ReturnsSecondPageCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books?PageNumber=2&PageSize=2");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var books = JsonSerializer.Deserialize<List<Book>>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        Assert.NotNull(books);
        Assert.Equal(2, books.Count);
        Assert.Equal("Fifth Test Book", books[0].Title);
        Assert.Equal("First Filtered Test Book", books[1].Title);
    }

    [Fact]
    public async Task CreateBook_ReturnsServerError_OnException()
    {
        // Arrange
        var localMockBookService = new Mock<IBookService>();
        localMockBookService.Setup(service => service.CreateBookAsync(It.IsAny<Book>()))
                            .Throws(new Exception("Simulated internal error"));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => localMockBookService.Object);
            });
        }).CreateClient();
        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, "Admin");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        var request = new CreateBookRequest
        {
            Title = "Error Test Book",
            ISBN = "0000000000",
            Author = "Error",
            PublishedDate = DateOnly.Parse("2021-01-01"),
            Price = 10.99m,
            Quantity = 100
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/books", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ReturnsServerError_OnException()
    {
        // Arrange
        var localMockBookService = new Mock<IBookService>();
        localMockBookService.Setup(service => service.UpdateBookAsync(It.IsAny<Book>()))
                            .Throws(new Exception("Simulated internal error"));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => localMockBookService.Object);
            });
        }).CreateClient();

        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, "Admin");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        var request = new UpdateBookRequest
        {
            Title = "Error Test Book",
            ISBN = "0000000000",
            Author = "Error",
            PublishedDate = DateOnly.Parse("2021-01-01"),
            Price = 10.99m,
            Quantity = 100
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/books/1", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsServerError_OnException()
    {
        // Arrange
        var localMockBookService = new Mock<IBookService>();
        localMockBookService.Setup(service => service.DeleteBookAsync(It.IsAny<int>()))
                            .Throws(new Exception("Simulated internal error"));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => localMockBookService.Object);
            });
        }).CreateClient();

        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, "Admin");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        // Act
        var response = await client.DeleteAsync("/api/v1/books/1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
