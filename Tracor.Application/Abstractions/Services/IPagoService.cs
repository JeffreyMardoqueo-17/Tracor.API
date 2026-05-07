using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

/// <summary>
/// Interface para servicios de pagos y cálculos
/// </summary>
public interface IPagoService
{
    Task<CalculoGananciaResponse> CalcularGananciaAsync(CalculoGananciaRequest request);
    Task<IEnumerable<PagoResponse>> ObtenerPagosContratoAsync(int contratoId);
    Task<PagoResponse> RegistrarDecisionAsync(DecisionPagoRequest request);
    Task<PagoResponse?> ObtenerProximoPagoAsync(int contratoId);
}
