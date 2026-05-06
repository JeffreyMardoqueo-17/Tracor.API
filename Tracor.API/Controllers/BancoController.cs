using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;

namespace Tradecorp.API.Controllers;

/// <summary>
/// Controller para gestionar el catalogo de Bancos
/// Todos los endpoints requieren autenticacion JWT
/// </summary>
[ApiController]
[Route("api/bancos")]
[Authorize]
public class BancoController : ControllerBase
{
    private readonly IBancoService _bancoService;
    private readonly ILogger<BancoController> _logger;

    public BancoController(IBancoService bancoService, ILogger<BancoController> logger)
    {
        _bancoService = bancoService ?? throw new ArgumentNullException(nameof(bancoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene todos los bancos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BancoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BancoResponse>>> ObtenerBancos()
    {
        try
        {
            var bancos = await _bancoService.GetAllAsync();
            return Ok(bancos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener bancos: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener bancos." });
        }
    }

    /// <summary>
    /// Obtiene un banco por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BancoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BancoResponse>> ObtenerBanco(int id)
    {
        try
        {
            var banco = await _bancoService.GetByIdAsync(id);
            if (banco == null)
                return NotFound(new { error = $"Banco con ID {id} no encontrado." });

            return Ok(banco);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al obtener banco: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al obtener banco." });
        }
    }

    /// <summary>
    /// Crea un nuevo banco
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BancoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BancoResponse>> CrearBanco([FromBody] CreateBancoRequest request)
    {
        try
        {
            var bancoCreado = await _bancoService.CreateAsync(request);
            return CreatedAtAction(nameof(ObtenerBanco), new { id = bancoCreado.Id }, bancoCreado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Validacion fallida al crear banco: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Operacion no permitida al crear banco: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al crear banco: {ex.Message}", ex);
            return StatusCode(500, new { error = "Error interno al crear banco." });
        }
    }
}
