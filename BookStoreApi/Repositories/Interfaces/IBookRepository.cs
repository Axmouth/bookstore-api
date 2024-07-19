using BookStoreApi.Models;
using BookStoreApi.Queries;

namespace BookStoreApi.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetBooksAsync(GetBooksQuery query);
    Task<Book?> GetBookByIdAsync(int id);
    Task AddBookAsync(Book book);
    Task UpdateBookAsync(Book book);
    Task DeleteBookAsync(int id);
}

