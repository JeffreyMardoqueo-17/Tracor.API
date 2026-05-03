using Tradecorp.Domain.Models.Entities;
namespace Tradecorp.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario> CreateAsync(Usuario user);
    Task UpdateAsync(Usuario user);
    Task SoftDeleteAsync(Usuario user);
    Task<IEnumerable<Usuario>> ListActiveAsync();
}
