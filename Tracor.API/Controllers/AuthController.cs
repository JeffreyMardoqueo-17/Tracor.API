using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;

namespace Tradecorp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string AccessTokenCookieName = "access_token";
    private readonly IUserService _userService;
    public AuthController(IUserService userService) => _userService = userService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var auth = await _userService.AuthenticateAsync(dto);
        if (auth == null) return Unauthorized(new { message = "Credenciales inválidas" });

        AppendAccessTokenCookie(auth.Token, auth.ExpiresAt);

        return Ok(new
        {
            auth.ExpiresAt,
            auth.User
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps || !Request.Host.Host.Contains("localhost"),
            SameSite = Request.IsHttps || !Request.Host.Host.Contains("localhost") ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/"
        });

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(rawUserId, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetByIdAsync(userId);
        return user is null ? Unauthorized() : Ok(user);
    }

    private void AppendAccessTokenCookie(string token, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(AccessTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps || !Request.Host.Host.Contains("localhost"),
            SameSite = Request.IsHttps || !Request.Host.Host.Contains("localhost") ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = expiresAtUtc,
            Path = "/"
        });
    }
}
