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

    private const string ApiBaseUrl = "/api/v1/Books";

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
        SeedDatabase();
    }

    protected void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
    }

    private static JsonSerializerOptions GetJsonSerializerOptions() =>
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
        var bookId = 3;
        var response = await _client.GetAsync($"{ApiBaseUrl}/{bookId}");
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
        Assert.Equal(request.Quantity, responseContent?.Quantity);
        Assert.Equal(request.PublishedDate, responseContent?.PublishedDate);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreated_WhenUserIsAdmin()
    {
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
        var request = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl)
        {
            Content = content
        };

        var response = await _client.SendAsync(request);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        var client = _factory.CreateClient();

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
        var request = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl)
        {
            Content = content
        };

        var response = await client.SendAsync(request);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreated_WhenUserIsAuthorized()
    {
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
        var request = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl)
        {
            Content = content
        };

        var response = await _client.SendAsync(request);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        var client = _factory.CreateClient();
        var request = new UpdateBookRequest
        {
            Title = "Unauthorized Update",
            Author = "Test Author",
            PublishedDate = DateOnly.Parse("2020-01-01"),
            Price = 15.99m,
            Quantity = 5
        };

        var response = await client.PutAsJsonAsync($"{ApiBaseUrl}/1", request);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"{ApiBaseUrl}/1");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsOk_WhenUserIsAuthorized()
    {
        var response = await _client.GetAsync(ApiBaseUrl);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBook_ReturnsOk_WhenUserIsAuthorized()
    {
        var bookId = 2;
        var response = await _client.GetAsync($"{ApiBaseUrl}/{bookId}");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var book = JsonSerializer.Deserialize<Book>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(book);
        Assert.Equal(bookId, book.Id);
    }

    [Fact]
    public async Task UpdateBook_ReturnsOk_WhenUserIsAdmin()
    {
        var request = new UpdateBookRequest
        {
            Title = "Updated Test Book",
            Author = "Updated Author",
            PublishedDate = DateOnly.Parse("2020-01-01"),
            Price = 11.99m,
            Quantity = 50
        };

        var response = await _client.PutAsJsonAsync($"{ApiBaseUrl}/1", request);
        var responseContent = JsonSerializer.Deserialize<UpdateBookResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(request.Title, responseContent?.Title);
        Assert.Equal(request.Author, responseContent?.Author);
        Assert.Equal(request.Price, responseContent?.Price);
        Assert.Equal(request.Quantity, responseContent?.Quantity);
        Assert.Equal(request.PublishedDate, responseContent?.PublishedDate);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenUserIsAdmin()
    {
        var response = await _client.DeleteAsync($"{ApiBaseUrl}/1");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        var client = CreateClientWithRole("User");
        var request = new CreateBookRequest
        {
            Title = "Forbidden Test Book",
            ISBN = "1234567890123",
            Author = "Unauthorized Author",
            PublishedDate = DateOnly.Parse("2020-01-01"),
            Price = 10.99m,
            Quantity = 100
        };

        var response = await client.PostAsJsonAsync(ApiBaseUrl, request);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        var client = CreateClientWithRole("User");
        var request = new UpdateBookRequest
        {
            Title = "Unauthorized Update",
            Author = "Test Author",
            PublishedDate = DateOnly.Parse("2020-01-01"),
            Price = 15.99m,
            Quantity = 5
        };

        var response = await client.PutAsJsonAsync($"{ApiBaseUrl}/1", request);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        var client = CreateClientWithRole("User");
        var response = await client.DeleteAsync($"{ApiBaseUrl}/1");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetBooks_ReturnsCorrectBooks_ByTitle()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?Title=Initial Test Book");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(booksResponse);
        Assert.Single(booksResponse.Books);
        Assert.Equal(1, booksResponse.TotalItems);
        Assert.Equal(1, booksResponse.PageNumber);
        Assert.Equal(10, booksResponse.PageSize);
        Assert.Null(booksResponse.NextPage);
        Assert.Null(booksResponse.PreviousPage);
        Assert.Equal("Initial Test Book", booksResponse.Books[0].Title);
    }

    [Fact]
    public async Task GetBooks_ReturnsCorrectBooks_ByPartialTitle()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?Title=Filtered");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(booksResponse);
        Assert.Equal(2, booksResponse.Books.Length);
        Assert.Equal(2, booksResponse.TotalItems);
        Assert.Equal(1, booksResponse.PageNumber);
        Assert.Equal(10, booksResponse.PageSize);
        Assert.Null(booksResponse.NextPage);
        Assert.Null(booksResponse.PreviousPage);
        Assert.Equal("First Filtered Test Book", booksResponse.Books[0].Title);
        Assert.Equal("Second Filtered Test Book", booksResponse.Books[1].Title);
    }

    [Fact]
    public async Task GetBooks_ReturnsCorrectBooks_ByAuthor()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?Author=Fourth Author");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(booksResponse);
        Assert.Equal(3, booksResponse.Books.Length);
        Assert.Equal(3, booksResponse.TotalItems);
        Assert.Equal(1, booksResponse.PageNumber);
        Assert.Equal(10, booksResponse.PageSize);
        Assert.Null(booksResponse.NextPage);
        Assert.Null(booksResponse.PreviousPage);
        Assert.Equal(3, booksResponse.Books.Length);
    }

    [Fact]
    public async Task GetBooks_ReturnsCorrectBooks_ByPartialAuthor()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?Author=Filter Test");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(booksResponse);
        Assert.Single(booksResponse.Books);
        Assert.Equal(1, booksResponse.TotalItems);
        Assert.Equal(1, booksResponse.PageNumber);
        Assert.Equal(10, booksResponse.PageSize);
        Assert.Null(booksResponse.NextPage);
        Assert.Null(booksResponse.PreviousPage);
    }

    [Fact]
    public async Task GetBooks_PaginatesCorrectly()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?PageNumber=1&PageSize=2");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(booksResponse);
        Assert.Equal(2, booksResponse.Books.Length);
        Assert.Equal(1, booksResponse.PageNumber);
        Assert.Equal(2, booksResponse.PageSize);
        Assert.Equal(7, booksResponse.TotalItems);
        Assert.Equal($"http://localhost{ApiBaseUrl}?PageNumber=2&PageSize=2", booksResponse.NextPage);
        Assert.Null(booksResponse.PreviousPage);
    }

    [Fact]
    public async Task GetBooks_SortsByPublishedDate_Descending()
    {
        var response = await _client.GetAsync(ApiBaseUrl);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(booksResponse);

        bool isSortedDescending = booksResponse.Books.Select(b => b.PublishedDate)
                                        .SequenceEqual(booksResponse.Books.Select(b => b.PublishedDate).OrderByDescending(d => d));
        Assert.True(isSortedDescending, "Books are not sorted by published date in descending order.");
    }

    [Fact]
    public async Task GetBooks_ReturnsEmpty_WhenNoMatchFound()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?Title=Nonexistent Book");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());
        Assert.NotNull(booksResponse);
        Assert.Empty(booksResponse.Books);
    }

    [Fact]
    public async Task GetBooks_ReturnsBadRequest_WhenPageNumberIsInvalid()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?PageNumber=-1");
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBooks_ReturnsFirstPageCorrectly()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?PageNumber=1&PageSize=2");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        Assert.NotNull(booksResponse);
        Assert.Equal(2, booksResponse.Books.Length);
        Assert.Equal("Fourth Test Book", booksResponse.Books[0].Title);
        Assert.Equal("Third Test Book", booksResponse.Books[1].Title);
        Assert.Equal(1, booksResponse.PageNumber);
        Assert.Equal(2, booksResponse.PageSize);
        Assert.Equal(7, booksResponse.TotalItems);
        Assert.Equal($"http://localhost{ApiBaseUrl}?PageNumber=2&PageSize=2", booksResponse.NextPage);
        Assert.Null(booksResponse.PreviousPage);
    }

    [Fact]
    public async Task GetBooks_ReturnsSecondPageCorrectly()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?PageNumber=2&PageSize=2");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        Assert.NotNull(booksResponse);
        Assert.Equal(2, booksResponse.Books.Length);
        Assert.Equal("Fifth Test Book", booksResponse.Books[0].Title);
        Assert.Equal("First Filtered Test Book", booksResponse.Books[1].Title);
        Assert.Equal(2, booksResponse.Books.Length);
        Assert.Equal(2, booksResponse.PageNumber);
        Assert.Equal(2, booksResponse.PageSize);
        Assert.Equal(7, booksResponse.TotalItems);
        Assert.Equal($"http://localhost{ApiBaseUrl}?PageNumber=3&PageSize=2", booksResponse.NextPage);
        Assert.Equal($"http://localhost{ApiBaseUrl}?PageNumber=1&PageSize=2", booksResponse.PreviousPage);
    }

    [Fact]
    public async Task GetBooks_ReturnsNextPageLink()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?PageNumber=1&PageSize=2");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        Assert.NotNull(booksResponse);
        Assert.NotNull(booksResponse.NextPage);
        Assert.Equal($"http://localhost{ApiBaseUrl}?PageNumber=2&PageSize=2", booksResponse.NextPage);
    }

    [Fact]
    public async Task GetBooks_ReturnsPreviousPageLink()
    {
        var response = await _client.GetAsync($"{ApiBaseUrl}?PageNumber=2&PageSize=2");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var booksResponse = JsonSerializer.Deserialize<GetBooksResponse>(await response.Content.ReadAsStringAsync(), GetJsonSerializerOptions());

        Assert.NotNull(booksResponse);
        Assert.NotNull(booksResponse.PreviousPage);
        Assert.Equal($"http://localhost{ApiBaseUrl}?PageNumber=1&PageSize=2", booksResponse.PreviousPage);
    }

    [Fact]
    public async Task CreateBook_ReturnsServerError_OnException()
    {
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

        var response = await client.PostAsJsonAsync(ApiBaseUrl, request);
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ReturnsServerError_OnException()
    {
        var localMockBookService = new Mock<IBookService>();
        localMockBookService.Setup(service => service.UpdateBookAsync(It.IsAny<int>(), It.IsAny<UpdateBookRequest>()))
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
            Author = "Error",
            PublishedDate = DateOnly.Parse("2021-01-01"),
            Price = 10.99m,
            Quantity = 100
        };

        var response = await client.PutAsJsonAsync($"{ApiBaseUrl}/1", request);
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsServerError_OnException()
    {
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

        var response = await client.DeleteAsync($"{ApiBaseUrl}/1");
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
