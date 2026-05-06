using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;

namespace Tradecorp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionSistemaService _service;

    public ConfiguracionController(IConfiguracionSistemaService service)
    {
        _service = service;
    }

    [HttpGet("sistema")]
    public async Task<IActionResult> GetSistema()
    {
        var data = await _service.GetSistemaAsync();
        return Ok(data);
    }

    [HttpPut("sistema")]
    public async Task<IActionResult> UpdateSistema([FromBody] UpdateSistemaConfiguracionRequestDto dto)
    {
        var data = await _service.UpdateSistemaAsync(dto);
        return Ok(data);
    }

    [HttpGet("cortes")]
    public async Task<IActionResult> GetCortes()
    {
        var data = await _service.GetCortesAsync();
        return Ok(data);
    }

    [HttpPost("cortes")]
    public async Task<IActionResult> CreateCorte([FromBody] UpsertCortePagoRequestDto dto)
    {
        var data = await _service.CreateCorteAsync(dto);
        return CreatedAtAction(nameof(GetCortes), new { id = data.Id }, data);
    }

    [HttpPut("cortes/{id:int}")]
    public async Task<IActionResult> UpdateCorte(int id, [FromBody] UpsertCortePagoRequestDto dto)
    {
        var data = await _service.UpdateCorteAsync(id, dto);
        return Ok(data);
    }

    [HttpDelete("cortes/{id:int}")]
    public async Task<IActionResult> DeleteCorte(int id)
    {
        await _service.DeleteCorteAsync(id);
        return NoContent();
    }
}
