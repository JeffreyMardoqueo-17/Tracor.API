using Microsoft.EntityFrameworkCore;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using Tradecorp.Infrastructure.Data;

namespace Tradecorp.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio de pagos
/// </summary>
public class PagoRepository : IPagoRepository
{
    private readonly AppDbContext _context;

    public PagoRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Pago?> GetByIdAsync(int id)
    {
        return await _context.Pagos
            .Include(p => p.CalculoPago)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Pago>> GetByContratoIdAsync(int contratoId)
    {
        return await _context.Pagos
            .Include(p => p.CalculoPago)
            .Where(p => p.CalculoPago.ContratoId == contratoId)
            .ToListAsync();
    }

    public async Task<Pago?> GetProximoPagoAsync(int contratoId)
    {
        return await _context.Pagos
            .Include(p => p.CalculoPago)
            .Where(p => p.CalculoPago.ContratoId == contratoId && p.Estado == EstadoPago.Pendiente)
            .OrderBy(p => p.CalculoPago.FechaCorte)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Pago>> GetAllAsync()
    {
        return await _context.Pagos
            .Include(p => p.CalculoPago)
            .ToListAsync();
    }
}
