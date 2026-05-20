using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
    private readonly IContratoProjectionService _proyeccionService;
    private readonly ILogger<ContratosController> _logger;

    public ContratosController(
        IContratoService contratoService,
        IContratoProjectionService proyeccionService,
        ILogger<ContratosController> logger)
    {
        _contratoService = contratoService ?? throw new ArgumentNullException(nameof(contratoService));
        _proyeccionService = proyeccionService ?? throw new ArgumentNullException(nameof(proyeccionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> CrearContrato([FromBody] CreateContratoRequest request)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var contratoCreado = await _contratoService.CrearContratoAsync(request, usuarioId);
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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContratoListaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ContratoListaResponse>>> ObtenerContratos([FromQuery] ContratoFiltroRequest filtro)
    {
        try
        {
            var contratos = await _contratoService.ObtenerContratosAsync(filtro);
            return Ok(contratos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al listar contratos: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al listar contratos." });
        }
    }

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

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> ActualizarContrato(int id, [FromBody] UpdateContratoRequest request)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var contratoActualizado = await _contratoService.ActualizarContratoAsync(id, request, usuarioId);
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

    [HttpPost("{id}/finalizar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FinalizarContrato(int id, [FromBody] DesunificarContratoRequest? request)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var resultado = await _contratoService.FinalizarContratoAsync(id, usuarioId, request?.Observacion);
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
            var usuarioId = ObtenerUsuarioId();
            var beneficiarios = await _contratoService.AsignarBeneficiariosAsync(id, request, usuarioId);
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

    [HttpPost("{id}/reinversion")]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> ReinvertirContrato(int id, [FromBody] ReinversionContratoRequest request)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var contrato = await _contratoService.RegistrarReinversionAsync(id, request, usuarioId);
            return Ok(contrato);
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
            _logger.LogError($"Error al registrar reinversión: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al registrar reinversión." });
        }
    }

    [HttpPost("{id}/inyeccion-capital")]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> RegistrarInyeccionCapital(int id, [FromBody] InyeccionCapitalRequest request)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var contrato = await _contratoService.RegistrarInyeccionCapitalAsync(id, request, usuarioId);
            return Ok(contrato);
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
            _logger.LogError($"Error al registrar inyección de capital: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al registrar inyección de capital." });
        }
    }

    [HttpPost("{id}/unificar")]
    [ProducesResponseType(typeof(ContratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoResponse>> UnificarContratos(
        int id,
        [FromBody] UnificarContratosRequest request)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var contratoUnificado = await _contratoService.UnificarContratosAsync(id, request, usuarioId);
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

    [HttpPost("{id}/desunificar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DesunificarContrato(int id, [FromBody] DesunificarContratoRequest request)
    {
        try
        {
            var usuarioId = ObtenerUsuarioId();
            var resultado = await _contratoService.DesunificarContratoAsync(id, request, usuarioId);
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

    [HttpGet("{id}/historial-financiero")]
    [ProducesResponseType(typeof(IEnumerable<HistorialFinancieroItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<HistorialFinancieroItemResponse>>> ObtenerHistorialFinanciero(int id)
    {
        try
        {
            var historial = await _contratoService.ObtenerHistorialFinancieroAsync(id);
            return Ok(historial);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener historial financiero: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener historial financiero." });
        }
    }

    [HttpGet("{id}/eventos")]
    [ProducesResponseType(typeof(IEnumerable<ContratoEventoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ContratoEventoResponse>>> ObtenerEventos(int id)
    {
        try
        {
            var eventos = await _contratoService.ObtenerEventosAsync(id);
            return Ok(eventos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener eventos: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener eventos." });
        }
    }

    [HttpGet("{id}/auditoria")]
    [ProducesResponseType(typeof(IEnumerable<AuditoriaContratoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<AuditoriaContratoResponse>>> ObtenerAuditoria(int id)
    {
        try
        {
            var auditoria = await _contratoService.ObtenerAuditoriaAsync(id);
            return Ok(auditoria);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener auditoría: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener auditoría." });
        }
    }

    [HttpGet("{id}/proyeccion-24m")]
    [ProducesResponseType(typeof(ContratoProjectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoProjectionResponse>> ObtenerProyeccion24Meses(int id)
    {
        try
        {
            var proyeccion = await _proyeccionService.ObtenerProyeccion24MesesAsync(id);
            if (proyeccion is null)
                return NotFound(new { error = $"Contrato con ID {id} no encontrado." });

            return Ok(proyeccion);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener proyección del contrato: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener proyección del contrato." });
        }
    }

    [HttpPost("simulacion")]
    [ProducesResponseType(typeof(ContratoProjectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContratoProjectionResponse>> SimularContrato([FromBody] SimularContratoRequest request)
    {
        try
        {
            var simulacion = await _proyeccionService.SimularAsync(request);
            return Ok(simulacion);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validación fallida al simular contrato.");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al simular contrato.");
            return StatusCode(500, new { error = "Error interno al simular contrato." });
        }
    }

    private int ObtenerUsuarioId()
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(rawUserId, out var usuarioId) || usuarioId <= 0)
            throw new InvalidOperationException("No se pudo determinar el usuario autenticado.");

        return usuarioId;
    }
}
