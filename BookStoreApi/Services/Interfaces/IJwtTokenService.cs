using Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace BookStoreApi.Services;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(AppUser user, UserManager<AppUser> userManager);
}