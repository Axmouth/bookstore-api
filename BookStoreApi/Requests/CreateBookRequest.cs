using System.ComponentModel.DataAnnotations;
using BookStoreApi.Annotations;
using BookStoreApi.Models;

namespace BookStoreApi.Requests;

public class CreateBookRequest
{

    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Author { get; set; }

    [Required]
    [IsbnValidation(ErrorMessage = "Invalid ISBN format. ISBN should be either 10 or 13 characters long.")]
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
            Author = Author,
            ISBN = ISBN,
            Title = Title,
            Price = Price,
            PublishedDate = PublishedDate,
            Quantity = Quantity
        };
    }

}