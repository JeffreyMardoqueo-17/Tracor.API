using Microsoft.EntityFrameworkCore;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Infrastructure.Data;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio para Clientes
/// Usa Entity Framework Core para acceso a datos
/// </summary>
public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _dbContext;

    public ClienteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        return await _dbContext.Clientes
            .AsNoTracking()
            .Include(c => c.UsuarioEjecutivo)
            .Include(c => c.ClienteCuentas)
                .ThenInclude(cc => cc.Banco)
            .Include(c => c.Beneficiarios)
            .Include(c => c.Contratos)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> GetByCodigoClienteAsync(string codigoCliente)
    {
        return await _dbContext.Clientes
            .AsNoTracking()
            .Include(c => c.UsuarioEjecutivo)
            .Include(c => c.ClienteCuentas)
                .ThenInclude(cc => cc.Banco)
            .Include(c => c.Beneficiarios)
            .FirstOrDefaultAsync(c => c.CodigoCliente == codigoCliente);
    }

    public async Task<Cliente?> GetByNumeroDocumentoAsync(string numeroDocumento)
    {
        return await _dbContext.Clientes
            .Include(c => c.UsuarioEjecutivo)
            .FirstOrDefaultAsync(c => c.NumeroDocumento == numeroDocumento);
    }

    public async Task<IEnumerable<Cliente>> GetAllActivosAsync()
    {
        return await _dbContext.Clientes
            .Where(c => c.Activo)
            .AsNoTracking()
            .Include(c => c.UsuarioEjecutivo)
            .Include(c => c.ClienteCuentas)
                .ThenInclude(cc => cc.Banco)
            .Include(c => c.Beneficiarios)
            .Include(c => c.Contratos)
            .OrderBy(c => c.NombreCompleto)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cliente>> GetByEjecutivoIdAsync(int ejecutivoId)
    {
        return await _dbContext.Clientes
            .Where(c => c.UsuarioEjecutivoId == ejecutivoId && c.Activo)
            .AsNoTracking()
            .Include(c => c.ClienteCuentas)
                .ThenInclude(cc => cc.Banco)
            .Include(c => c.Beneficiarios)
            .OrderBy(c => c.NombreCompleto)
            .ToListAsync();
    }

    public async Task<Cliente> CreateAsync(Cliente cliente)
    {
        // Generar código de cliente si no existe
        if (string.IsNullOrEmpty(cliente.CodigoCliente))
        {
            cliente.CodigoCliente = await GetNextCodigoClienteAsync();
        }

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();
        
        // Recargar con relaciones
        return await _dbContext.Clientes
            .Include(c => c.UsuarioEjecutivo)
            .Include(c => c.ClienteCuentas)
                .ThenInclude(cc => cc.Banco)
            .Include(c => c.Beneficiarios)
            .Include(c => c.Contratos)
            .FirstAsync(c => c.Id == cliente.Id);
    }

    public async Task<Cliente> UpdateAsync(Cliente cliente)
    {
        _dbContext.Clientes.Update(cliente);
        await _dbContext.SaveChangesAsync();
        return cliente;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var cliente = await _dbContext.Clientes.FindAsync(id);
        if (cliente == null)
            return false;

        cliente.Activo = false;
        _dbContext.Clientes.Update(cliente);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento)
    {
        return await _dbContext.Clientes
            .AnyAsync(c => c.NumeroDocumento == numeroDocumento);
    }

    public async Task<bool> ExistsByCodigoAsync(string codigo)
    {
        return await _dbContext.Clientes
            .AnyAsync(c => c.CodigoCliente == codigo);
    }

    public async Task<string> GetNextCodigoClienteAsync()
    {
        var lastCliente = await _dbContext.Clientes
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        if (lastCliente == null)
            return "CLI-001";

        // Extraer el número del código actual
        var parts = lastCliente.CodigoCliente.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out var number))
        {
            return $"CLI-{(number + 1):D3}";
        }

        return "CLI-001";
    }
}
