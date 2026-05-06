using Microsoft.EntityFrameworkCore;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Infrastructure.Data;

namespace Tradecorp.Infrastructure.Persistence;

public class ConfiguracionSistemaRepository : IConfiguracionSistemaRepository
{
    private readonly AppDbContext _db;

    public ConfiguracionSistemaRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ConfiguracionSistema?> GetActivaAsync()
    {
        return await _db.ConfiguracionesSistema
            .Include(x => x.CortesPago.OrderBy(c => c.Id))
            .ThenInclude(x => x.FechasCorte.OrderBy(f => f.Orden))
            .Where(x => x.Activo)
            .OrderByDescending(x => x.FechaActualizacion)
            .FirstOrDefaultAsync();
    }

    public async Task<ConfiguracionSistema> CreateDefaultAsync()
    {
        var config = new ConfiguracionSistema
        {
            DiasCuatrimestreBase = 120,
            DiasMesBase = 30,
            ComisionEmpresaPorcentaje = 5m,
            UsarDiasExactosPrimerCuatrimestre = true,
            AplicarReglaAnioBisiesto = true,
            Activo = true,
            FechaActualizacion = DateTime.UtcNow
        };

        _db.ConfiguracionesSistema.Add(config);
        await _db.SaveChangesAsync();
        return config;
    }

    public async Task UpdateAsync(ConfiguracionSistema configuracion)
    {
        _db.ConfiguracionesSistema.Update(configuracion);
        await _db.SaveChangesAsync();
    }

    public async Task<ConfiguracionCortePago?> GetCorteByIdAsync(int corteId)
    {
        return await _db.ConfiguracionesCortePago
            .Include(x => x.FechasCorte.OrderBy(f => f.Orden))
            .FirstOrDefaultAsync(x => x.Id == corteId);
    }

    public async Task<ConfiguracionCortePago> AddCorteAsync(ConfiguracionCortePago corte)
    {
        _db.ConfiguracionesCortePago.Add(corte);
        await _db.SaveChangesAsync();
        return corte;
    }

    public async Task UpdateCorteAsync(ConfiguracionCortePago corte)
    {
        _db.ConfiguracionesCortePago.Update(corte);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCorteAsync(ConfiguracionCortePago corte)
    {
        _db.ConfiguracionesCortePago.Remove(corte);
        await _db.SaveChangesAsync();
    }
}
