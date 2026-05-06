using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Tradecorp.Application.DTOs;
using Tradecorp.Application.Abstractions.Services;

namespace Tradecorp.API.Controllers;

/// <summary>
/// Controller para gestionar Contratos de Inversión
/// Todos los endpoints requieren autenticación JWT
/// </summary>
[ApiController]
[Route("api/contratos")]
[Authorize]
public class ContratosController : ControllerBase
{
    private readonly IContratoService _contratoService;
    private readonly ILogger<ContratosController> _logger;

    public ContratosController(
        IContratoService contratoService,
        ILogger<ContratosController> logger)
    {
        _contratoService = contratoService ?? throw new ArgumentNullException(nameof(contratoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Crea un nuevo contrato de inversión
    /// Validaciones:
    /// - Capital > 0
    /// - Porcentaje entre 6% y 8.50%
    /// - Cliente debe existir
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> CrearContrato([FromBody] CreateContratoRequest request)
    {
        try
        {
            var contratoCreado = await _contratoService.CrearContratoAsync(request);
            return CreatedAtAction(nameof(ObtenerContrato), new { id = contratoCreado.Id }, contratoCreado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida al crear contrato: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Operación no permitida al crear contrato: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al crear contrato: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al crear contrato." });
        }
    }

    /// <summary>
    /// Obtiene un contrato específico con todos sus detalles
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> ObtenerContrato(int id)
    {
        try
        {
            var contrato = await _contratoService.ObtenerContratoAsync(id);
            if (contrato == null)
                return NotFound(new { error = $"Contrato con ID {id} no encontrado." });

            return Ok(contrato);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener contrato: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener contrato." });
        }
    }

    /// <summary>
    /// Obtiene todos los contratos de un cliente (activos e inactivos)
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    [ProducesResponseType(typeof(IEnumerable<ContratoListaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ContratoListaResponse>>> ObtenerContratosCliente(int clienteId)
    {
        try
        {
            var contratos = await _contratoService.ObtenerContratosClienteAsync(clienteId);
            return Ok(contratos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener contratos del cliente: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener contratos." });
        }
    }

    /// <summary>
    /// Obtiene solo los contratos activos de un cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}/activos")]
    [ProducesResponseType(typeof(IEnumerable<ContratoListaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ContratoListaResponse>>> ObtenerContratosActivosCliente(int clienteId)
    {
        try
        {
            var contratos = await _contratoService.ObtenerContratosActivosClienteAsync(clienteId);
            return Ok(contratos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener contratos activos: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener contratos." });
        }
    }

    /// <summary>
    /// Obtiene todos los contratos activos del sistema
    /// </summary>
    [HttpGet("activos")]
    [ProducesResponseType(typeof(IEnumerable<ContratoListaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ContratoListaResponse>>> ObtenerTodosContratosActivos()
    {
        try
        {
            var contratos = await _contratoService.ObtenerTodosContratosActivosAsync();
            return Ok(contratos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener contratos activos: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener contratos." });
        }
    }

    /// <summary>
    /// Actualiza información de un contrato
    /// Solo ciertos campos pueden editarse en contratos activos
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> ActualizarContrato(int id, [FromBody] UpdateContratoRequest request)
    {
        try
        {
            var contratoActualizado = await _contratoService.ActualizarContratoAsync(id, request);
            return Ok(contratoActualizado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Contrato no encontrado o no puede actualizarse: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida al actualizar contrato: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al actualizar contrato: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al actualizar contrato." });
        }
    }

    /// <summary>
    /// Finaliza un contrato (marca como inactivo)
    /// </summary>
    [HttpPost("{id}/finalizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FinalizarContrato(int id)
    {
        try
        {
            var resultado = await _contratoService.FinalizarContratoAsync(id);
            if (!resultado)
                return NotFound(new { error = $"Contrato con ID {id} no encontrado." });

            return Ok(new { mensaje = "Contrato finalizado correctamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al finalizar contrato: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al finalizar contrato." });
        }
    }

    /// <summary>
    /// Asigna beneficiarios a un contrato
    /// Puede reutilizar beneficiarios existentes o crear nuevos
    /// </summary>
    [HttpPost("{id}/beneficiarios")]
    [ProducesResponseType(typeof(IEnumerable<BeneficiarioContratoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BeneficiarioContratoResponse>>> AsignarBeneficiarios(
        int id,
        [FromBody] List<AsignarBeneficiarioRequest> request)
    {
        try
        {
            var beneficiarios = await _contratoService.AsignarBeneficiariosAsync(id, request);
            return Ok(beneficiarios);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Operación no permitida: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validación fallida: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al asignar beneficiarios: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al asignar beneficiarios." });
        }
    }

    /// <summary>
    /// Obtiene los beneficiarios asignados a un contrato
    /// </summary>
    [HttpGet("{id}/beneficiarios")]
    [ProducesResponseType(typeof(IEnumerable<BeneficiarioContratoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BeneficiarioContratoResponse>>> ObtenerBeneficiarios(int id)
    {
        try
        {
            var beneficiarios = await _contratoService.ObtenerBeneficiariosContratoAsync(id);
            return Ok(beneficiarios);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener beneficiarios: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener beneficiarios." });
        }
    }

    /// <summary>
    /// Unifica múltiples contratos en uno solo
    /// Suma los capitales y marca los contratos origen como finalizados
    /// </summary>
    [HttpPost("{id}/unificar")]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> UnificarContratos(
        int id,
        [FromBody] List<int> contratosOrigenIds)
    {
        try
        {
            var contratoUnificado = await _contratoService.UnificarContratosAsync(id, contratosOrigenIds);
            return Ok(contratoUnificado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Operación no permitida: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al unificar contratos: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al unificar contratos." });
        }
    }

    /// <summary>
    /// Desunifica un contrato
    /// </summary>
    [HttpPost("{id}/desunificar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DesunificarContrato(int id)
    {
        try
        {
            var resultado = await _contratoService.DesunificarContratoAsync(id);
            if (!resultado)
                return NotFound(new { error = $"Contrato con ID {id} no encontrado." });

            return Ok(new { mensaje = "Contrato desunificado correctamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al desunificar contrato: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al desunificar contrato." });
        }
    }
}
