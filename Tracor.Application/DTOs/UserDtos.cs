using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Application.DTOs;

public record UserCreateDto(string Nombre, string Email, string Password, UsuarioRol Rol);
public record UserUpdateDto(string? Nombre, string? Email, UsuarioRol? Rol);
public record UserResponseDto(int Id, string Nombre, string Email, UsuarioRol Rol, bool Activo);

public record LoginRequestDto(string Email, string Password);
public record AuthResponseDto(string Token, DateTime ExpiresAt, UserResponseDto User);
