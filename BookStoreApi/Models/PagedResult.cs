public class PagedResult<T>
{
    public required IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
}
