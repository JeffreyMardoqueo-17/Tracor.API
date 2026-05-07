using Tradecorp.Domain.Models.Entities;
using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Persistence;

/// <summary>
/// Interface para repositorio de pagos
/// </summary>
public interface IPagoRepository
{
    Task<Pago?> GetByIdAsync(int id);
    Task<IEnumerable<Pago>> GetByContratoIdAsync(int contratoId);
    Task<Pago?> GetProximoPagoAsync(int contratoId);
    Task<IEnumerable<Pago>> GetAllAsync();
}
