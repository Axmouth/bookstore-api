using BookStoreApi.Models;
using BookStoreApi.Queries;

namespace BookStoreApi.Services;

public interface IBookService
{
    Task<IEnumerable<Book>> GetBooksAsync(GetBooksQuery query);
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> CreateBookAsync(Book book);
    Task<Book?> UpdateBookAsync(Book book);
    Task<bool> DeleteBookAsync(int id);
}

