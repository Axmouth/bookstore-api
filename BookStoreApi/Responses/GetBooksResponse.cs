
using BookStoreApi.Models;

namespace BookStoreApi.Responses;

public class GetBooksResponse
{
    public required GetBookResponse[] Books { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int Total { get; set; }

    public static GetBooksResponse FromBooks(Book[] books, int pageNumber, int pageSize, int total)
    {
        return new GetBooksResponse
        {
            Books = books.Select(GetBookResponse.FromBook).ToArray(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            Total = total
        };
    }
}
