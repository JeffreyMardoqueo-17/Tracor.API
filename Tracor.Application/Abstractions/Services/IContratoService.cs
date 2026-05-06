using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

/// <summary>
/// Servicio de negocio para Contratos
/// Aplica reglas de inversión y validaciones
/// </summary>
public interface IContratoService
{
    /// <summary>
    /// Crea un nuevo contrato para un cliente
    /// Reglas:
    /// - Capital > 0
    /// - Porcentaje entre 6% y 8.50%
    /// - Cliente debe existir
    /// - Se asignan beneficiarios
    /// </summary>
    Task<ContratoResponse> CrearContratoAsync(CreateContratoRequest request);

    /// <summary>
    /// Obtiene un contrato específico con todos sus detalles
    /// </summary>
    Task<ContratoResponse?> ObtenerContratoAsync(int contratoId);

    /// <summary>
    /// Obtiene todos los contratos de un cliente
    /// </summary>
    Task<IEnumerable<ContratoListaResponse>> ObtenerContratosClienteAsync(int clienteId);

    /// <summary>
    /// Obtiene solo los contratos activos de un cliente
    /// </summary>
    Task<IEnumerable<ContratoListaResponse>> ObtenerContratosActivosClienteAsync(int clienteId);

    /// <summary>
    /// Obtiene todos los contratos activos del sistema
    /// </summary>
    Task<IEnumerable<ContratoListaResponse>> ObtenerTodosContratosActivosAsync();

    /// <summary>
    /// Actualiza información del contrato
    /// Solo campos específicos pueden editarse en contratos activos
    /// </summary>
    Task<ContratoResponse> ActualizarContratoAsync(int contratoId, UpdateContratoRequest request);

    /// <summary>
    /// Finaliza un contrato (marca como inactivo)
    /// </summary>
    Task<bool> FinalizarContratoAsync(int contratoId);

    /// <summary>
    /// Asigna beneficiarios existentes a un contrato
    /// </summary>
    Task<IEnumerable<BeneficiarioContratoResponse>> AsignarBeneficiariosAsync(int contratoId, List<AsignarBeneficiarioRequest> beneficiarios);

    /// <summary>
    /// Obtiene los beneficiarios asignados a un contrato
    /// </summary>
    Task<IEnumerable<BeneficiarioContratoResponse>> ObtenerBeneficiariosContratoAsync(int contratoId);

    /// <summary>
    /// Unifica múltiples contratos en uno solo
    /// </summary>
    Task<ContratoResponse> UnificarContratosAsync(int contratoDestinoId, List<int> contratosOrigenIds);

    /// <summary>
    /// Desunifica un contrato (lo separa de otros)
    /// </summary>
    Task<bool> DesunificarContratoAsync(int contratoId);
}
