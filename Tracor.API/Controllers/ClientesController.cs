using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Tradecorp.Application.DTOs;
using Tradecorp.Application.Abstractions.Services;

namespace Tradecorp.API.Controllers;

/// <summary>
/// Controller para gestionar Clientes y sus Beneficiarios
/// Todos los endpoints requieren autenticación JWT
/// </summary>
[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IBeneficiarioService _beneficiarioService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(
        IClienteService clienteService,
        IBeneficiarioService beneficiarioService,
        ILogger<ClientesController> logger)
    {
        _clienteService = clienteService ?? throw new ArgumentNullException(nameof(clienteService));
        _beneficiarioService = beneficiarioService ?? throw new ArgumentNullException(nameof(beneficiarioService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registra un nuevo cliente con su cuenta bancaria
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteResponse>> RegistrarCliente([FromBody] CreateClienteRequest request)
    {
        try
        {
            int usuarioId;
            if (request.UsuarioEjecutivoId.HasValue && request.UsuarioEjecutivoId.Value > 0)
            {
                usuarioId = request.UsuarioEjecutivoId.Value;
            }
            else
            {
                usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            }

            if (usuarioId == 0)
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var clienteCreado = await _clienteService.RegistrarClienteAsync(request, usuarioId);
            return CreatedAtAction(nameof(ObtenerCliente), new { id = clienteCreado.Id }, clienteCreado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida al registrar cliente: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Operación no permitida al registrar cliente: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al registrar cliente: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al registrar cliente." });
        }
    }

    /// <summary>
    /// Obtiene un cliente específico con todas sus relaciones
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteResponse>> ObtenerCliente(int id)
    {
        try
        {
            var cliente = await _clienteService.ObtenerClienteAsync(id);
            if (cliente == null)
                return NotFound(new { error = $"Cliente con ID {id} no encontrado." });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener cliente: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener cliente." });
        }
    }

    /// <summary>
    /// Obtiene un cliente por su código único
    /// </summary>
    [HttpGet("codigo/{codigoCliente}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteResponse>> ObtenerClientePorCodigo(string codigoCliente)
    {
        try
        {
            var cliente = await _clienteService.ObtenerClientePorCodigoAsync(codigoCliente);
            if (cliente == null)
                return NotFound(new { error = $"Cliente con código {codigoCliente} no encontrado." });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener cliente por código: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener cliente." });
        }
    }

    /// <summary>
    /// Obtiene todos los clientes activos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ClienteResponse>>> ObtenerClientes()
    {
        try
        {
            var clientes = await _clienteService.ObtenerClientesActivosAsync();
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener clientes: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener clientes." });
        }
    }

    /// <summary>
    /// Actualiza información de un cliente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteResponse>> ActualizarCliente(int id, [FromBody] UpdateClienteRequest request)
    {
        try
        {
            var clienteActualizado = await _clienteService.ActualizarClienteAsync(id, request);
            return Ok(clienteActualizado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Cliente no encontrado: {ex.Message}");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al actualizar cliente: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al actualizar cliente." });
        }
    }

    /// <summary>
    /// Desactiva un cliente (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DesactivarCliente(int id)
    {
        try
        {
            var resultado = await _clienteService.DesactivarClienteAsync(id);
            if (!resultado)
                return NotFound(new { error = $"Cliente con ID {id} no encontrado." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al desactivar cliente: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al desactivar cliente." });
        }
    }

    /// <summary>
    /// Obtiene advertencias del cliente (sin contrato, beneficiarios incompletos, etc.)
    /// </summary>
    [HttpGet("{id}/advertencias")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<string>>> ObtenerAdvertenciasCliente(int id)
    {
        try
        {
            var cliente = await _clienteService.ObtenerClienteAsync(id);
            if (cliente == null)
                return NotFound(new { error = $"Cliente con ID {id} no encontrado." });

            var advertencias = await _clienteService.ObtenerAdvertenciasClienteAsync(id);
            return Ok(advertencias);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener advertencias del cliente: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener advertencias." });
        }
    }

    // ==================== ENDPOINTS DE BENEFICIARIOS ====================

    /// <summary>
    /// Crea un nuevo beneficiario para un cliente
    /// Valida que la suma de porcentajes no exceda 100%
    /// </summary>
    [HttpPost("{clienteId}/beneficiarios")]
    [ProducesResponseType(typeof(ClienteBeneficiarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteBeneficiarioResponse>> CrearBeneficiario(int clienteId, [FromBody] CreateClienteBeneficiarioRequest request)
    {
        try
        {
            var beneficiarioCreado = await _beneficiarioService.CrearBeneficiarioAsync(clienteId, request);
            return CreatedAtAction(nameof(ObtenerBeneficiario), new { clienteId, beneficiarioId = beneficiarioCreado.Id }, beneficiarioCreado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida al crear beneficiario: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Operación no permitida al crear beneficiario: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al crear beneficiario: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al crear beneficiario." });
        }
    }

    /// <summary>
    /// Obtiene un beneficiario específico
    /// </summary>
    [HttpGet("{clienteId}/beneficiarios/{beneficiarioId}")]
    [ProducesResponseType(typeof(ClienteBeneficiarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteBeneficiarioResponse>> ObtenerBeneficiario(int clienteId, int beneficiarioId)
    {
        try
        {
            var beneficiario = await _beneficiarioService.ObtenerBeneficiarioAsync(beneficiarioId);
            if (beneficiario == null || beneficiario.Id != beneficiarioId)
                return NotFound(new { error = $"Beneficiario con ID {beneficiarioId} no encontrado." });

            return Ok(beneficiario);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener beneficiario: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener beneficiario." });
        }
    }

    /// <summary>
    /// Obtiene todos los beneficiarios de un cliente
    /// </summary>
    [HttpGet("{clienteId}/beneficiarios")]
    [ProducesResponseType(typeof(IEnumerable<ClienteBeneficiarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ClienteBeneficiarioResponse>>> ObtenerBeneficiarios(int clienteId)
    {
        try
        {
            var cliente = await _clienteService.ObtenerClienteAsync(clienteId);
            if (cliente == null)
                return NotFound(new { error = $"Cliente con ID {clienteId} no encontrado." });

            var beneficiarios = await _beneficiarioService.ObtenerBeneficiariosClienteAsync(clienteId);
            return Ok(beneficiarios);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener beneficiarios: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener beneficiarios." });
        }
    }

    /// <summary>
    /// Obtiene solo los beneficiarios activos de un cliente
    /// </summary>
    [HttpGet("{clienteId}/beneficiarios/activos")]
    [ProducesResponseType(typeof(IEnumerable<ClienteBeneficiarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ClienteBeneficiarioResponse>>> ObtenerBeneficiariosActivos(int clienteId)
    {
        try
        {
            var cliente = await _clienteService.ObtenerClienteAsync(clienteId);
            if (cliente == null)
                return NotFound(new { error = $"Cliente con ID {clienteId} no encontrado." });

            var beneficiarios = await _beneficiarioService.ObtenerBeneficiariosActivosAsync(clienteId);
            return Ok(beneficiarios);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener beneficiarios activos: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener beneficiarios activos." });
        }
    }

    /// <summary>
    /// Actualiza un beneficiario existente
    /// Valida que los porcentajes no excedan 100%
    /// </summary>
    [HttpPut("{clienteId}/beneficiarios/{beneficiarioId}")]
    [ProducesResponseType(typeof(ClienteBeneficiarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteBeneficiarioResponse>> ActualizarBeneficiario(int clienteId, int beneficiarioId, [FromBody] UpdateClienteBeneficiarioRequest request)
    {
        try
        {
            var beneficiarioActualizado = await _beneficiarioService.ActualizarBeneficiarioAsync(beneficiarioId, request);
            return Ok(beneficiarioActualizado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Beneficiario no encontrado: {ex.Message}");
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida al actualizar beneficiario: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al actualizar beneficiario: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al actualizar beneficiario." });
        }
    }

    /// <summary>
    /// Valida que los beneficiarios activos de un cliente sumen exactamente 100%
    /// </summary>
    [HttpGet("{clienteId}/validar-beneficiarios")]
    [ProducesResponseType(typeof(ClienteBeneficiarioValidacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClienteBeneficiarioValidacionResponse>> ValidarBeneficiarios(int clienteId)
    {
        try
        {
            var cliente = await _clienteService.ObtenerClienteAsync(clienteId);
            if (cliente == null)
                return NotFound(new { error = $"Cliente con ID {clienteId} no encontrado." });

            var validacion = await _beneficiarioService.ValidarBeneficiariosAsync(clienteId);
            return Ok(validacion);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al validar beneficiarios: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al validar beneficiarios." });
        }
    }

    /// <summary>
    /// Obtiene el porcentaje disponible para asignar a nuevos beneficiarios
    /// </summary>
    [HttpGet("{clienteId}/porcentaje-disponible")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<decimal>> ObtenerPorcentajeDisponible(int clienteId)
    {
        try
        {
            var cliente = await _clienteService.ObtenerClienteAsync(clienteId);
            if (cliente == null)
                return NotFound(new { error = $"Cliente con ID {clienteId} no encontrado." });

            var disponible = await _beneficiarioService.ObtenerPorcentajeDisponibleAsync(clienteId);
            return Ok(disponible);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener porcentaje disponible: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener porcentaje disponible." });
        }
    }

    /// <summary>
    /// Registra el fallecimiento de un beneficiario
    /// Guarda histórico con porcentaje y datos al momento del fallecimiento
    /// </summary>
    [HttpPost("{clienteId}/beneficiarios/{beneficiarioId}/fallecimiento")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegistrarFallecimientoBeneficiario(int clienteId, int beneficiarioId, [FromBody] RegistrarFallecimientoBeneficiarioRequest request)
    {
        try
        {
            var resultado = await _beneficiarioService.RegistrarFallecimientoBeneficiarioAsync(beneficiarioId, request);
            if (!resultado)
                return NotFound(new { error = $"Beneficiario con ID {beneficiarioId} no encontrado." });

            return Ok(new { mensaje = "Fallecimiento registrado correctamente. Se guardó el histórico." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida al registrar fallecimiento: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al registrar fallecimiento: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al registrar fallecimiento." });
        }
    }

    /// <summary>
    /// Obtiene el historial de cambios de un beneficiario
    /// </summary>
    [HttpGet("{clienteId}/beneficiarios/{beneficiarioId}/historico")]
    [ProducesResponseType(typeof(IEnumerable<ClienteBeneficiarioHistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ClienteBeneficiarioHistoricoResponse>>> ObtenerHistoricoBeneficiario(int clienteId, int beneficiarioId)
    {
        try
        {
            var historico = await _beneficiarioService.ObtenerHistoricoBeneficiarioAsync(beneficiarioId);
            return Ok(historico);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener histórico del beneficiario: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener histórico." });
        }
    }

    /// <summary>
    /// Obtiene el historial completo de beneficiarios de un cliente
    /// </summary>
    [HttpGet("{clienteId}/beneficiarios/historico")]
    [ProducesResponseType(typeof(IEnumerable<ClienteBeneficiarioHistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ClienteBeneficiarioHistoricoResponse>>> ObtenerHistoricoCliente(int clienteId)
    {
        try
        {
            var cliente = await _clienteService.ObtenerClienteAsync(clienteId);
            if (cliente == null)
                return NotFound(new { error = $"Cliente con ID {clienteId} no encontrado." });

            var historico = await _beneficiarioService.ObtenerHistoricoClienteAsync(clienteId);
            return Ok(historico);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener historial del cliente: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener historial." });
        }
    }
}
