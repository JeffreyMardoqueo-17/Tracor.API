using Microsoft.EntityFrameworkCore;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using Tradecorp.Infrastructure.Data;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio para Beneficiarios
/// Maneja la lógica de persistencia de beneficiarios y su histórico
/// Valida reglas de negocio: porcentajes activos deben sumar 100%
/// </summary>
public class ClienteBeneficiarioRepository : IClienteBeneficiarioRepository
{
    private readonly AppDbContext _dbContext;

    public ClienteBeneficiarioRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ClienteBeneficiario?> GetByIdAsync(int id)
    {
        return await _dbContext.ClientesBeneficiarios
            .AsNoTracking()
            .Include(b => b.Cliente)
            .Include(b => b.Historico)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<ClienteBeneficiario>> GetByClienteIdAsync(int clienteId)
    {
        return await _dbContext.ClientesBeneficiarios
            .AsNoTracking()
            .Where(b => b.ClienteId == clienteId)
            .OrderBy(b => b.NombreCompleto)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClienteBeneficiario>> GetActivosByClienteIdAsync(int clienteId)
    {
        return await _dbContext.ClientesBeneficiarios
            .AsNoTracking()
            .Where(b => b.ClienteId == clienteId && b.Estado == ClienteBeneficiarioEstado.Activo)
            .OrderBy(b => b.NombreCompleto)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClienteBeneficiario>> GetInactivosByClienteIdAsync(int clienteId)
    {
        return await _dbContext.ClientesBeneficiarios
            .AsNoTracking()
            .Where(b => b.ClienteId == clienteId && 
                   (b.Estado == ClienteBeneficiarioEstado.Inactivo || 
                    b.Estado == ClienteBeneficiarioEstado.Fallecido))
            .OrderBy(b => b.NombreCompleto)
            .ToListAsync();
    }

    public async Task<decimal> GetSumaPorcentajeActivosAsync(int clienteId)
    {
        var suma = await _dbContext.ClientesBeneficiarios
            .Where(b => b.ClienteId == clienteId && b.Estado == ClienteBeneficiarioEstado.Activo)
            .SumAsync(b => b.Porcentaje);

        return suma;
    }

    public async Task<bool> SonBeneficiariosCompletosAsync(int clienteId)
    {
        var suma = await GetSumaPorcentajeActivosAsync(clienteId);
        return suma == 100m;
    }

    public async Task<ClienteBeneficiario> CreateAsync(ClienteBeneficiario beneficiario)
    {
        beneficiario.FechaCreacion = DateTime.UtcNow;
        beneficiario.FechaActualizacion = DateTime.UtcNow;

        _dbContext.ClientesBeneficiarios.Add(beneficiario);
        await _dbContext.SaveChangesAsync();
        return beneficiario;
    }

    public async Task<ClienteBeneficiario> UpdateAsync(ClienteBeneficiario beneficiario)
    {
        beneficiario.FechaActualizacion = DateTime.UtcNow;
        _dbContext.ClientesBeneficiarios.Update(beneficiario);
        await _dbContext.SaveChangesAsync();
        return beneficiario;
    }

    public async Task<bool> RegistrarFallecimientoAsync(int beneficiarioId, string? notas)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var beneficiario = await _dbContext.ClientesBeneficiarios
                .FirstOrDefaultAsync(b => b.Id == beneficiarioId);

            if (beneficiario == null)
                return false;

            // Crear registro histórico ANTES de cambiar estado
            var historico = new ClienteBeneficiarioHistorico
            {
                ClienteBeneficiarioId = beneficiario.Id,
                ClienteId = beneficiario.ClienteId,
                NombreCompleto = beneficiario.NombreCompleto,
                DUI = beneficiario.DUI,
                PorcentajeAsignado = beneficiario.Porcentaje,
                TipoRelacion = beneficiario.TipoRelacion,
                Evento = "Fallecimiento registrado",
                FechaEvento = DateTime.UtcNow,
                Notas = notas
            };

            _dbContext.ClientesBeneficiariosHistorico.Add(historico);

            // Marcar como fallecido
            beneficiario.Estado = ClienteBeneficiarioEstado.Fallecido;
            beneficiario.FechaActualizacion = DateTime.UtcNow;

            _dbContext.ClientesBeneficiarios.Update(beneficiario);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeactivateAsync(int beneficiarioId, string? razon)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var beneficiario = await _dbContext.ClientesBeneficiarios
                .FirstOrDefaultAsync(b => b.Id == beneficiarioId);

            if (beneficiario == null)
                return false;

            // Crear registro histórico
            var historico = new ClienteBeneficiarioHistorico
            {
                ClienteBeneficiarioId = beneficiario.Id,
                ClienteId = beneficiario.ClienteId,
                NombreCompleto = beneficiario.NombreCompleto,
                DUI = beneficiario.DUI,
                PorcentajeAsignado = beneficiario.Porcentaje,
                TipoRelacion = beneficiario.TipoRelacion,
                Evento = "Beneficiario desactivado",
                FechaEvento = DateTime.UtcNow,
                Notas = razon
            };

            _dbContext.ClientesBeneficiariosHistorico.Add(historico);

            // Marcar como inactivo
            beneficiario.Estado = ClienteBeneficiarioEstado.Inactivo;
            beneficiario.FechaActualizacion = DateTime.UtcNow;

            _dbContext.ClientesBeneficiarios.Update(beneficiario);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<ClienteBeneficiarioHistorico>> GetHistoricoByBeneficiarioIdAsync(int beneficiarioId)
    {
        return await _dbContext.ClientesBeneficiariosHistorico
            .Where(h => h.ClienteBeneficiarioId == beneficiarioId)
            .OrderByDescending(h => h.FechaEvento)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClienteBeneficiarioHistorico>> GetHistoricoByClienteIdAsync(int clienteId)
    {
        return await _dbContext.ClientesBeneficiariosHistorico
            .Where(h => h.ClienteId == clienteId)
            .OrderByDescending(h => h.FechaEvento)
            .ToListAsync();
    }

    public async Task<int> GetCantidadActivosAsync(int clienteId)
    {
        return await _dbContext.ClientesBeneficiarios
            .CountAsync(b => b.ClienteId == clienteId && b.Estado == ClienteBeneficiarioEstado.Activo);
    }

    public async Task<bool> ExistsByDuiAsync(int clienteId, string dui)
    {
        return await _dbContext.ClientesBeneficiarios
            .AnyAsync(b => b.ClienteId == clienteId && b.DUI == dui);
    }
}
