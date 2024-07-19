using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Queries;
using BookStoreApi.Services;
using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // GET /books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] GetBooksQuery query)
    {
        var books = await _bookService.GetBooksAsync(query);
        return Ok(books);
    }


    // GET /books/:id
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
    [HttpPost]
    public async Task<ActionResult<Book>> PostBook(int id)
    {
        await _bookService.DeleteBookAsync(id);
        return NoContent();
    }

    // PUT /books/:id
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBook(int id, Book book)
    {
        if (id != book.ID)
        {
            return BadRequest();
        }
        await _bookService.UpdateBookAsync(book);
        return NoContent();
    }

    // DELETE /books/:id
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        await _bookService.DeleteBookAsync(id);
        return NoContent();
    }
}
