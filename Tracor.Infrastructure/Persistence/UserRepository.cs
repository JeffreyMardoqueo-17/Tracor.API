using Microsoft.EntityFrameworkCore;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Infrastructure.Data;

namespace Tradecorp.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<Usuario?> GetByIdAsync(int id) =>
        await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Usuario?> GetByEmailAsync(string email) =>
        await _db.Usuarios.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

    public async Task<Usuario> CreateAsync(Usuario user)
    {
        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(Usuario user)
    {
        _db.Usuarios.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Usuario user)
    {
        user.Activo = false;
        await UpdateAsync(user);
    }

    public async Task<IEnumerable<Usuario>> ListActiveAsync() =>
        await _db.Usuarios.Where(x => x.Activo).ToListAsync();
}
