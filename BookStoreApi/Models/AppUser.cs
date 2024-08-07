using Microsoft.AspNetCore.Identity;

namespace Identity.Models;

public class AppUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
