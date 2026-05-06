using Microsoft.EntityFrameworkCore;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Infrastructure.Data;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio para Contratos
/// Usa Entity Framework Core para acceso a datos
/// </summary>
public class ContratoRepository : IContratoRepository
{
    private readonly AppDbContext _dbContext;

    public ContratoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Contrato?> GetByIdAsync(int id)
    {
        return await _dbContext.Contratos
            .AsNoTracking()
            .Include(c => c.Cliente)
            .Include(c => c.ConfiguracionesContrato)
            .Include(c => c.MovimientosContrato)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contrato?> GetByNumeroContratoAsync(string numeroContrato)
    {
        return await _dbContext.Contratos
            .AsNoTracking()
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.NumeroContrato == numeroContrato);
    }

    public async Task<IEnumerable<Contrato>> GetByClienteIdAsync(int clienteId)
    {
        return await _dbContext.Contratos
            .Where(c => c.ClienteId == clienteId)
            .AsNoTracking()
            .Include(c => c.Cliente)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Contrato>> GetActivosByClienteIdAsync(int clienteId)
    {
        return await _dbContext.Contratos
            .Where(c => c.ClienteId == clienteId && c.Activo)
            .AsNoTracking()
            .Include(c => c.Cliente)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Contrato>> GetAllActivosAsync()
    {
        return await _dbContext.Contratos
            .Where(c => c.Activo)
            .AsNoTracking()
            .Include(c => c.Cliente)
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task<Contrato> CreateAsync(Contrato contrato)
    {
        // Generar número de contrato si no existe
        if (string.IsNullOrEmpty(contrato.NumeroContrato))
        {
            contrato.NumeroContrato = await ObtenerSiguienteNumeroContratoAsync(contrato.ClienteId);
        }

        contrato.FechaCreacion = DateTime.UtcNow;
        
        _dbContext.Contratos.Add(contrato);
        await _dbContext.SaveChangesAsync();
        
        // Recargar con relaciones
        return await _dbContext.Contratos
            .Include(c => c.Cliente)
            .FirstAsync(c => c.Id == contrato.Id);
    }

    public async Task<Contrato> UpdateAsync(Contrato contrato)
    {
        _dbContext.Contratos.Update(contrato);
        await _dbContext.SaveChangesAsync();
        return contrato;
    }

    public async Task<bool> FinalizarAsync(int contratoId)
    {
        var contrato = await _dbContext.Contratos.FindAsync(contratoId);
        if (contrato == null)
            return false;

        contrato.Activo = false;
        contrato.FechaCierre = DateTime.UtcNow;
        _dbContext.Contratos.Update(contrato);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNumeroContratoAsync(string numeroContrato)
    {
        return await _dbContext.Contratos
            .AnyAsync(c => c.NumeroContrato == numeroContrato);
    }

    public async Task<bool> TieneContratosActivosAsync(int clienteId)
    {
        return await _dbContext.Contratos
            .AnyAsync(c => c.ClienteId == clienteId && c.Activo);
    }

    public async Task<string> ObtenerSiguienteNumeroContratoAsync(int clienteId)
    {
        var lastContrato = await _dbContext.Contratos
            .Where(c => c.ClienteId == clienteId)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        if (lastContrato == null)
            return $"CONT-{clienteId:D6}-001";

        // Extraer el número secuencial del último contrato
        var parts = lastContrato.NumeroContrato.Split('-');
        if (parts.Length == 3 && int.TryParse(parts[2], out var number))
        {
            return $"CONT-{clienteId:D6}-{(number + 1):D3}";
        }

        return $"CONT-{clienteId:D6}-001";
    }
}
