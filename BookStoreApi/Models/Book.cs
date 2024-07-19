using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Models;

public class Book
{
    public int ID { get; set; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Author { get; set; }

    [Required]
    public required string ISBN { get; set; }
    public DateTime PublishedDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
