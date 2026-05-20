using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

/// <summary>
/// Interfaz del servicio de negocios para Clientes
/// Aplica reglas de negocio y valida operaciones
/// </summary>
public interface IClienteService
{
    /// <summary>
    /// Registra un nuevo cliente con su cuenta bancaria
    /// </summary>
    /// <param name="request">Datos del cliente a registrar</param>
    /// <param name="usuarioEjecutivoId">ID del usuario ejecutivo asignado</param>
    /// <returns>Cliente registrado con código asignado (solo Id, CodigoCliente, NombreCompleto)</returns>
    Task<CreateClienteResponse> RegistrarClienteAsync(CreateClienteRequest request, int usuarioEjecutivoId);

    /// <summary>
    /// Obtiene un cliente con todas sus relaciones
    /// </summary>
    Task<ClienteDetalleResponse?> ObtenerClienteAsync(int clienteId);

    /// <summary>
    /// Obtiene un cliente por su código único
    /// </summary>
    Task<ClienteDetalleResponse?> ObtenerClientePorCodigoAsync(string codigoCliente);

    /// <summary>
    /// Obtiene todos los clientes activos con información resumida
    /// </summary>
    Task<IEnumerable<ClienteResumenResponse>> ObtenerClientesActivosAsync();

    /// <summary>
    /// Obtiene todos los clientes de un ejecutivo
    /// </summary>
    Task<IEnumerable<ClienteResumenResponse>> ObtenerClientesPorEjecutivoAsync(int ejecutivoId);

    /// <summary>
    /// Actualiza información básica del cliente
    /// </summary>
    Task<ClienteDetalleResponse> ActualizarClienteAsync(int clienteId, UpdateClienteRequest request);

    /// <summary>
    /// Desactiva un cliente (soft delete)
    /// </summary>
    Task<bool> DesactivarClienteAsync(int clienteId);

    /// <summary>
    /// Verifica si un cliente tiene contrato activo
    /// </summary>
    Task<bool> TieneContratoActivoAsync(int clienteId);

    /// <summary>
    /// Obtiene advertencias del cliente (ej: sin contrato, beneficiarios incompletos)
    /// </summary>
    Task<IEnumerable<string>> ObtenerAdvertenciasClienteAsync(int clienteId);
}
