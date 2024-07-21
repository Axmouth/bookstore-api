using System.Net;
using System.Text.Json;
using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Queries;
using BookStoreApi.Requests;
using BookStoreApi.Responses;
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
    public async Task<ActionResult<CreateBookResponse>> CreateBook(Book book)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _bookService.CreateBookAsync(book);
            return CreatedAtAction(nameof(GetBook), new { id = created.Id }, CreateBookResponse.FromBook(book));
        }
        catch (DbUpdateException dbException)
        {
            if (dbException.GetBaseException() is PostgresException pgException)
            {
                if (pgException.SqlState == "23505")
                {
                    return Conflict(new ErrorDetails
                    {
                        StatusCode = (int)HttpStatusCode.Conflict,
                        Message = "Book with conflicting ISBN or Title/Author combination exists"
                    });
                }
            }
            throw;
        }
    }

    // PUT /books/:id
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateBookResponse>> UpdateBook(int id, [FromBody] UpdateBookRequest request)
    {
        try
        {
            Book book = request.ToBook();
            book.Id = id;
            var updated = await _bookService.UpdateBookAsync(book);

            if (updated is null)
            {
                return NotFound();
            }

            return Ok(UpdateBookResponse.FromBook(updated));
        }
        catch (DbUpdateException dbException)
        {
            if (dbException.GetBaseException() is PostgresException pgException)
            {
                if (pgException.SqlState == "23505")
                {
                    return Conflict(new ErrorDetails
                    {
                        StatusCode = (int)HttpStatusCode.Conflict,
                        Message = "Book with conflicting ISBN or Title/Author combination exists"
                    });
                }
            }
            throw;
        }
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
