using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Application.Abstractions.Persistence;

public interface IContratoRelacionRepository
{
    Task<ContratoRelacion> CreateAsync(ContratoRelacion relacion);
    Task<IEnumerable<ContratoRelacion>> GetPorContratoAsync(int contratoId);
}
