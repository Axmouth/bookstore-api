using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Annotations;

public class IsbnValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string isbn && (isbn.Length == 10 || isbn.Length == 13) && ValidationResult.Success is not null)
        {
            return ValidationResult.Success;
        }
        return new ValidationResult(ErrorMessage ?? "Invalid ISBN format");
    }
}