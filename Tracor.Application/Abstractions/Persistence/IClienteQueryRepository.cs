using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Persistence;

/// <summary>
/// Read model de clientes.
/// Separa consultas optimizadas de la persistencia del agregado Cliente.
/// </summary>
public interface IClienteQueryRepository
{
    Task<ClienteDetalleResponse?> GetDetalleByIdAsync(int clienteId);
    Task<ClienteDetalleResponse?> GetDetalleByCodigoAsync(string codigoCliente);
    Task<IReadOnlyCollection<ClienteResumenResponse>> GetActivosAsync();
    Task<IReadOnlyCollection<ClienteResumenResponse>> GetByEjecutivoIdAsync(int ejecutivoId);
    Task<ClienteResumenResponse?> GetResumenByIdAsync(int clienteId);
}
