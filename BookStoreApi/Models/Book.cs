using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Models;

[Index(nameof(ID), IsUnique = true)]
[Index(nameof(Title))]
[Index(nameof(Author))]
[Index(nameof(ISBN), IsUnique = true)]
[Index(nameof(Title), nameof(Author), IsUnique = true)]
public class Book
{
    public int ID { get; set; }

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

}
