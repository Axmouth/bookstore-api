using BookStoreApi.Models;
using BookStoreApi.Repositories;
using BookStoreApi.Services;
using BookStoreApi.Queries;
using Moq;
using Xunit;
using BookStoreApi.Requests;

namespace BookStoreApi.Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly IBookService _bookService;

    public BookServiceTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _bookService = new BookService(_mockBookRepository.Object);
    }

    [Fact]
    public async Task GetBooksAsync_ReturnsBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" },
            new Book { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "5678" }
        };
        var pagedResult = new PagedResult<Book> { Items = books, TotalCount = books.Count };

        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.IsAny<GetBooksQuery>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _bookService.GetBooksAsync(new GetBooksQuery());

        // Assert
        Assert.Equal(2, result.Items.Count());
        Assert.Contains(result.Items, b => b.Title == "Book 1");
        Assert.Contains(result.Items, b => b.Title == "Book 2");
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetBooksAsync_WithTitleFilter_ReturnsFilteredBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" },
            new Book { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "5678" }
        };
        var filteredBooks = books.Where(b => b.Title.Contains("Book 1")).ToList();
        var pagedResult = new PagedResult<Book> { Items = filteredBooks, TotalCount = filteredBooks.Count };

        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.Is<GetBooksQuery>(q => q.Title == "Book 1")))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _bookService.GetBooksAsync(new GetBooksQuery { Title = "Book 1" });

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Book 1", result.Items.First().Title);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetBooksAsync_WithAuthorFilter_ReturnsFilteredBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" },
            new Book { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "5678" }
        };
        var filteredBooks = books.Where(b => b.Author.Contains("Author 1")).ToList();
        var pagedResult = new PagedResult<Book> { Items = filteredBooks, TotalCount = filteredBooks.Count };

        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.Is<GetBooksQuery>(q => q.Author == "Author 1")))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _bookService.GetBooksAsync(new GetBooksQuery { Author = "Author 1" });

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Author 1", result.Items.First().Author);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetBooksAsync_WithPagination_ReturnsPagedBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" },
            new Book { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "5678" },
            new Book { Id = 3, Title = "Book 3", Author = "Author 3", ISBN = "9012" }
        };
        var pagedBooks = books.Skip(1).Take(1).ToList();
        var pagedResult = new PagedResult<Book> { Items = pagedBooks, TotalCount = books.Count };

        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.Is<GetBooksQuery>(q => q.PageNumber == 2 && q.PageSize == 1)))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _bookService.GetBooksAsync(new GetBooksQuery { PageNumber = 2, PageSize = 1 });

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Book 2", result.Items.First().Title);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsBook()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" };
        _mockBookRepository.Setup(repo => repo.GetBookByIdAsync(1))
            .ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBookByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Book 1", result.Title);
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsNullWhenNotFound()
    {
        // Arrange
        _mockBookRepository.Setup(repo => repo.GetBookByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.GetBookByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateBookAsync_CallsRepositoryAddBook()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" };

        // Act
        await _bookService.CreateBookAsync(book);

        // Assert
        _mockBookRepository.Verify(repo => repo.CreateBookAsync(book), Times.Once);
        _mockBookRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateBookAsync_CallsRepositoryUpdateBook()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "Updated Book", Author = "Author 1", ISBN = "1234" };
        var request = new UpdateBookRequest
        {
            Title = "Updated Book",
            Author = "Author 1",
            PublishedDate = DateOnly.Parse("2021-01-01"),
            Price = 15.99m,
            Quantity = 10
        };
        var bookId = book.Id ?? throw new Exception("Null Book Id");

        _mockBookRepository.Setup(repo => repo.GetBookByIdAsync(bookId))
            .ReturnsAsync(book);

        _mockBookRepository.Setup(repo => repo.UpdateBookAsync(It.IsAny<Book>()))
            .Returns(Task.FromResult(book)!);

        _mockBookRepository.Setup(repo => repo.ExecuteInTransactionAsync(It.IsAny<Func<Task<Book>>>()))
            .Returns<Func<Task<Book>>>(async action => await action());

        // Act
        var result = await _bookService.UpdateBookAsync(bookId, request);

        // Assert
        _mockBookRepository.Verify(repo => repo.GetBookByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateBookAsync(It.Is<Book>(b => b.Title == request.Title && b.Author == request.Author)), Times.Once);
        _mockBookRepository.Verify(repo => repo.ExecuteInTransactionAsync(It.IsAny<Func<Task<Book>>>()), Times.Once);
        _mockBookRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteBookAsync_CallsRepositoryDeleteBook()
    {
        // Arrange
        int bookId = 1;

        // Act
        await _bookService.DeleteBookAsync(bookId);

        // Assert
        _mockBookRepository.Verify(repo => repo.DeleteBookAsync(bookId), Times.Once);
        _mockBookRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetBooksAsync_ReturnsEmptyList_WhenNoBooksFound()
    {
        // Arrange
        var pagedResult = new PagedResult<Book> { Items = new List<Book>(), TotalCount = 0 };
        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.IsAny<GetBooksQuery>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _bookService.GetBooksAsync(new GetBooksQuery());

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
