using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Application.Abstractions.Security;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;
using Tradecorp.Domain.Models.Entities;
using BCrypt.Net;

namespace Tradecorp.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IJwtService _jwt;

    public UserService(IUserRepository repo, IJwtService jwt)
    {
        _repo = repo;
        _jwt = jwt;
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
    {
        var existing = await _repo.GetByEmailAsync(dto.Email);
        if (existing is not null) throw new InvalidOperationException("Email ya registrado.");

        var user = new Usuario
        {
            Nombre = dto.Nombre,
            Email = dto.Email,
            Rol = dto.Rol,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        user = await _repo.CreateAsync(user);

        return new UserResponseDto(user.Id, user.Nombre, user.Email, user.Rol, user.Activo);
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var u = await _repo.GetByIdAsync(id);
        if (u == null || !u.Activo) return null;
        return new UserResponseDto(u.Id, u.Nombre, u.Email, u.Rol, u.Activo);
    }

    public async Task<IEnumerable<UserResponseDto>> ListActiveAsync()
    {
        var list = await _repo.ListActiveAsync();
        return list.Select(u => new UserResponseDto(u.Id, u.Nombre, u.Email, u.Rol, u.Activo));
    }

    public async Task UpdateAsync(int id, UserUpdateDto dto)
    {
        var u = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Usuario no encontrado.");
        if (!u.Activo) throw new InvalidOperationException("Usuario inactivo.");

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) u.Nombre = dto.Nombre;
        if (!string.IsNullOrWhiteSpace(dto.Email)) u.Email = dto.Email;
        if (dto.Rol.HasValue) u.Rol = dto.Rol.Value;

        await _repo.UpdateAsync(u);
    }

    public async Task SoftDeleteAsync(int id)
    {
        var u = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Usuario no encontrado.");
        await _repo.SoftDeleteAsync(u);
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto dto)
    {
        var u = await _repo.GetByEmailAsync(dto.Email);
        if (u == null || !u.Activo) return null;

        var valid = BCrypt.Net.BCrypt.Verify(dto.Password, u.PasswordHash);
        if (!valid) return null;

        var token = _jwt.GenerateToken(u);
        var expires = _jwt.GetExpiryUtc();

        var userDto = new UserResponseDto(u.Id, u.Nombre, u.Email, u.Rol, u.Activo);
        return new AuthResponseDto(token, expires, userDto);
    }
}
