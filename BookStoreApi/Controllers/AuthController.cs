using BookStoreApi.Requests;
using BookStoreApi.Responses;
using BookStoreApi.Services;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            return Unauthorized(new ErrorDetails { Message = "Invalid username or password", StatusCode = StatusCodes.Status401Unauthorized });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new ErrorDetails { Message = "Invalid username or password", StatusCode = StatusCodes.Status401Unauthorized });
        }

        var token = await _jwtTokenService.GenerateTokenAsync(user, _userManager);
        return Ok(new LoginResponse { Token = token });
    }
}

