using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Persistence;

public interface IContratoRepository
{
    Task<Tradecorp.Domain.Models.Entities.Contrato> CreateAsync(Tradecorp.Domain.Models.Entities.Contrato contrato);
    Task<Tradecorp.Domain.Models.Entities.Contrato?> GetByIdAsync(int id);
    Task<Tradecorp.Domain.Models.Entities.Contrato?> GetByNumeroAsync(string numero);
    Task<IEnumerable<Tradecorp.Domain.Models.Entities.Contrato>> GetActivosPorClienteAsync(int clienteId);
    Task<IEnumerable<Tradecorp.Domain.Models.Entities.Contrato>> GetPorClienteAsync(int clienteId);
    Task<Tradecorp.Domain.Models.Entities.Contrato> UpdateAsync(Tradecorp.Domain.Models.Entities.Contrato contrato);
    Task<bool> DeleteAsync(int id);
    Task<string> GetProximoNumeroContratoAsync();

    Task<bool> ClienteExisteAsync(int clienteId);
    Task<bool> ContratoExisteAsync(int contratoId);
    Task<bool> ContratoActivoAsync(int contratoId);
    Task<int> ObtenerClienteIdContratoAsync(int contratoId);
    Task<string> GenerarNumeroContratoAsync(int clienteId);

    Task<int> CrearContratoAsync(CreateContratoRequest request, int usuarioId, string numeroContrato);
    Task<bool> ActualizarContratoAsync(int contratoId, UpdateContratoRequest request, int usuarioId);
    Task<bool> FinalizarContratoAsync(int contratoId, int usuarioId, string? observacion);
    Task<bool> RegistrarReinversionAsync(int contratoId, ReinversionContratoRequest request, int usuarioId);
    Task<bool> RegistrarInyeccionCapitalAsync(int contratoId, InyeccionCapitalRequest request, int usuarioId);
    Task<bool> UnificarContratosAsync(int contratoDestinoId, UnificarContratosRequest request, int usuarioId);
    Task<bool> DesunificarContratoAsync(int contratoId, DesunificarContratoRequest request, int usuarioId);
    Task<IEnumerable<BeneficiarioContratoResponse>> AsignarBeneficiariosAsync(int contratoId, List<AsignarBeneficiarioRequest> beneficiarios, int usuarioId);

    Task<IEnumerable<ContratoListaResponse>> ObtenerContratosAsync(ContratoFiltroRequest? filtro);
    Task<IEnumerable<ContratoListaResponse>> ObtenerContratosClienteAsync(int clienteId);
    Task<IEnumerable<ContratoListaResponse>> ObtenerContratosActivosClienteAsync(int clienteId);
    Task<IEnumerable<ContratoListaResponse>> ObtenerTodosContratosActivosAsync();
    Task<ContratoResponse?> ObtenerContratoDetalleAsync(int contratoId);
    Task<IEnumerable<BeneficiarioContratoResponse>> ObtenerBeneficiariosContratoAsync(int contratoId);
    Task<IEnumerable<HistorialFinancieroItemResponse>> ObtenerHistorialFinancieroAsync(int contratoId);
    Task<IEnumerable<ContratoEventoResponse>> ObtenerEventosAsync(int contratoId);
    Task<IEnumerable<AuditoriaContratoResponse>> ObtenerAuditoriaAsync(int contratoId);
}
