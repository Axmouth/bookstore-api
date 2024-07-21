
using BookStoreApi.Models;

namespace BookStoreApi.Responses;

public class UpdateBookResponse
{
    public int? Id { get; set; }

    public required string Title { get; set; }

    public required string Author { get; set; }

    public required string ISBN { get; set; }

    public DateOnly PublishedDate { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public static UpdateBookResponse FromBook(Book book)
    {
        return new UpdateBookResponse
        {
            Id = book.Id,
            Author = book.Author,
            ISBN = book.ISBN,
            Title = book.Title,
            Price = book.Price,
            PublishedDate = book.PublishedDate,
            Quantity = book.Quantity
        };
    }

}
