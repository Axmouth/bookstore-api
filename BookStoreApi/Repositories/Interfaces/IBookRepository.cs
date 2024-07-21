using BookStoreApi.Models;
using BookStoreApi.Queries;

namespace BookStoreApi.Repositories;

public interface IBookRepository
{
    Task<PagedResult<Book>> GetBooksAsync(GetBooksQuery query);
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> CreateBookAsync(Book book);
    Task<Book?> UpdateBookAsync(Book book);
    Task<bool> DeleteBookAsync(int id);
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
}

