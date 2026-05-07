using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Application.Abstractions.Persistence;

public interface IContratoBeneficiarioRepository
{
    Task<ContratoBeneficiario> CreateAsync(ContratoBeneficiario beneficiario);
    Task<IEnumerable<ContratoBeneficiario>> GetPorContratoAsync(int contratoId);
    Task<decimal> GetSumaPorcentajesAsync(int contratoId);
    Task<bool> DeleteAsync(int id);
}
