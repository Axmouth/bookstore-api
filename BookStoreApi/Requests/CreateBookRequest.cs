using System.ComponentModel.DataAnnotations;
using BookStoreApi.Models;

namespace BookStoreApi.Requests;

public class CreateBookRequest
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

    public Book FromBook()
    {
        return new Book
        {
            Author = this.Author,
            ISBN = this.ISBN,
            Title = this.Title,
            Price = this.Price,
            PublishedDate = this.PublishedDate,
            Quantity = this.Quantity
        };
    }

}