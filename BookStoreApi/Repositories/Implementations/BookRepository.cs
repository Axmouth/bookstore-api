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

    public async Task<IEnumerable<Book>> GetBooksAsync(GetBooksQuery query)
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

        return await books
            .OrderByDescending(b => b.PublishedDate)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
    }

    public async Task CreateBookAsync(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateBookAsync(Book book)
    {
        var existingBook = await _context.Books.FindAsync(book.ID);
        if (existingBook == null)
        {
            return false;
        }

        _context.Entry(existingBook).CurrentValues.SetValues(book);
        await _context.SaveChangesAsync();
        return true;
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
}
