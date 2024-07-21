using BookStoreApi.Data;
using BookStoreApi.Models;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class TestDataSeeder
{
    public static string TestUserName = "testuser";
    public static string TestUserPassword = "Password123!";
    public static string TestUserEmail = "testuser@example.com";

    public static async Task SeedTestData(IServiceScope scope, AppDbContext dbContext)
    {

        try
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            if (!await dbContext.Users.AnyAsync())
            {
                var newUser = new AppUser
                {
                    UserName = TestUserName,
                    Email = TestUserEmail,
                    FirstName = "App",
                    LastName = "User"
                };
                await userManager.CreateAsync(newUser, TestUserPassword);
            }

            if (!await dbContext.Books.AnyAsync())
            {
                await dbContext.Books.AddRangeAsync(
                    new Book
                    {
                        Id = 1,
                        Title = "Initial Test Book",
                        Author = "Initial Author",
                        ISBN = "1234567890123",
                        PublishedDate = DateOnly.Parse("1920-01-01"),
                        Price = 15.99m,
                        Quantity = 10
                    },
                    new Book
                    {
                        Id = 2,
                        Title = "Second Test Book",
                        Author = "Second Author",
                        ISBN = "1234567890987",
                        PublishedDate = DateOnly.Parse("2001-01-01"),
                        Price = 14.99m,
                        Quantity = 9
                    },
                    new Book
                    {
                        Id = 3,
                        Title = "Third Test Book",
                        Author = "Second Author",
                        ISBN = "1234567890456",
                        PublishedDate = DateOnly.Parse("2022-01-01"),
                        Price = 13.99m,
                        Quantity = 8
                    },
                    new Book
                    {
                        Id = 4,
                        Title = "Fourth Test Book",
                        Author = "Third Author Filter Test",
                        ISBN = "1234561230123",
                        PublishedDate = DateOnly.Parse("2023-01-01"),
                        Price = 11.99m,
                        Quantity = 5
                    },
                    new Book
                    {
                        Id = 5,
                        Title = "Fifth Test Book",
                        Author = "Fourth Author",
                        ISBN = "0987654321123",
                        PublishedDate = DateOnly.Parse("2019-05-01"),
                        Price = 10.99m,
                        Quantity = 4
                    },
                    new Book
                    {
                        Id = 6,
                        Title = "First Filtered Test Book",
                        Author = "Fourth Author",
                        ISBN = "1237654321123",
                        PublishedDate = DateOnly.Parse("2018-04-01"),
                        Price = 21.99m,
                        Quantity = 34
                    },
                    new Book
                    {
                        Id = 7,
                        Title = "Second Filtered Test Book",
                        Author = "Fourth Author",
                        ISBN = "7897654561123",
                        PublishedDate = DateOnly.Parse("2017-07-01"),
                        Price = 22.99m,
                        Quantity = 23
                    }
                );

                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
