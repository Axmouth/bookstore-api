using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Queries;
using BookStoreApi.Services;
using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookStoreApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // GET /books
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] GetBooksQuery query)
    {
        var books = await _bookService.GetBooksAsync(query);
        return Ok(books);
    }


    // GET /books/:id
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound();
        }
        return Ok(book);
    }

    // POST /books
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _bookService.CreateBookAsync(book);
            return CreatedAtAction(nameof(GetBook), new { id = book.ID }, book);
        }
        catch (DbUpdateException dbException)
        {
            if (dbException.GetBaseException() is PostgresException pgException)
            {
                switch (pgException.SqlState)
                {
                    case "23505":
                        ModelState.AddModelError(string.Empty, "This entity exists in the database");
                        return Conflict("Error updating book.");
                    default:
                        throw;
                }
            }
            return Conflict("Error updating book.");
        }
    }

    // PUT /books/:id
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBook(int id, Book book)
    {
        if (id != book.ID)
        {
            return BadRequest();
        }

        var updated = await _bookService.UpdateBookAsync(book);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE /books/:id
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var deleted = await _bookService.DeleteBookAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
