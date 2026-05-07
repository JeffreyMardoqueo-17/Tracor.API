using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Application.Abstractions.Persistence;

public interface IAuditoriaContratoRepository
{
    Task<AuditoriaContrato> CreateAsync(AuditoriaContrato auditoria);
    Task<IEnumerable<AuditoriaContrato>> GetPorContratoAsync(int contratoId);
}
