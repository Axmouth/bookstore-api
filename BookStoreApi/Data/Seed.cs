using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using BookStoreApi.Data;
using BookStoreApi.Models;
using Identity.Models;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext dbContext)
    {
        // Seed roles and admin user
        await SeedRolesAndAdminUserAsync(serviceProvider, userManager, roleManager);

        // Check initialization status
        if (!await IsDatabaseInitializedAsync(dbContext))
        {
            // Seed books from CSV
            await SeedBooksFromCsvAsync(dbContext);

            // Set initialization status
            await SetDatabaseInitializedAsync(dbContext);
        }
    }

    private static async Task<bool> IsDatabaseInitializedAsync(AppDbContext dbContext)
    {
        return await dbContext.InitializationStatus.AnyAsync();
    }

    private static async Task SetDatabaseInitializedAsync(AppDbContext dbContext)
    {
        dbContext.InitializationStatus.Add(new InitializationStatus { Id = 1, IsInitialized = true });
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRolesAndAdminUserAsync(IServiceProvider serviceProvider, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = ["Admin", "User"];
        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var admin = new AppUser
        {
            UserName = "admin",
            Email = "admin@bookstore.com",
            FirstName = "Admin",
            LastName = "User"
        };

        string adminPassword = "Admin@123";

        var user = await userManager.FindByEmailAsync(admin.Email);

        if (user == null)
        {
            var createAdmin = await userManager.CreateAsync(admin, adminPassword);
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }

    private static async Task SeedBooksFromCsvAsync(AppDbContext dbContext)
    {
        // Check if books already exist
        if (dbContext.Books.Any())
        {
            return; // DB has been seeded
        }

        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        using (var reader = new StreamReader("books.csv"))
        using (var csv = new CsvReader(reader, configuration))
        {
            var books = csv.GetRecords<Book>().ToList();
            await dbContext.Books.AddRangeAsync(books);
            await dbContext.SaveChangesAsync();
        }
    }
}
