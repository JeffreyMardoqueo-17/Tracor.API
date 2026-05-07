using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

public interface IContratoService
{
    Task<ContratoResponse> CrearContratoAsync(CreateContratoRequest request, int usuarioId);
    Task<ContratoResponse?> ObtenerContratoAsync(int contratoId);
    Task<IEnumerable<ContratoListaResponse>> ObtenerContratosAsync(ContratoFiltroRequest? filtro);
    Task<IEnumerable<ContratoListaResponse>> ObtenerContratosClienteAsync(int clienteId);
    Task<IEnumerable<ContratoListaResponse>> ObtenerContratosActivosClienteAsync(int clienteId);
    Task<IEnumerable<ContratoListaResponse>> ObtenerTodosContratosActivosAsync();
    Task<ContratoResponse> ActualizarContratoAsync(int contratoId, UpdateContratoRequest request, int usuarioId);
    Task<bool> FinalizarContratoAsync(int contratoId, int usuarioId, string? observacion = null);
    Task<IEnumerable<BeneficiarioContratoResponse>> AsignarBeneficiariosAsync(int contratoId, List<AsignarBeneficiarioRequest> beneficiarios, int usuarioId);
    Task<IEnumerable<BeneficiarioContratoResponse>> ObtenerBeneficiariosContratoAsync(int contratoId);
    Task<ContratoResponse> RegistrarReinversionAsync(int contratoId, ReinversionContratoRequest request, int usuarioId);
    Task<ContratoResponse> RegistrarInyeccionCapitalAsync(int contratoId, InyeccionCapitalRequest request, int usuarioId);
    Task<ContratoResponse> UnificarContratosAsync(int contratoDestinoId, UnificarContratosRequest request, int usuarioId);
    Task<bool> DesunificarContratoAsync(int contratoId, DesunificarContratoRequest request, int usuarioId);
    Task<IEnumerable<HistorialFinancieroItemResponse>> ObtenerHistorialFinancieroAsync(int contratoId);
    Task<IEnumerable<ContratoEventoResponse>> ObtenerEventosAsync(int contratoId);
    Task<IEnumerable<AuditoriaContratoResponse>> ObtenerAuditoriaAsync(int contratoId);
}
