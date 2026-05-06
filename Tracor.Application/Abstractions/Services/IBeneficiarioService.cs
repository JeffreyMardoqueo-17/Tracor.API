using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

/// <summary>
/// Interfaz del servicio de negocios para Beneficiarios
/// Aplica regla de negocio: porcentajes de beneficiarios activos deben sumar 100%
/// </summary>
public interface IBeneficiarioService
{
    /// <summary>
    /// Crea un nuevo beneficiario para un cliente
    /// Valida que no se exceda el 100% de asignación
    /// </summary>
    Task<ClienteBeneficiarioResponse> CrearBeneficiarioAsync(int clienteId, CreateClienteBeneficiarioRequest request);

    /// <summary>
    /// Obtiene un beneficiario específico
    /// </summary>
    Task<ClienteBeneficiarioResponse?> ObtenerBeneficiarioAsync(int beneficiarioId);

    /// <summary>
    /// Obtiene todos los beneficiarios de un cliente
    /// </summary>
    Task<IEnumerable<ClienteBeneficiarioResponse>> ObtenerBeneficiariosClienteAsync(int clienteId);

    /// <summary>
    /// Obtiene solo beneficiarios activos de un cliente
    /// </summary>
    Task<IEnumerable<ClienteBeneficiarioResponse>> ObtenerBeneficiariosActivosAsync(int clienteId);

    /// <summary>
    /// Actualiza información de un beneficiario
    /// Valida que la suma de porcentajes no exceda 100%
    /// </summary>
    Task<ClienteBeneficiarioResponse> ActualizarBeneficiarioAsync(int beneficiarioId, UpdateClienteBeneficiarioRequest request);

    /// <summary>
    /// Valida si los beneficiarios de un cliente suman exactamente 100%
    /// </summary>
    Task<ClienteBeneficiarioValidacionResponse> ValidarBeneficiariosAsync(int clienteId);

    /// <summary>
    /// Registra el fallecimiento de un beneficiario
    /// Guarda historial con porcentaje y datos al momento del fallecimiento
    /// </summary>
    Task<bool> RegistrarFallecimientoBeneficiarioAsync(int beneficiarioId, RegistrarFallecimientoBeneficiarioRequest request);

    /// <summary>
    /// Desactiva un beneficiario
    /// </summary>
    Task<bool> DesactivarBeneficiarioAsync(int beneficiarioId, string? razon);

    /// <summary>
    /// Obtiene el historial de cambios de un beneficiario
    /// </summary>
    Task<IEnumerable<ClienteBeneficiarioHistoricoResponse>> ObtenerHistoricoBeneficiarioAsync(int beneficiarioId);

    /// <summary>
    /// Obtiene el historial completo de beneficiarios de un cliente
    /// </summary>
    Task<IEnumerable<ClienteBeneficiarioHistoricoResponse>> ObtenerHistoricoClienteAsync(int clienteId);

    /// <summary>
    /// Calcula el porcentaje disponible para asignar a nuevos beneficiarios
    /// Retorna: 100 - suma de activos
    /// </summary>
    Task<decimal> ObtenerPorcentajeDisponibleAsync(int clienteId);
}

/// <summary>
/// DTO de respuesta para historial de beneficiarios
/// </summary>
public class ClienteBeneficiarioHistoricoResponse
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? DUI { get; set; }
    public decimal PorcentajeAsignado { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public string? Notas { get; set; }
}
