using Microsoft.EntityFrameworkCore;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using Tradecorp.Domain.Models.ValueObjects;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Application.DTOs;
using Tradecorp.Infrastructure.Data;

namespace Tradecorp.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio para Contratos usando EF Core.
/// </summary>
public class ContratoRepository : IContratoRepository
{
    private readonly AppDbContext _dbContext;

    public ContratoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<bool> ClienteExisteAsync(int clienteId)
    {
        return await _dbContext.Clientes.AnyAsync(c => c.Id == clienteId);
    }

    public async Task<bool> ClienteTieneContratosActivosAsync(int clienteId)
    {
        return await _dbContext.Contratos.AnyAsync(c =>
            c.ClienteId == clienteId &&
            c.Estado == EstadoContrato.Activo);
    }

    public async Task<bool> ContratoExisteAsync(int contratoId)
    {
        return await _dbContext.Contratos.AnyAsync(c => c.Id == contratoId);
    }

    public async Task<bool> ContratoActivoAsync(int contratoId)
    {
        return await _dbContext.Contratos.AnyAsync(c => c.Id == contratoId && c.Estado == EstadoContrato.Activo);
    }

    public async Task<int> ObtenerClienteIdContratoAsync(int contratoId)
    {
        return await _dbContext.Contratos
            .Where(c => c.Id == contratoId)
            .Select(c => c.ClienteId)
            .FirstAsync();
    }

    public async Task<string> GenerarNumeroContratoAsync(int clienteId)
    {
        var ultimoNumero = await _dbContext.Contratos
            .Where(c => c.ClienteId == clienteId)
            .OrderByDescending(c => c.Id)
            .Select(c => c.NumeroContrato)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(ultimoNumero))
            return $"CTR-{clienteId:D6}-0001";

        var partes = ultimoNumero.Split('-');
        if (partes.Length >= 3 && int.TryParse(partes[^1], out var secuencia))
            return $"CTR-{clienteId:D6}-{(secuencia + 1):D4}";

        return $"CTR-{clienteId:D6}-0001";
    }

    public async Task<int> CrearContratoAsync(CreateContratoRequest request, int usuarioId, string numeroContrato)
    {
        var modalidad = Enum.TryParse<ModalidadRendimiento>(request.ModalidadRendimiento, out var parsedModalidad)
            ? parsedModalidad
            : ModalidadRendimiento.Normal;

        var contrato = Contrato.Crear(
            request.ClienteId,
            numeroContrato,
            request.FechaInicio ?? DateOnly.FromDateTime(DateTime.UtcNow),
            request.CapitalInicial,
            request.PorcentajeMensual,
            request.ComisionRetiro ?? 0m,
            modalidad,
            request.PermiteUnificacion);

        _dbContext.Contratos.Add(contrato);
        await _dbContext.SaveChangesAsync();

        if (request.Beneficiarios is { Count: > 0 })
            await AsignarBeneficiariosAsync(contrato.Id, request.Beneficiarios, usuarioId);

        return contrato.Id;
    }

    public async Task<bool> ActualizarContratoAsync(int contratoId, UpdateContratoRequest request, int usuarioId)
    {
        var contrato = await _dbContext.Contratos.FirstOrDefaultAsync(c => c.Id == contratoId);
        if (contrato is null || contrato.Estado != EstadoContrato.Activo)
            return false;

        if (request.PorcentajeMensual.HasValue)
            contrato.CambiarPorcentajeMensual(request.PorcentajeMensual.Value);

        if (request.ComisionRetiro.HasValue)
            _dbContext.Entry(contrato).Property(nameof(Contrato.ComisionRetiro)).CurrentValue = request.ComisionRetiro.Value;

        if (request.ModalidadRendimiento is not null && Enum.TryParse<ModalidadRendimiento>(request.ModalidadRendimiento, out var modalidad))
            _dbContext.Entry(contrato).Property(nameof(Contrato.ModalidadRendimiento)).CurrentValue = modalidad;

        if (request.PermiteUnificacion.HasValue)
            _dbContext.Entry(contrato).Property(nameof(Contrato.PermiteUnificacion)).CurrentValue = request.PermiteUnificacion.Value;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> FinalizarContratoAsync(int contratoId, int usuarioId, string? observacion)
    {
        var contrato = await _dbContext.Contratos.FirstOrDefaultAsync(c => c.Id == contratoId);
        if (contrato is null || contrato.Estado != EstadoContrato.Activo)
            return false;

        contrato.Finalizar();
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegistrarReinversionAsync(int contratoId, ReinversionContratoRequest request, int usuarioId)
    {
        var contrato = await _dbContext.Contratos.FirstOrDefaultAsync(c => c.Id == contratoId);
        if (contrato is null || contrato.Estado != EstadoContrato.Activo)
            return false;

        var monto = request.Tipo.Equals("Parcial", StringComparison.OrdinalIgnoreCase)
            ? request.MontoReinvertir ?? 0m
            : contrato.CapitalActual;

        if (monto <= 0m)
            return false;

        contrato.Reinvertir(new Capital(monto), request.MontoReinvertir.HasValue ? request.MontoReinvertir.Value : null);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegistrarInyeccionCapitalAsync(int contratoId, InyeccionCapitalRequest request, int usuarioId)
    {
        var contrato = await _dbContext.Contratos.FirstOrDefaultAsync(c => c.Id == contratoId);
        if (contrato is null || contrato.Estado != EstadoContrato.Activo)
            return false;

        contrato.InyectarCapital(new Capital(request.Monto));
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnificarContratosAsync(int contratoDestinoId, UnificarContratosRequest request, int usuarioId)
    {
        var destino = await _dbContext.Contratos.FirstOrDefaultAsync(c => c.Id == contratoDestinoId);
        if (destino is null || destino.Estado != EstadoContrato.Activo)
            return false;

        var origenes = await _dbContext.Contratos
            .Where(c => request.ContratosOrigenIds.Contains(c.Id) && c.Id != contratoDestinoId)
            .ToListAsync();

        if (origenes.Count == 0)
            return false;

        var capitalTotal = origenes.Sum(c => c.CapitalActual);
        destino.InyectarCapital(new Capital(capitalTotal), request.AprobacionGerencial);

        foreach (var origen in origenes)
        {
            origen.MarcarComoUnificado(request.AprobacionGerencial);
            _dbContext.ContratoRelaciones.Add(new ContratoRelacion
            {
                ContratoOrigenId = origen.Id,
                ContratoDestinoId = destino.Id,
                TipoRelacion = TipoRelacionContrato.Unificacion,
                MontoTransferido = origen.CapitalActual,
                UsuarioId = usuarioId,
                Observacion = request.Observacion
            });
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesunificarContratoAsync(int contratoId, DesunificarContratoRequest request, int usuarioId)
    {
        var contrato = await _dbContext.Contratos.FirstOrDefaultAsync(c => c.Id == contratoId);
        if (contrato is null)
            return false;

        _dbContext.Entry(contrato).Property(nameof(Contrato.PermiteUnificacion)).CurrentValue = false;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<BeneficiarioContratoResponse>> AsignarBeneficiariosAsync(int contratoId, List<AsignarBeneficiarioRequest> beneficiarios, int usuarioId)
    {
        var contrato = await _dbContext.Contratos
            .Include(c => c.Beneficiarios)
            .FirstOrDefaultAsync(c => c.Id == contratoId);

        if (contrato is null)
            return Array.Empty<BeneficiarioContratoResponse>();

        _dbContext.ContratoBeneficiarios.RemoveRange(contrato.Beneficiarios);

        foreach (var item in beneficiarios)
        {
            var beneficiarioId = item.ClienteBeneficiarioId ?? 0;
            if (beneficiarioId <= 0 && item.Beneficiario is null)
                continue;

            if (beneficiarioId <= 0 && item.Beneficiario is not null)
            {
                var tipoRelacion = Enum.TryParse<ClienteBeneficiarioTipo>(item.Beneficiario.TipoRelacion, out var parsedTipoRelacion)
                    ? parsedTipoRelacion
                    : ClienteBeneficiarioTipo.Otro;

                var nuevoBeneficiario = new ClienteBeneficiario
                {
                    ClienteId = contrato.ClienteId,
                    NombreCompleto = item.Beneficiario.NombreCompleto,
                    DUI = item.Beneficiario.DUI,
                    Correo = item.Beneficiario.Correo,
                    Telefono = item.Beneficiario.Telefono,
                    Direccion = item.Beneficiario.Direccion,
                    TipoRelacion = tipoRelacion,
                    Estado = ClienteBeneficiarioEstado.Activo
                };

                _dbContext.ClientesBeneficiarios.Add(nuevoBeneficiario);
                await _dbContext.SaveChangesAsync();
                beneficiarioId = nuevoBeneficiario.Id;
            }

            _dbContext.ContratoBeneficiarios.Add(new ContratoBeneficiario
            {
                ContratoId = contratoId,
                ClienteBeneficiarioId = beneficiarioId,
                PorcentajeAsignado = item.Porcentaje
            });
        }

        await _dbContext.SaveChangesAsync();
        return await ObtenerBeneficiariosContratoAsync(contratoId);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerContratosAsync(ContratoFiltroRequest? filtro)
    {
        var query = _dbContext.Contratos.AsNoTracking().Include(c => c.Cliente).AsQueryable();

        if (filtro?.ClienteId is not null)
            query = query.Where(c => c.ClienteId == filtro.ClienteId.Value);

        if (!string.IsNullOrWhiteSpace(filtro?.Estado) && Enum.TryParse<EstadoContrato>(filtro.Estado, out var estado))
            query = query.Where(c => c.Estado == estado);

        if (!string.IsNullOrWhiteSpace(filtro?.NumeroContrato))
            query = query.Where(c => c.NumeroContrato.Contains(filtro.NumeroContrato));

        var contratos = await query.OrderByDescending(c => c.FechaCreacion).ToListAsync();
        return contratos.Select(MapearLista).ToList();
    }

    public Task<IEnumerable<ContratoListaResponse>> ObtenerContratosClienteAsync(int clienteId)
    {
        return ObtenerContratosAsync(new ContratoFiltroRequest { ClienteId = clienteId });
    }

    public Task<IEnumerable<ContratoListaResponse>> ObtenerContratosActivosClienteAsync(int clienteId)
    {
        return ObtenerContratosAsync(new ContratoFiltroRequest { ClienteId = clienteId, Estado = EstadoContrato.Activo.ToString() });
    }

    public Task<IEnumerable<ContratoListaResponse>> ObtenerTodosContratosActivosAsync()
    {
        return ObtenerContratosAsync(new ContratoFiltroRequest { Estado = EstadoContrato.Activo.ToString() });
    }

    public async Task<ContratoResponse?> ObtenerContratoDetalleAsync(int contratoId)
    {
        var contrato = await _dbContext.Contratos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Cliente)
            .Include(c => c.Beneficiarios)
                .ThenInclude(b => b.ClienteBeneficiario)
            .Include(c => c.RelacionesComoOrigen)
            .Include(c => c.RelacionesComoDestino)
            .FirstOrDefaultAsync(c => c.Id == contratoId);

        return contrato is null ? null : MapearDetalle(contrato);
    }

    public async Task<IEnumerable<BeneficiarioContratoResponse>> ObtenerBeneficiariosContratoAsync(int contratoId)
    {
        var beneficiarios = await _dbContext.ContratoBeneficiarios
            .AsNoTracking()
            .Include(x => x.ClienteBeneficiario)
            .Where(x => x.ContratoId == contratoId)
            .ToListAsync();

        return beneficiarios.Select(x => new BeneficiarioContratoResponse
        {
            Id = x.Id,
            ClienteBeneficiarioId = x.ClienteBeneficiarioId,
            NombreCompleto = x.ClienteBeneficiario?.NombreCompleto ?? string.Empty,
            DUI = x.ClienteBeneficiario?.DUI,
            TipoRelacion = x.ClienteBeneficiario?.TipoRelacion.ToString() ?? string.Empty,
            Estado = x.ClienteBeneficiario?.Estado.ToString() ?? string.Empty,
            Porcentaje = x.PorcentajeAsignado
        }).ToList();
    }

    public Task<IEnumerable<HistorialFinancieroItemResponse>> ObtenerHistorialFinancieroAsync(int contratoId)
    {
        return Task.FromResult<IEnumerable<HistorialFinancieroItemResponse>>(Array.Empty<HistorialFinancieroItemResponse>());
    }

    public Task<IEnumerable<ContratoEventoResponse>> ObtenerEventosAsync(int contratoId)
    {
        return Task.FromResult<IEnumerable<ContratoEventoResponse>>(Array.Empty<ContratoEventoResponse>());
    }

    public Task<IEnumerable<AuditoriaContratoResponse>> ObtenerAuditoriaAsync(int contratoId)
    {
        return Task.FromResult<IEnumerable<AuditoriaContratoResponse>>(Array.Empty<AuditoriaContratoResponse>());
    }

    private static ContratoListaResponse MapearLista(Contrato contrato)
    {
        return new ContratoListaResponse
        {
            Id = contrato.Id,
            ClienteId = contrato.ClienteId,
            ClienteNombre = contrato.Cliente?.NombreCompleto ?? string.Empty,
            NumeroContrato = contrato.NumeroContrato,
            CapitalInicial = contrato.CapitalInicial,
            CapitalActual = contrato.CapitalActual,
            PorcentajeMensual = contrato.PorcentajeMensual,
            ModalidadRendimiento = contrato.ModalidadRendimiento.ToString(),
            PermiteUnificacion = contrato.PermiteUnificacion,
            Estado = contrato.Estado.ToString(),
            FechaInicio = contrato.FechaInicio,
            FechaCreacion = contrato.FechaCreacion,
            FechaCierre = contrato.FechaCierre
        };
    }

    private static ContratoResponse MapearDetalle(Contrato contrato)
    {
        return new ContratoResponse
        {
            Id = contrato.Id,
            ClienteId = contrato.ClienteId,
            ClienteNombre = contrato.Cliente?.NombreCompleto ?? string.Empty,
            NumeroContrato = contrato.NumeroContrato,
            FechaInicio = contrato.FechaInicio,
            CapitalInicial = contrato.CapitalInicial,
            CapitalActual = contrato.CapitalActual,
            PorcentajeMensual = contrato.PorcentajeMensual,
            ComisionRetiro = contrato.ComisionRetiro,
            ModalidadRendimiento = contrato.ModalidadRendimiento.ToString(),
            PermiteUnificacion = contrato.PermiteUnificacion,
            Estado = contrato.Estado.ToString(),
            FechaCreacion = contrato.FechaCreacion,
            FechaActualizacion = contrato.FechaCreacion,
            FechaCierre = contrato.FechaCierre,
            Beneficiarios = contrato.Beneficiarios.Select(b => new BeneficiarioContratoResponse
            {
                Id = b.Id,
                ClienteBeneficiarioId = b.ClienteBeneficiarioId,
                NombreCompleto = b.ClienteBeneficiario?.NombreCompleto ?? string.Empty,
                DUI = b.ClienteBeneficiario?.DUI,
                TipoRelacion = b.ClienteBeneficiario?.TipoRelacion.ToString() ?? string.Empty,
                Estado = b.ClienteBeneficiario?.Estado.ToString() ?? string.Empty,
                Porcentaje = b.PorcentajeAsignado
            }).ToList()
        };
    }

    public async Task<Contrato?> GetByIdAsync(int id)
    {
        return await _dbContext.Contratos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Cliente)
            .Include(c => c.Beneficiarios)
                .ThenInclude(b => b.ClienteBeneficiario)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Contrato?> GetByNumeroAsync(string numero)
    {
        return await _dbContext.Contratos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.NumeroContrato == numero);
    }

    public async Task<IEnumerable<Contrato>> GetActivosPorClienteAsync(int clienteId)
    {
        return await _dbContext.Contratos
            .Where(c => c.ClienteId == clienteId && c.Estado == EstadoContrato.Activo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Contrato>> GetPorClienteAsync(int clienteId)
    {
        return await _dbContext.Contratos
            .Where(c => c.ClienteId == clienteId)
            .AsNoTracking()
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task<Contrato> CreateAsync(Contrato contrato)
    {
        _dbContext.Contratos.Add(contrato);
        await _dbContext.SaveChangesAsync();
        return await GetByIdAsync(contrato.Id) ?? contrato;
    }

    public async Task<Contrato> UpdateAsync(Contrato contrato)
    {
        _dbContext.Contratos.Update(contrato);
        await _dbContext.SaveChangesAsync();
        return contrato;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contrato = await _dbContext.Contratos.FindAsync(id);
        if (contrato == null)
            return false;

        _dbContext.Contratos.Remove(contrato);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<string> GetProximoNumeroContratoAsync()
    {
        var ultimoContrato = await _dbContext.Contratos
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        if (ultimoContrato == null)
            return "CNT-001";

        var parts = ultimoContrato.NumeroContrato.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out var numero))
        {
            return $"CNT-{(numero + 1):D3}";
        }

        return "CNT-001";
    }
}

/// <summary>
/// Implementación del repositorio para relaciones entre contratos.
/// </summary>
public class ContratoRelacionRepository : IContratoRelacionRepository
{
    private readonly AppDbContext _dbContext;

    public ContratoRelacionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ContratoRelacion> CreateAsync(ContratoRelacion relacion)
    {
        _dbContext.ContratoRelaciones.Add(relacion);
        await _dbContext.SaveChangesAsync();
        return relacion;
    }

    public async Task<IEnumerable<ContratoRelacion>> GetPorContratoAsync(int contratoId)
    {
        return await _dbContext.ContratoRelaciones
            .Where(r => r.ContratoOrigenId == contratoId || r.ContratoDestinoId == contratoId)
            .AsNoTracking()
            .ToListAsync();
    }
}

/// <summary>
/// Implementación del repositorio para auditoría de contratos.
/// </summary>
public class AuditoriaContratoRepository : IAuditoriaContratoRepository
{
    private readonly AppDbContext _dbContext;

    public AuditoriaContratoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<AuditoriaContrato> CreateAsync(AuditoriaContrato auditoria)
    {
        _dbContext.AuditoriaContratos.Add(auditoria);
        await _dbContext.SaveChangesAsync();
        return auditoria;
    }

    public async Task<IEnumerable<AuditoriaContrato>> GetPorContratoAsync(int contratoId)
    {
        return await _dbContext.AuditoriaContratos
            .Where(a => a.ContratoId == contratoId)
            .AsNoTracking()
            .OrderByDescending(a => a.FechaMovimiento)
            .ToListAsync();
    }
}

/// <summary>
/// Implementación del repositorio para beneficiarios por contrato.
/// </summary>
public class ContratoBeneficiarioRepository : IContratoBeneficiarioRepository
{
    private readonly AppDbContext _dbContext;

    public ContratoBeneficiarioRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ContratoBeneficiario> CreateAsync(ContratoBeneficiario beneficiario)
    {
        _dbContext.ContratoBeneficiarios.Add(beneficiario);
        await _dbContext.SaveChangesAsync();
        return beneficiario;
    }

    public async Task<IEnumerable<ContratoBeneficiario>> GetPorContratoAsync(int contratoId)
    {
        return await _dbContext.ContratoBeneficiarios
            .Where(b => b.ContratoId == contratoId)
            .AsNoTracking()
            .Include(b => b.ClienteBeneficiario)
            .ToListAsync();
    }

    public async Task<decimal> GetSumaPorcentajesAsync(int contratoId)
    {
        return await _dbContext.ContratoBeneficiarios
            .Where(b => b.ContratoId == contratoId)
            .SumAsync(b => b.PorcentajeAsignado);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var beneficiario = await _dbContext.ContratoBeneficiarios.FindAsync(id);
        if (beneficiario == null)
            return false;

        _dbContext.ContratoBeneficiarios.Remove(beneficiario);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
