namespace BookStoreApi.Options;

public class BookStoreConfiguration
{
    public required PostgreSQLSettings PostgreSQL { get; set; }
}