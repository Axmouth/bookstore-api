using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.Requests;

public class LoginRequest
{
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string Password { get; set; }
}