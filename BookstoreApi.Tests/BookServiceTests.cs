using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStoreApi.Models;
using BookStoreApi.Repositories;
using BookStoreApi.Services;
using BookStoreApi.Queries;
using Moq;
using Xunit;

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
        var books = new List<Book>
        {
            new Book { ID = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" },
            new Book { ID = 2, Title = "Book 2", Author = "Author 2", ISBN = "5678" }
        };
        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.IsAny<GetBooksQuery>()))
            .ReturnsAsync(books);

        var result = await _bookService.GetBooksAsync(new GetBooksQuery());

        Assert.Equal(2, result.Count());
        Assert.Contains(result, b => b.Title == "Book 1");
        Assert.Contains(result, b => b.Title == "Book 2");
    }

    [Fact]
    public async Task GetBooksAsync_WithTitleFilter_ReturnsFilteredBooks()
    {
        var books = new List<Book>
        {
            new Book { ID = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" },
            new Book { ID = 2, Title = "Book 2", Author = "Author 2", ISBN = "5678" }
        };
        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.Is<GetBooksQuery>(q => q.Title == "Book 1")))
            .ReturnsAsync(books.Where(b => b.Title.Contains("Book 1")).ToList());

        var result = await _bookService.GetBooksAsync(new GetBooksQuery { Title = "Book 1" });

        Assert.Single(result);
        Assert.Equal("Book 1", result.First().Title);
    }

    [Fact]
    public async Task GetBooksAsync_WithAuthorFilter_ReturnsFilteredBooks()
    {
        var books = new List<Book>
        {
            new Book { ID = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" },
            new Book { ID = 2, Title = "Book 2", Author = "Author 2", ISBN = "5678" }
        };
        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.Is<GetBooksQuery>(q => q.Author == "Author 1")))
            .ReturnsAsync(books.Where(b => b.Author.Contains("Author 1")).ToList());

        var result = await _bookService.GetBooksAsync(new GetBooksQuery { Author = "Author 1" });

        Assert.Single(result);
        Assert.Equal("Author 1", result.First().Author);
    }

    [Fact]
    public async Task GetBooksAsync_WithPagination_ReturnsPagedBooks()
    {
        var books = new List<Book>
        {
            new Book { ID = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" },
            new Book { ID = 2, Title = "Book 2", Author = "Author 2", ISBN = "5678" },
            new Book { ID = 3, Title = "Book 3", Author = "Author 3", ISBN = "9012" }
        };
        _mockBookRepository.Setup(repo => repo.GetBooksAsync(It.Is<GetBooksQuery>(q => q.PageNumber == 2 && q.PageSize == 1)))
            .ReturnsAsync(books.Skip(1).Take(1).ToList());

        var result = await _bookService.GetBooksAsync(new GetBooksQuery { PageNumber = 2, PageSize = 1 });

        Assert.Single(result);
        Assert.Equal("Book 2", result.First().Title);
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsBook()
    {
        var book = new Book { ID = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" };
        _mockBookRepository.Setup(repo => repo.GetBookByIdAsync(1))
            .ReturnsAsync(book);

        var result = await _bookService.GetBookByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Book 1", result.Title);
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsNullWhenNotFound()
    {
        _mockBookRepository.Setup(repo => repo.GetBookByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Book)null);

        var result = await _bookService.GetBookByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateBookAsync_CallsRepositoryAddBook()
    {
        var book = new Book { ID = 1, Title = "Book 1", Author = "Author 1", ISBN = "1234" };

        await _bookService.CreateBookAsync(book);

        _mockBookRepository.Verify(repo => repo.CreateBookAsync(book), Times.Once);
    }

    [Fact]
    public async Task UpdateBookAsync_CallsRepositoryUpdateBook()
    {
        var book = new Book { ID = 1, Title = "Updated Book", Author = "Author 1", ISBN = "1234" };

        await _bookService.UpdateBookAsync(book);

        _mockBookRepository.Verify(repo => repo.UpdateBookAsync(book), Times.Once);
    }

    [Fact]
    public async Task DeleteBookAsync_CallsRepositoryDeleteBook()
    {
        int bookID = 1;

        await _bookService.DeleteBookAsync(bookID);

        _mockBookRepository.Verify(repo => repo.DeleteBookAsync(bookID), Times.Once);
    }
}
