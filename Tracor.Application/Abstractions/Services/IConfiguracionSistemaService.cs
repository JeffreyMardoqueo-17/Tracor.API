using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

public interface IConfiguracionSistemaService
{
    Task<SistemaConfiguracionDto> GetSistemaAsync();
    Task<SistemaConfiguracionDto> UpdateSistemaAsync(UpdateSistemaConfiguracionRequestDto dto);
    Task<IReadOnlyCollection<CortePagoDto>> GetCortesAsync();
    Task<CortePagoDto> CreateCorteAsync(UpsertCortePagoRequestDto dto);
    Task<CortePagoDto> UpdateCorteAsync(int corteId, UpsertCortePagoRequestDto dto);
    Task DeleteCorteAsync(int corteId);
}
