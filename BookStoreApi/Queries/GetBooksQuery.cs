namespace BookStoreApi.Queries;

public class GetBooksQuery
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
