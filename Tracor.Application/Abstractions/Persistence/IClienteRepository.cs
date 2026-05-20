using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Application.Abstractions.Persistence;

/// <summary>
/// Interfaz del repositorio para acceder a datos de Clientes
/// Implementa el patrón Repository con SOLID principles
/// </summary>
public interface IClienteRepository
{
    /// <summary>
    /// Obtiene un cliente por su ID
    /// </summary>
    Task<Cliente?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un cliente por su número de documento
    /// </summary>
    Task<Cliente?> GetByNumeroDocumentoAsync(string numeroDocumento);

    /// <summary>
    /// Crea un nuevo cliente
    /// </summary>
    Task<Cliente> CreateAsync(Cliente cliente);

    /// <summary>
    /// Actualiza un cliente existente
    /// </summary>
    Task<Cliente> UpdateAsync(Cliente cliente);

    /// <summary>
    /// Marca un cliente como inactivo (soft delete)
    /// </summary>
    Task<bool> DeactivateAsync(int id);

    /// <summary>
    /// Verifica si existe un cliente con el número de documento especificado
    /// </summary>
    Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento);

    /// <summary>
    /// Verifica si existe un cliente con el código especificado
    /// </summary>
    Task<bool> ExistsByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene el próximo código de cliente disponible
    /// </summary>
    Task<string> GetNextCodigoClienteAsync();
}
