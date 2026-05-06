using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;
using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    // [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
    {
        var user = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await _userService.GetByIdAsync(id);
        if (u == null) return NotFound();
        return Ok(u);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] int? rol)
    {
        var list = await _userService.ListActiveAsync();
        if (rol.HasValue)
        {
            var rolString = rol.Value == 1 ? "Admin" : "Ejecutivo";
            list = list.Where(u => u.Rol == rolString);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            list = list.Where(u => u.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase)
                                 || u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
        }
        return Ok(list);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
    {
        await _userService.UpdateAsync(id, dto);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.SoftDeleteAsync(id);
        return NoContent();
    }
}
