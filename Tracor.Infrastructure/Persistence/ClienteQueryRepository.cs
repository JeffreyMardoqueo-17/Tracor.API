using Microsoft.EntityFrameworkCore;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Application.DTOs;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using Tradecorp.Infrastructure.Data;

namespace Tradecorp.Infrastructure.Persistence;

/// <summary>
/// Read model de clientes optimizado para consultas.
/// Evita acoplar el alta y los listados de clientes al agregado Contrato.
/// </summary>
public class ClienteQueryRepository : IClienteQueryRepository
{
    private readonly AppDbContext _dbContext;

    public ClienteQueryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ClienteDetalleResponse?> GetDetalleByIdAsync(int clienteId)
    {
        var resumen = await GetResumenByIdAsync(clienteId);
        return resumen is null ? null : await BuildDetalleAsync(resumen);
    }

    public async Task<ClienteDetalleResponse?> GetDetalleByCodigoAsync(string codigoCliente)
    {
        var resumen = await BuildResumenesAsync(
            _dbContext.Clientes
                .AsNoTracking()
                .Where(c => c.CodigoCliente == codigoCliente));

        var cliente = resumen.SingleOrDefault();
        return cliente is null ? null : await BuildDetalleAsync(cliente);
    }

    public async Task<IReadOnlyCollection<ClienteResumenResponse>> GetActivosAsync()
    {
        return await BuildResumenesAsync(
            _dbContext.Clientes
                .AsNoTracking()
                .Where(c => c.Activo)
                .OrderBy(c => c.NombreCompleto));
    }

    public async Task<IReadOnlyCollection<ClienteResumenResponse>> GetByEjecutivoIdAsync(int ejecutivoId)
    {
        return await BuildResumenesAsync(
            _dbContext.Clientes
                .AsNoTracking()
                .Where(c => c.UsuarioEjecutivoId == ejecutivoId && c.Activo)
                .OrderBy(c => c.NombreCompleto));
    }

    public async Task<ClienteResumenResponse?> GetResumenByIdAsync(int clienteId)
    {
        var resumenes = await BuildResumenesAsync(
            _dbContext.Clientes
                .AsNoTracking()
                .Where(c => c.Id == clienteId));

        return resumenes.SingleOrDefault();
    }

    private async Task<List<ClienteResumenResponse>> BuildResumenesAsync(IQueryable<Cliente> query)
    {
        var resumenes = await query
            .Select(c => new ClienteResumenResponse
            {
                Id = c.Id,
                CodigoCliente = c.CodigoCliente,
                NombreCompleto = c.NombreCompleto,
                TipoDocumento = c.TipoDocumento,
                NumeroDocumento = c.NumeroDocumento,
                TipoPersona = c.TipoPersona,
                Correo = c.Correo,
                Telefono = c.Telefono,
                Notas = c.Notas,
                Activo = c.Activo,
                UsuarioEjecutivoId = c.UsuarioEjecutivoId,
                NombreEjecutivo = c.UsuarioEjecutivo.Nombre,
                TieneContratoActivo = _dbContext.Contratos.Any(ct =>
                    ct.ClienteId == c.Id &&
                    ct.Estado == EstadoContrato.Activo),
                CantidadBeneficiariosActivos = c.Beneficiarios.Count(b =>
                    b.Estado == ClienteBeneficiarioEstado.Activo),
                SumaPorcentajeBeneficiarios = c.Beneficiarios
                    .Where(b => b.Estado == ClienteBeneficiarioEstado.Activo)
                    .Select(b => (decimal?)b.Porcentaje)
                    .Sum() ?? 0m
            })
            .ToListAsync();

        await AttachCuentaPrincipalAsync(resumenes);
        return resumenes;
    }

    private async Task AttachCuentaPrincipalAsync(List<ClienteResumenResponse> clientes)
    {
        if (clientes.Count == 0)
            return;

        var clienteIds = clientes.Select(c => c.Id).ToList();
        var cuentasPrincipales = await _dbContext.ClienteCuentas
            .AsNoTracking()
            .Where(cc => clienteIds.Contains(cc.ClienteId) && cc.EsPrincipal)
            .Select(cc => new
            {
                cc.ClienteId,
                Cuenta = new ClienteCuentaResponse
                {
                    Id = cc.Id,
                    NombreBanco = cc.Banco.Nombre,
                    NumeroCuenta = cc.NumeroCuenta,
                    TipoCuenta = cc.TipoCuenta,
                    EsPrincipal = cc.EsPrincipal
                }
            })
            .ToListAsync();

        var cuentasPorCliente = cuentasPrincipales
            .GroupBy(x => x.ClienteId)
            .ToDictionary(x => x.Key, x => x.First().Cuenta);

        foreach (var cliente in clientes)
        {
            if (cuentasPorCliente.TryGetValue(cliente.Id, out var cuenta))
                cliente.CuentaBancariaPrincipal = cuenta;
        }
    }

    private async Task<ClienteDetalleResponse> BuildDetalleAsync(ClienteResumenResponse resumen)
    {
        var cuentas = await _dbContext.ClienteCuentas
            .AsNoTracking()
            .Where(cc => cc.ClienteId == resumen.Id)
            .OrderByDescending(cc => cc.EsPrincipal)
            .ThenBy(cc => cc.Id)
            .Select(cc => new ClienteCuentaResponse
            {
                Id = cc.Id,
                NombreBanco = cc.Banco.Nombre,
                NumeroCuenta = cc.NumeroCuenta,
                TipoCuenta = cc.TipoCuenta,
                EsPrincipal = cc.EsPrincipal
            })
            .ToListAsync();

        var beneficiarios = await _dbContext.ClientesBeneficiarios
            .AsNoTracking()
            .Where(b => b.ClienteId == resumen.Id)
            .OrderByDescending(b => b.Estado == ClienteBeneficiarioEstado.Activo)
            .ThenBy(b => b.NombreCompleto)
            .ToListAsync();

        var contratos = await _dbContext.Contratos
            .AsNoTracking()
            .Where(c => c.ClienteId == resumen.Id)
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new ContratoResumenResponse
            {
                Id = c.Id,
                NumeroContrato = c.NumeroContrato,
                CapitalInicial = c.CapitalInicial,
                PorcentajeMensual = c.PorcentajeMensual,
                Activo = c.Estado == EstadoContrato.Activo
            })
            .ToListAsync();

        return new ClienteDetalleResponse
        {
            Id = resumen.Id,
            CodigoCliente = resumen.CodigoCliente,
            NombreCompleto = resumen.NombreCompleto,
            TipoDocumento = resumen.TipoDocumento,
            NumeroDocumento = resumen.NumeroDocumento,
            TipoPersona = resumen.TipoPersona,
            Correo = resumen.Correo,
            Telefono = resumen.Telefono,
            Notas = resumen.Notas,
            Activo = resumen.Activo,
            UsuarioEjecutivoId = resumen.UsuarioEjecutivoId,
            NombreEjecutivo = resumen.NombreEjecutivo,
            CuentaBancariaPrincipal = resumen.CuentaBancariaPrincipal,
            TieneContratoActivo = resumen.TieneContratoActivo,
            CantidadBeneficiariosActivos = resumen.CantidadBeneficiariosActivos,
            SumaPorcentajeBeneficiarios = resumen.SumaPorcentajeBeneficiarios,
            CuentasBancarias = cuentas,
            Beneficiarios = beneficiarios.Select(MapBeneficiario).ToList(),
            Contratos = contratos
        };
    }

    private static ClienteBeneficiarioResponse MapBeneficiario(ClienteBeneficiario beneficiario)
    {
        return new ClienteBeneficiarioResponse
        {
            Id = beneficiario.Id,
            NombreCompleto = beneficiario.NombreCompleto,
            DUI = beneficiario.DUI,
            Correo = beneficiario.Correo,
            Telefono = beneficiario.Telefono,
            Direccion = beneficiario.Direccion,
            Porcentaje = beneficiario.Porcentaje,
            TipoRelacion = beneficiario.TipoRelacion.ToString(),
            Estado = beneficiario.Estado.ToString(),
            Notas = beneficiario.Notas,
            FechaCreacion = beneficiario.FechaCreacion,
            FechaActualizacion = beneficiario.FechaActualizacion
        };
    }
}
