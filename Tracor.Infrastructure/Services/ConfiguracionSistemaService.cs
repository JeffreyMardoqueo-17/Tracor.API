using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;
using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Infrastructure.Services;

public class ConfiguracionSistemaService : IConfiguracionSistemaService
{
    private readonly IConfiguracionSistemaRepository _repo;

    public ConfiguracionSistemaService(IConfiguracionSistemaRepository repo)
    {
        _repo = repo;
    }

    public async Task<SistemaConfiguracionDto> GetSistemaAsync()
    {
        var config = await GetOrCreateConfiguracionActivaAsync();
        return ToSistemaDto(config);
    }

    public async Task<SistemaConfiguracionDto> UpdateSistemaAsync(UpdateSistemaConfiguracionRequestDto dto)
    {
        ValidateSistema(dto);

        var config = await GetOrCreateConfiguracionActivaAsync();
        config.DiasCuatrimestreBase = dto.DiasCuatrimestreBase;
        config.DiasMesBase = dto.DiasMesBase;
        config.ComisionEmpresaPorcentaje = dto.ComisionEmpresaPorcentaje;
        config.UsarDiasExactosPrimerCuatrimestre = dto.UsarDiasExactosPrimerCuatrimestre;
        config.AplicarReglaAnioBisiesto = dto.AplicarReglaAnioBisiesto;
        config.Activo = dto.Activo;
        config.FechaActualizacion = DateTime.UtcNow;

        await _repo.UpdateAsync(config);
        return ToSistemaDto(config);
    }

    public async Task<IReadOnlyCollection<CortePagoDto>> GetCortesAsync()
    {
        var config = await GetOrCreateConfiguracionActivaAsync();
        return config.CortesPago
            .OrderBy(x => x.Id)
            .Select(ToCorteDto)
            .ToArray();
    }

    public async Task<CortePagoDto> CreateCorteAsync(UpsertCortePagoRequestDto dto)
    {
        ValidateCorte(dto);

        var config = await GetOrCreateConfiguracionActivaAsync();
        var corte = new ConfiguracionCortePago
        {
            ConfiguracionSistemaId = config.Id,
            Nombre = dto.Nombre.Trim(),
            Activo = dto.Activo,
            FechasCorte = dto.Fechas
                .OrderBy(x => x.Orden)
                .Select(x => new FechaCortePago
                {
                    Orden = x.Orden,
                    Dia = x.Dia,
                    Mes = x.Mes
                })
                .ToList()
        };

        var created = await _repo.AddCorteAsync(corte);
        return ToCorteDto(created);
    }

    public async Task<CortePagoDto> UpdateCorteAsync(int corteId, UpsertCortePagoRequestDto dto)
    {
        ValidateCorte(dto);

        var corte = await _repo.GetCorteByIdAsync(corteId)
            ?? throw new KeyNotFoundException("Corte de pago no encontrado.");

        corte.Nombre = dto.Nombre.Trim();
        corte.Activo = dto.Activo;

        corte.FechasCorte.Clear();
        foreach (var fecha in dto.Fechas.OrderBy(x => x.Orden))
        {
            corte.FechasCorte.Add(new FechaCortePago
            {
                Orden = fecha.Orden,
                Dia = fecha.Dia,
                Mes = fecha.Mes
            });
        }

        await _repo.UpdateCorteAsync(corte);
        return ToCorteDto(corte);
    }

    public async Task DeleteCorteAsync(int corteId)
    {
        var corte = await _repo.GetCorteByIdAsync(corteId)
            ?? throw new KeyNotFoundException("Corte de pago no encontrado.");

        await _repo.DeleteCorteAsync(corte);
    }

    private async Task<ConfiguracionSistema> GetOrCreateConfiguracionActivaAsync()
    {
        return await _repo.GetActivaAsync() ?? await _repo.CreateDefaultAsync();
    }

    private static void ValidateSistema(UpdateSistemaConfiguracionRequestDto dto)
    {
        if (dto.DiasCuatrimestreBase <= 0)
        {
            throw new InvalidOperationException("DiasCuatrimestreBase debe ser mayor a 0.");
        }

        if (dto.DiasMesBase <= 0)
        {
            throw new InvalidOperationException("DiasMesBase debe ser mayor a 0.");
        }

        if (dto.ComisionEmpresaPorcentaje < 0)
        {
            throw new InvalidOperationException("ComisionEmpresaPorcentaje no puede ser negativa.");
        }
    }

    private static void ValidateCorte(UpsertCortePagoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            throw new InvalidOperationException("El nombre del corte es requerido.");
        }

        if (dto.Fechas is null || dto.Fechas.Count == 0)
        {
            throw new InvalidOperationException("Debe incluir al menos una fecha de corte.");
        }

        var ordenes = new HashSet<int>();
        var fechas = new HashSet<string>();

        foreach (var fecha in dto.Fechas)
        {
            if (fecha.Orden <= 0)
            {
                throw new InvalidOperationException("El orden de cada fecha debe ser mayor a 0.");
            }

            if (fecha.Mes is < 1 or > 12)
            {
                throw new InvalidOperationException("El mes de cada fecha debe estar entre 1 y 12.");
            }

            if (fecha.Dia is < 1 or > 31)
            {
                throw new InvalidOperationException("El dia de cada fecha debe estar entre 1 y 31.");
            }

            if (!ordenes.Add(fecha.Orden))
            {
                throw new InvalidOperationException("No se permiten ordenes de fecha duplicados.");
            }

            var key = $"{fecha.Mes}-{fecha.Dia}";
            if (!fechas.Add(key))
            {
                throw new InvalidOperationException("No se permiten fechas de corte repetidas.");
            }
        }
    }

    private static SistemaConfiguracionDto ToSistemaDto(ConfiguracionSistema config)
    {
        return new SistemaConfiguracionDto(
            config.Id,
            config.DiasCuatrimestreBase,
            config.DiasMesBase,
            config.ComisionEmpresaPorcentaje,
            config.UsarDiasExactosPrimerCuatrimestre,
            config.AplicarReglaAnioBisiesto,
            config.Activo,
            config.FechaActualizacion,
            config.CortesPago
                .OrderBy(x => x.Id)
                .Select(ToCorteDto)
                .ToArray());
    }

    private static CortePagoDto ToCorteDto(ConfiguracionCortePago corte)
    {
        return new CortePagoDto(
            corte.Id,
            corte.Nombre,
            corte.Activo,
            corte.FechasCorte
                .OrderBy(x => x.Orden)
                .Select(x => new FechaCorteDto(x.Id, x.Orden, x.Dia, x.Mes))
                .ToArray());
    }
}
