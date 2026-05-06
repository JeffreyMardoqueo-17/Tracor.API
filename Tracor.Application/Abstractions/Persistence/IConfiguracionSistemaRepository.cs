using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Application.Abstractions.Persistence;

public interface IConfiguracionSistemaRepository
{
    Task<ConfiguracionSistema?> GetActivaAsync();
    Task<ConfiguracionSistema> CreateDefaultAsync();
    Task UpdateAsync(ConfiguracionSistema configuracion);
    Task<ConfiguracionCortePago?> GetCorteByIdAsync(int corteId);
    Task<ConfiguracionCortePago> AddCorteAsync(ConfiguracionCortePago corte);
    Task UpdateCorteAsync(ConfiguracionCortePago corte);
    Task DeleteCorteAsync(ConfiguracionCortePago corte);
}
