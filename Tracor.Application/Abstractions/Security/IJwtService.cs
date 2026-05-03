using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Application.Abstractions.Security;

public interface IJwtService
{
    string GenerateToken(Usuario user);
    DateTime GetExpiryUtc();
}
