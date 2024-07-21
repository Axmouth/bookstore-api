using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Models;

[Index(nameof(Id), IsUnique = true)]
[Index(nameof(Title))]
[Index(nameof(Author))]
[Index(nameof(ISBN), IsUnique = true)]
[Index(nameof(Title), nameof(Author), IsUnique = true)]
public class Book
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }

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
