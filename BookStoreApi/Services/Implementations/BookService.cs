using BookStoreApi.Models;
using BookStoreApi.Queries;
using BookStoreApi.Repositories;
using BookStoreApi.Requests;

namespace BookStoreApi.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<PagedResult<Book>> GetBooksAsync(GetBooksQuery query)
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

    public async Task<Book?> UpdateBookAsync(int id, UpdateBookRequest request)
    {
        return await _bookRepository.ExecuteInTransactionAsync(async () =>
        {
            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null)
            {
                return null;
            }

            existingBook.Title = request.Title;
            existingBook.Author = request.Author;
            existingBook.PublishedDate = request.PublishedDate;
            existingBook.Price = request.Price;
            existingBook.Quantity = request.Quantity;

            await _bookRepository.UpdateBookAsync(existingBook);

            return existingBook;
        });
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        return await _bookRepository.DeleteBookAsync(id);
    }
}

