using System.ComponentModel.DataAnnotations;
using BookStoreApi.Models;

namespace BookStoreApi.Requests;

public class UpdateBookRequest
{

    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Author { get; set; }

    [Required]
    public required string ISBN { get; set; }

    [Required]
    public DateOnly PublishedDate { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public Book ToBook()
    {
        return new Book
        {
            Author = Author,
            ISBN = ISBN,
            Title = Title,
            Price = Price,
            PublishedDate = PublishedDate,
            Quantity = Quantity
        };
    }

    public static UpdateBookRequest FromBook(Book book)
    {
        return new UpdateBookRequest
        {
            Author = book.Author,
            ISBN = book.ISBN,
            Title = book.Title,
            Price = book.Price,
            PublishedDate = book.PublishedDate,
            Quantity = book.Quantity
        };
    }
}