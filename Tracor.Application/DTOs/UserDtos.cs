using Tradecorp.Domain.Models.Enums;
using System.Text.Json.Serialization;

namespace Tradecorp.Application.DTOs;

public record UserCreateDto(string Nombre, string Email, string Password, [property: JsonConverter(typeof(JsonStringEnumConverter))] UsuarioRol Rol);

public record UserUpdateDto(string? Nombre, string? Email, [property: JsonConverter(typeof(JsonStringEnumConverter))] UsuarioRol? Rol);

public record UserResponseDto(int Id, string Nombre, string Email, string Rol, bool Activo);

public record LoginRequestDto(string Email, string Password);
public record AuthResponseDto(string Token, DateTime ExpiresAt, UserResponseDto User);
