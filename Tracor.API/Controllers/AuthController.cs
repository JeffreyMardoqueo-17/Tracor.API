using Microsoft.AspNetCore.Mvc;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;

namespace Tradecorp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    public AuthController(IUserService userService) => _userService = userService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var auth = await _userService.AuthenticateAsync(dto);
        if (auth == null) return Unauthorized(new { message = "Credenciales inválidas" });
        return Ok(auth);
    }
}
