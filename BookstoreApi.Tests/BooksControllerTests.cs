using BookStoreApi.Controllers;
using BookStoreApi.Models;
using BookStoreApi.Queries;
using BookStoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BookStoreApi.Tests.Controllers;

public class BooksControllerTests
{
    private readonly Mock<IBookService> _mockBookService;
    private readonly BooksController _booksController;

    public BooksControllerTests()
    {
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
                    ID = 1,
                    Title = "Test Book",
                    Author = "Author",
                    ISBN = "1234567890123",
                    PublishedDate = new DateOnly(2020, 1, 1),
                    Price = 10.99m,
                    Quantity = 100
                }
            };
        _mockBookService.Setup(service => service.GetBooksAsync(query)).ReturnsAsync(books);

        // Act
        var result = await _booksController.GetBooks(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnBooks = Assert.IsType<List<Book>>(okResult.Value);
        Assert.Single(returnBooks);
    }

    [Fact]
    public async Task GetBook_ReturnsOkResult_WithBook()
    {
        // Arrange
        var bookId = 1;
        var book = new Book
        {
            ID = bookId,
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
        Assert.Equal(bookId, returnBook.ID);
    }

    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = 1;
        _mockBookService.Setup(service => service.GetBookByIdAsync(bookId)).ReturnsAsync((Book)null);

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
            ID = 1,
            Title = "Test Book",
            Author = "Author",
            ISBN = "1234567890123",
            PublishedDate = new DateOnly(2020, 1, 1),
            Price = 10.99m,
            Quantity = 100
        };

        _mockBookService.Setup(service => service.CreateBookAsync(book)).Returns(Task.CompletedTask);

        // Act
        var result = await _booksController.CreateBook(book);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Book>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(_booksController.GetBook), createdAtActionResult.ActionName);
        Assert.Equal(book.ID, ((Book)createdAtActionResult.Value).ID);
    }

    [Fact]
    public async Task UpdateBook_ReturnsBadRequest_WhenIdDoesNotMatch()
    {
        // Arrange
        var bookId = 1;
        var book = new Book
        {
            ID = 2,
            Title = "Updated Test Book",
            Author = "Updated Author",
            ISBN = "1234567890123",
            PublishedDate = new DateOnly(2020, 1, 1),
            Price = 10.99m,
            Quantity = 100
        };

        // Act
        var result = await _booksController.PutBook(bookId, book);

        // Assert
        Assert.IsType<BadRequestResult>(result);
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
    public async Task CreateBook_ReturnsConflict_WhenBookWithSameTitleOrIsbnExists()
    {
        // Arrange
        var book = new Book
        {
            ID = 1,
            Title = "Test Book",
            Author = "Author",
            ISBN = "1234567890123",
            PublishedDate = new DateOnly(2020, 1, 1),
            Price = 10.99m,
            Quantity = 100
        };

        _mockBookService.Setup(service => service.CreateBookAsync(book))
            .ThrowsAsync(new DbUpdateException()); // Simulate a conflict

        // Act
        var result = await _booksController.CreateBook(book);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("Error updating book.", conflictResult.Value);
    }

}

