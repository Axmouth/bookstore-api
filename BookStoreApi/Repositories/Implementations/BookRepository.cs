using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Queries;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Book>> GetBooksAsync(GetBooksQuery query)
    {
        var books = _context.Books.AsQueryable();

        if (!string.IsNullOrEmpty(query.Title))
        {
            books = books.Where(b => b.Title.Contains(query.Title));
        }

        if (!string.IsNullOrEmpty(query.Author))
        {
            books = books.Where(b => b.Author.Contains(query.Author));
        }

        var totalCount = await books.CountAsync();

        var paginatedBooks = await books
            .OrderByDescending(b => b.PublishedDate)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<Book>
        {
            Items = paginatedBooks,
            TotalCount = totalCount
        };
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateBookAsync(Book book)
    {
        var existingBook = await _context.Books.FindAsync(book.Id);
        if (existingBook == null)
        {
            return null;
        }

        _context.Entry(existingBook).CurrentValues.SetValues(book);
        await _context.SaveChangesAsync();
        return existingBook;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
