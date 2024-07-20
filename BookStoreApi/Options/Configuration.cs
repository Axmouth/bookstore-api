namespace BookStoreApi.Options;

public class BookStoreConfiguration
{
    public required AdminSettings AdminSettings { get; set; }
    public required PostgreSQLSettings PostgreSQL { get; set; }
    public required JwtSettings JwtSettings { get; set; }
}