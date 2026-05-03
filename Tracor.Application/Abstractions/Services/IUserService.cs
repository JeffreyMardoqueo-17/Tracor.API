using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

public interface IUserService
{
    Task<UserResponseDto> CreateAsync(UserCreateDto dto);
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<UserResponseDto>> ListActiveAsync();
    Task UpdateAsync(int id, UserUpdateDto dto);
    Task SoftDeleteAsync(int id);
    Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto dto);
}
