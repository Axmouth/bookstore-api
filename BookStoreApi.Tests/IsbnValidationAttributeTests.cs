using System.ComponentModel.DataAnnotations;
using BookStoreApi.Models;
using Xunit;

public class IsbnValidationAttributeTests
{
    [Theory]
    [InlineData("1234567890")]  // Valid ISBN-10
    [InlineData("1234567890123")]  // Valid ISBN-13
    public void IsbnValidation_ValidISBN_ReturnsSuccess(string isbn)
    {
        // Arrange
        var book = new Book { ISBN = isbn, Title = "Book Title", Author = "Book Author" };

        // Act
        var context = new ValidationContext(book) { MemberName = "ISBN" };
        var result = Validator.TryValidateProperty(book.ISBN, context, null);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("12345")]  // Invalid ISBN
    [InlineData("123456789012")]  // Invalid ISBN
    public void IsbnValidation_InvalidISBN_ReturnsError(string isbn)
    {
        // Arrange
        var book = new Book { ISBN = isbn, Title = "Book Title", Author = "Book Author" };

        // Act
        var context = new ValidationContext(book) { MemberName = "ISBN" };
        var result = Validator.TryValidateProperty(book.ISBN, context, null);

        // Assert
        Assert.False(result);
    }
}
