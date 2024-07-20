using BookStoreApi.Data;
using BookStoreApi.Models;

public static class TestDataSeeder
{
    public static void SeedTestData(AppDbContext dbContext)
    {
        // Check if data already exists to prevent re-seeding
        if (!dbContext.Books.Any())
        {
            dbContext.Books.Add(new Book
            {
                ID = 1,
                Title = "Initial Test Book",
                Author = "Initial Author",
                ISBN = "1234567890123",
                PublishedDate = DateOnly.Parse("2020-01-01"),
                Price = 15.99m,
                Quantity = 10
            });
            dbContext.Books.Add(new Book
            {
                ID = 2,
                Title = "Second Test Book",
                Author = "Second Author",
                ISBN = "1234567890987",
                PublishedDate = DateOnly.Parse("2021-01-01"),
                Price = 14.99m,
                Quantity = 9
            });
            dbContext.Books.Add(new Book
            {
                ID = 3,
                Title = "Third Test Book",
                Author = "Second Author",
                ISBN = "1234567890456",
                PublishedDate = DateOnly.Parse("2022-01-01"),
                Price = 13.99m,
                Quantity = 8
            });
            dbContext.Books.Add(new Book
            {
                ID = 4,
                Title = "Fourth Test Book",
                Author = "Third Author",
                ISBN = "1234561230123",
                PublishedDate = DateOnly.Parse("2023-01-01"),
                Price = 11.99m,
                Quantity = 5
            });

            dbContext.SaveChanges();
        }
    }
}
