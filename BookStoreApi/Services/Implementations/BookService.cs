using BookStoreApi.Models;
using BookStoreApi.Queries;
using BookStoreApi.Repositories;

namespace BookStoreApi.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<Book>> GetBooksAsync(GetBooksQuery query)
    {
        return await _bookRepository.GetBooksAsync(query);
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _bookRepository.GetBookByIdAsync(id);
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        return await _bookRepository.CreateBookAsync(book);
    }

    public async Task<Book?> UpdateBookAsync(Book book)
    {
        return await _bookRepository.UpdateBookAsync(book);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        return await _bookRepository.DeleteBookAsync(id);
    }
}

