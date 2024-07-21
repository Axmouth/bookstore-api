using System.Text.Json;
using BookStoreApi.Controllers;
using BookStoreApi.Models;
using BookStoreApi.Queries;
using BookStoreApi.Requests;
using BookStoreApi.Responses;
using BookStoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace BookStoreApi.Tests.Controllers;

public class BooksControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<IBookService> _mockBookService;
    private readonly BooksController _booksController;

    public BooksControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mockBookService = new Mock<IBookService>();
        _booksController = new BooksController(_mockBookService.Object);
    }

    [Fact]
    public async Task GetBooks_ReturnsOkResult_WithListOfBooks()
    {
        // Arrange
        var query = new GetBooksQuery();
        var books = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Title = "Test Book",
                    Author = "Author",
                    ISBN = "1234567890123",
                    PublishedDate = new DateOnly(2020, 1, 1),
                    Price = 10.99m,
                    Quantity = 100
                }
            };
        var pagedResult = new PagedResult<Book> { Items = books, TotalCount = books.Count };
        _mockBookService.Setup(service => service.GetBooksAsync(query)).ReturnsAsync(pagedResult);

        // Act
        var result = await _booksController.GetBooks(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnBooks = Assert.IsType<GetBooksResponse>(okResult.Value);
        Assert.Single(returnBooks.Books);
        Assert.Equal(1, returnBooks.TotalItems);
    }

    [Fact]
    public async Task GetBook_ReturnsOkResult_WithBook()
    {
        // Arrange
        var bookId = 1;
        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            Author = "Author",
            ISBN = "1234567890123",
            PublishedDate = new DateOnly(2020, 1, 1),
            Price = 10.99m,
            Quantity = 100
        };
        _mockBookService.Setup(service => service.GetBookByIdAsync(bookId)).ReturnsAsync(book);

        // Act
        var result = await _booksController.GetBook(bookId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnBook = Assert.IsType<Book>(okResult.Value);
        Assert.Equal(bookId, returnBook.Id);
    }

    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = 1;
        _mockBookService.Setup(service => service.GetBookByIdAsync(bookId)).ReturnsAsync((Book?)null);

        // Act
        var result = await _booksController.GetBook(bookId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateBook_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            ISBN = "1234567890123",
            Author = "Author",
            PublishedDate = new DateOnly(2020, 1, 1),
            Price = 10.99m,
            Quantity = 100
        };

        // Simulate model validation error
        _booksController.ModelState.AddModelError("Author", "Required");

        // Act
        var result = await _booksController.CreateBook(book);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreatedAtActionResult_WithBook()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Author = "Author",
            ISBN = "1234567890123",
            PublishedDate = new DateOnly(2020, 1, 1),
            Price = 10.99m,
            Quantity = 100
        };

        _mockBookService.Setup(service => service.CreateBookAsync(book)).Returns(Task.FromResult(book));

        // Act
        var result = await _booksController.CreateBook(book);

        // Assert
        var actionResult = Assert.IsType<ActionResult<CreateBookResponse>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(_booksController.GetBook), createdAtActionResult.ActionName);
        Assert.Equal(book.Id, ((CreateBookResponse)createdAtActionResult.Value!).Id);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent()
    {
        // Arrange
        var bookId = 1;
        _mockBookService.Setup(service => service.DeleteBookAsync(bookId)).Returns(Task.FromResult(true));

        // Act
        var result = await _booksController.DeleteBook(bookId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateBook_ReturnsConflict_WhenDuplicateISBNExists()
    {
        // Arrange
        var book = new Book
        {
            Title = "Conflict Book",
            Author = "Author",
            ISBN = "1234567890123",
            PublishedDate = new DateOnly(2021, 1, 1),
            Price = 15.99m,
            Quantity = 10
        };

        var pgException = new PostgresException("test", "test", "test", "23505");
        var mockException = new DbUpdateException("test", pgException);

        _mockBookService.Setup(s => s.CreateBookAsync(book))
            .ThrowsAsync(mockException);

        // Act
        var result = await _booksController.CreateBook(book);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result?.Result);
        var details = JsonSerializer.Deserialize<ErrorDetails>(conflictResult?.Value?.ToString()!);
        Assert.Equal("Book with conflicting ISBN or Title/Author combination exists", details?.Message);
    }

    [Fact]
    public async Task UpdateBook_ReturnsConflict_WhenDuplicateValueExists()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Title = "Original Book",
            Author = "Original Author",
            ISBN = "DuplicateISBN123",
            PublishedDate = new DateOnly(2021, 1, 1),
            Price = 20.99m,
            Quantity = 5
        };

        var pgException = new PostgresException("Duplicate ISBN", "P0001", "Duplicate ISBN", "23505");
        var mockException = new DbUpdateException("Conflict on ISBN", pgException);
        var bookRequest = UpdateBookRequest.FromBook(book);
        var bookId = book.Id ?? -1;

        _mockBookService.Setup(s => s.UpdateBookAsync(It.IsAny<int>(), It.IsAny<UpdateBookRequest>()))
            .ThrowsAsync(mockException);

        // Act
        var result = await _booksController.UpdateBook(bookId, bookRequest);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var details = JsonSerializer.Deserialize<ErrorDetails>(conflictResult?.Value?.ToString()!);
        Assert.Equal("Book with conflicting ISBN or Title/Author combination exists", details?.Message);
    }
}
