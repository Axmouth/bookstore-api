using System.Net.Http;
using System.Text;
using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Options;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

public class BooksControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JwtSettings _jwtSettings;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public BooksControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jwtSettings = _factory.Services.GetRequiredService<JwtSettings>();

        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, "Admin");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

    }

    [Fact]
    public async Task GetBook_ReturnsOk_WhenBookExists()
    {
        var bookId = 3;

        var response = await _client.GetAsync($"/api/v1/books/{bookId}");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var book = JsonConvert.DeserializeObject<Book>(await response.Content.ReadAsStringAsync());

        Assert.NotNull(book);
        Assert.Equal(bookId, book?.ID);
        Assert.Equal("Third Test Book", book?.Title);
    }

    [Fact]
    public async Task UpdateBook_ReturnsNoContent_WhenBookExists_AndVerifyUpdate()
    {
        var bookId = 2;
        var updatedBook = new
        {
            ID = bookId,
            Title = "Updated Test Book",
            ISBN = "1234567890123",
            Author = "Updated Author",
            PublishedDate = "2020-01-01",
            Price = 11.99m,
            Quantity = 50
        };

        var content = new StringContent(JsonConvert.SerializeObject(updatedBook), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/v1/books/{bookId}", content);

        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        // Fetch the book again to verify the update
        response = await _client.GetAsync($"/api/v1/books/{bookId}");
        response.EnsureSuccessStatusCode();

        var bookContent = await response.Content.ReadAsStringAsync();
        var book = JsonConvert.DeserializeObject<Book>(bookContent);

        Assert.Equal("Updated Test Book", book?.Title);  // Verify the title was updated correctly.
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenBookExists()
    {
        var bookId = 4; // Ensure this book exists and can be deleted

        var response = await _client.DeleteAsync($"/api/v1/books/{bookId}");

        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
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

        var content = new StringContent(JsonConvert.SerializeObject(book), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/books")
        {
            Content = content
        };

        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        // Create a new client without authorization headers
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

        var content = new StringContent(JsonConvert.SerializeObject(book), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/books")
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

        var content = new StringContent(JsonConvert.SerializeObject(book), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/books")
        {
            Content = content
        };

        // Client is already configured with JWT in the constructor
        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }
    [Fact]
    public async Task UpdateBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        var client = _factory.CreateClient();  // No JWT Token
        var updatedBook = new
        {
            ID = 1,
            Title = "Unauthorized Update",
            ISBN = "1234567890123",
            Author = "Test Author",
            PublishedDate = "2020-01-01",
            Price = 15.99m,
            Quantity = 5
        };

        var content = new StringContent(JsonConvert.SerializeObject(updatedBook), Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/books/1", content);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        var client = _factory.CreateClient();  // No JWT Token
        var response = await client.DeleteAsync("/api/v1/books/1");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsOk_WhenUserIsAuthorized()
    {
        // Assuming no authorization is required; adjust if necessary.
        var response = await _client.GetAsync("/api/v1/books");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBook_ReturnsOk_WhenUserIsAuthorized()
    {
        var bookId = 2;
        var response = await _client.GetAsync($"/api/v1/books/{bookId}");  // Assuming this book exists

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var book = JsonConvert.DeserializeObject<Book>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(book);
        Assert.Equal(bookId, book.ID);  // Assuming ID 1 exists from your seeded data
    }

    [Fact]
    public async Task UpdateBook_ReturnsNoContent_WhenUserIsAdmin()
    {
        var updatedBook = new
        {
            ID = 1,
            Title = "Updated Test Book",
            ISBN = "1234567890123",
            Author = "Updated Author",
            PublishedDate = "2020-01-01",
            Price = 11.99m,
            Quantity = 50
        };

        var content = new StringContent(JsonConvert.SerializeObject(updatedBook), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/v1/books/1", content);

        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenUserIsAdmin()
    {
        var response = await _client.DeleteAsync("/api/v1/books/1");

        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        var client = _factory.CreateClient();
        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, "User");  // Non-admin role
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        var book = new
        {
            Title = "Forbidden Test Book",
            ISBN = "1234567890123",
            Author = "Unauthorized Author",
            PublishedDate = "2020-01-01",
            Price = 10.99m,
            Quantity = 100
        };

        var content = new StringContent(JsonConvert.SerializeObject(book), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/books", content);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        var client = _factory.CreateClient();
        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, "User");  // Non-admin role
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        var updatedBook = new
        {
            ID = 1,
            Title = "Unauthorized Update",
            ISBN = "1234567890123",
            Author = "Test Author",
            PublishedDate = "2020-01-01",
            Price = 15.99m,
            Quantity = 5
        };

        var content = new StringContent(JsonConvert.SerializeObject(updatedBook), Encoding.UTF8, "application/json");
        var response = await client.PutAsync("/api/v1/books/1", content);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        var client = _factory.CreateClient();
        var jwtToken = JwtTokenGenerator.GenerateJwtToken(_jwtSettings, "User");  // Non-admin role
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await client.DeleteAsync("/api/v1/books/1");

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }


}

