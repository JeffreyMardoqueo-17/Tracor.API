using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Application.Abstractions.Persistence;

/// <summary>
/// Repositorio para operaciones de Contrato
/// </summary>
public interface IContratoRepository
{
    /// <summary>
    /// Crear un nuevo contrato
    /// </summary>
    Task<Contrato> CreateAsync(Contrato contrato);

    /// <summary>
    /// Obtener contrato por ID con relaciones
    /// </summary>
    Task<Contrato?> GetByIdAsync(int id);

    /// <summary>
    /// Obtener contrato por número único
    /// </summary>
    Task<Contrato?> GetByNumeroContratoAsync(string numeroContrato);

    /// <summary>
    /// Obtener todos los contratos de un cliente
    /// </summary>
    Task<IEnumerable<Contrato>> GetByClienteIdAsync(int clienteId);

    /// <summary>
    /// Obtener contratos activos de un cliente
    /// </summary>
    Task<IEnumerable<Contrato>> GetActivosByClienteIdAsync(int clienteId);

    /// <summary>
    /// Obtener todos los contratos activos
    /// </summary>
    Task<IEnumerable<Contrato>> GetAllActivosAsync();

    /// <summary>
    /// Actualizar contrato
    /// </summary>
    Task<Contrato> UpdateAsync(Contrato contrato);

    /// <summary>
    /// Finalizar contrato (marca como inactivo)
    /// </summary>
    Task<bool> FinalizarAsync(int contratoId);

    /// <summary>
    /// Verifica si un número de contrato ya existe
    /// </summary>
    Task<bool> ExistsByNumeroContratoAsync(string numeroContrato);

    /// <summary>
    /// Verifica si un cliente tiene contratos activos
    /// </summary>
    Task<bool> TieneContratosActivosAsync(int clienteId);

    /// <summary>
    /// Obtener el siguiente número de contrato secuencial
    /// </summary>
    Task<string> ObtenerSiguienteNumeroContratoAsync(int clienteId);
}
