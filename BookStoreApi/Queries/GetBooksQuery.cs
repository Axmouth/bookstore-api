using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Queries;

public class GetBooksQuery
{
    public string? Title { get; set; }

    public string? Author { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
