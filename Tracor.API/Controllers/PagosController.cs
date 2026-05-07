using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;

namespace Tradecorp.API.Controllers;

/// <summary>
/// Controller para cálculos de pagos y ganancias
/// Sigue las reglas de negocio del documento project.md
/// </summary>
[ApiController]
[Route("api/pagos")]
[Authorize]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;
    private readonly ILogger<PagosController> _logger;

    public PagosController(
        IPagoService pagoService,
        ILogger<PagosController> logger)
    {
        _pagoService = pagoService ?? throw new ArgumentNullException(nameof(pagoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene el historial de pagos de un contrato
    /// </summary>
    [HttpGet("contrato/{contratoId}")]
    [ProducesResponseType(typeof(IEnumerable<PagoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<PagoResponse>>> ObtenerPagosContrato(int contratoId)
    {
        try
        {
            var pagos = await _pagoService.ObtenerPagosContratoAsync(contratoId);
            return Ok(pagos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener pagos: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener pagos." });
        }
    }

    /// <summary>
    /// Registra una decisión de pago (retiro, reinversión, etc.)
    /// </summary>
    [HttpPost("decision")]
    [ProducesResponseType(typeof(PagoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagoResponse>> RegistrarDecision([FromBody] DecisionPagoRequest request)
    {
        try
        {
            var response = await _pagoService.RegistrarDecisionAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Operación no permitida: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al registrar decisión: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al registrar decisión." });
        }
    }

    /// <summary>
    /// Obtiene el próximo pago programado para un contrato
    /// </summary>
    [HttpGet("contrato/{contratoId}/proximo")]
    [ProducesResponseType(typeof(PagoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagoResponse>> ObtenerProximoPago(int contratoId)
    {
        try
        {
            var pago = await _pagoService.ObtenerProximoPagoAsync(contratoId);
            if (pago == null)
                return NotFound(new { error = "No hay pagos programados." });

            return Ok(pago);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener próximo pago: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno." });
        }
    }
}
