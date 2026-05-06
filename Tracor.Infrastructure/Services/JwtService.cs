using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tradecorp.Application.Abstractions.Security;
using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _cfg;
    private readonly string _key = string.Empty;
    private readonly string _issuer = string.Empty;
    private readonly string _audience = string.Empty;
    private readonly int _expireHours;

    public JwtService(IConfiguration cfg)
    {
        _cfg = cfg;
        _key = cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
        _issuer = cfg["Jwt:Issuer"] ?? "Tradecorp";
        _audience = cfg["Jwt:Audience"] ?? "TradecorpClients";
        _expireHours = int.TryParse(cfg["Jwt:ExpireHours"], out var h) ? h : 24;
    }

    public string GenerateToken(Usuario user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim(ClaimTypes.Role, user.Rol.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(_expireHours);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiryUtc() => DateTime.UtcNow.AddHours(_expireHours);
}
