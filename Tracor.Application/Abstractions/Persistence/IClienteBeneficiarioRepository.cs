using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Application.Abstractions.Persistence;

/// <summary>
/// Interfaz del repositorio para acceder a datos de Beneficiarios
/// Implementa el patrón Repository con SOLID principles
/// Regla de negocio: Los porcentajes de beneficiarios activos deben sumar 100%
/// </summary>
public interface IClienteBeneficiarioRepository
{
    /// <summary>
    /// Obtiene un beneficiario por su ID
    /// </summary>
    Task<ClienteBeneficiario?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todos los beneficiarios de un cliente
    /// </summary>
    Task<IEnumerable<ClienteBeneficiario>> GetByClienteIdAsync(int clienteId);

    /// <summary>
    /// Obtiene solo los beneficiarios activos de un cliente
    /// </summary>
    Task<IEnumerable<ClienteBeneficiario>> GetActivosByClienteIdAsync(int clienteId);

    /// <summary>
    /// Obtiene los beneficiarios inactivos o fallecidos de un cliente
    /// </summary>
    Task<IEnumerable<ClienteBeneficiario>> GetInactivosByClienteIdAsync(int clienteId);

    /// <summary>
    /// Calcula la suma total del porcentaje de beneficiarios activos de un cliente
    /// Regla: Debe ser 100% cuando está completo
    /// </summary>
    Task<decimal> GetSumaPorcentajeActivosAsync(int clienteId);

    /// <summary>
    /// Verifica si los beneficiarios activos de un cliente suman exactamente 100%
    /// </summary>
    Task<bool> SonBeneficiariosCompletosAsync(int clienteId);

    /// <summary>
    /// Crea un nuevo beneficiario
    /// </summary>
    Task<ClienteBeneficiario> CreateAsync(ClienteBeneficiario beneficiario);

    /// <summary>
    /// Actualiza un beneficiario existente
    /// </summary>
    Task<ClienteBeneficiario> UpdateAsync(ClienteBeneficiario beneficiario);

    /// <summary>
    /// Marca un beneficiario como fallecido y guarda histórico
    /// </summary>
    Task<bool> RegistrarFallecimientoAsync(int beneficiarioId, string? notas);

    /// <summary>
    /// Desactiva un beneficiario (sin eliminar datos)
    /// </summary>
    Task<bool> DeactivateAsync(int beneficiarioId, string? razon);

    /// <summary>
    /// Obtiene el historial de cambios de un beneficiario
    /// </summary>
    Task<IEnumerable<ClienteBeneficiarioHistorico>> GetHistoricoByBeneficiarioIdAsync(int beneficiarioId);

    /// <summary>
    /// Obtiene el historial completo de cambios de beneficiarios de un cliente
    /// </summary>
    Task<IEnumerable<ClienteBeneficiarioHistorico>> GetHistoricoByClienteIdAsync(int clienteId);

    /// <summary>
    /// Obtiene la cantidad de beneficiarios activos de un cliente
    /// </summary>
    Task<int> GetCantidadActivosAsync(int clienteId);

    /// <summary>
    /// Verifica si existe un beneficiario con el DUI especificado para un cliente
    /// </summary>
    Task<bool> ExistsByDuiAsync(int clienteId, string dui);
}
