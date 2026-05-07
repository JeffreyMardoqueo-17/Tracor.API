using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

/// <summary>
/// Interfaz del servicio de aplicación para gestionar contratos.
/// Orquesta casos de uso y valida reglas de dominio.
/// </summary>
public interface IContratoApplicationService
{
    /// <summary>
    /// Crear un nuevo contrato para un cliente.
    /// </summary>
    Task<OperacionContratoResponse> CrearContratoAsync(CreateContratoRequest request, int usuarioEjecutivoId);

    /// <summary>
    /// Crear un contrato adicional para un cliente que ya tiene contratos.
    /// </summary>
    Task<OperacionContratoResponse> CrearContratoAdicionalAsync(CreateContratoAdicionalRequest request, int usuarioEjecutivoId);

    /// <summary>
    /// Unificar múltiples contratos en uno.
    /// </summary>
    Task<OperacionContratoResponse> UnificarContratosAsync(UnificarContratosRequest request, int usuarioId);

    /// <summary>
    /// Inyectar capital a un contrato (cierra el actual, crea uno nuevo).
    /// </summary>
    Task<OperacionContratoResponse> InyectarCapitalAsync(InyectarCapitalRequest request, int usuarioId);

    /// <summary>
    /// Reinvertir ganancias en un contrato.
    /// </summary>
    Task<OperacionContratoResponse> ReinvertirGananciasAsync(ReinvertirGananciasRequest request, int usuarioId);

    /// <summary>
    /// Cambiar el porcentaje mensual de un contrato.
    /// </summary>
    Task<OperacionContratoResponse> CambiarPorcentajeAsync(CambiarPorcentajeRequest request, int usuarioId);

    /// <summary>
    /// Obtener un contrato con todos sus detalles.
    /// </summary>
    Task<ContratoResponse?> ObtenerContratoAsync(int contratoId);

    /// <summary>
    /// Obtener contratos activos de un cliente.
    /// </summary>
    Task<IEnumerable<ContratoResponse>> ObtenerContratosActivosAsync(int clienteId);

    /// <summary>
    /// Obtener historial de auditoría de un contrato.
    /// </summary>
    Task<IEnumerable<AuditoriaContratoResponse>> ObtenerAuditoriaAsync(int contratoId);
}

/// <summary>
/// Interfaz del servicio de auditoría.
/// Maneja el registro de cambios de manera desacoplada.
/// </summary>
public interface IAuditoriaContratoService
{
    /// <summary>
    /// Registrar un evento de auditoría.
    /// </summary>
    Task<long> RegistrarAsync(int contratoId, string tipoMovimiento, int usuarioId, object? valorAnterior = null, object? valorNuevo = null, string? observacion = null);
}
