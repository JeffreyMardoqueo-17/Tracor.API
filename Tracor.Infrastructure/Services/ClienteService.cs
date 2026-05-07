using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using Tradecorp.Application.DTOs;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Services;

/// <summary>
/// Servicio de negocios para Clientes
/// Aplica reglas de negocio, validaciones y orquesta operaciones
/// Principio SOLID: Single Responsibility - solo maneja lógica de cliente
/// </summary>
public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IClienteBeneficiarioRepository _beneficiarioRepository;

    public ClienteService(
        IClienteRepository clienteRepository,
        IClienteBeneficiarioRepository beneficiarioRepository)
    {
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        _beneficiarioRepository = beneficiarioRepository ?? throw new ArgumentNullException(nameof(beneficiarioRepository));
    }

    public async Task<ClienteResponse> RegistrarClienteAsync(CreateClienteRequest request, int usuarioEjecutivoId)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            throw new ArgumentException("El nombre del cliente es requerido.");

        if (!request.TipoPersona.HasValue)
            throw new ArgumentException("El tipo de persona es requerido.");

        if (!Enum.IsDefined(typeof(ClienteTipoPersona), request.TipoPersona.Value))
            throw new ArgumentException("El tipo de persona proporcionado no es válido.");

        // Validar unicidad del documento si se proporciona
        if (!string.IsNullOrEmpty(request.NumeroDocumento))
        {
            var existe = await _clienteRepository.ExistsByNumeroDocumentoAsync(request.NumeroDocumento);
            if (existe)
                throw new InvalidOperationException($"Ya existe un cliente con el documento {request.NumeroDocumento}.");
        }

        // Crear entidad Cliente
        var cliente = new Cliente
        {
            NombreCompleto = request.NombreCompleto.Trim(),
            TipoDocumento = request.TipoDocumento,
            NumeroDocumento = request.NumeroDocumento,
            TipoPersona = request.TipoPersona.Value,
            Correo = request.Correo,
            Telefono = request.Telefono,
            Notas = request.Notas,
            UsuarioEjecutivoId = usuarioEjecutivoId,
            Activo = true
        };

        // Guardar cliente (genera código automático)
        var clienteCreado = await _clienteRepository.CreateAsync(cliente);

        // Crear cuenta bancaria si se proporciona
        if (request.CuentaBancaria != null)
        {
            var cuenta = new ClienteCuenta
            {
                ClienteId = clienteCreado.Id,
                BancoId = request.CuentaBancaria.BancoId,
                NumeroCuenta = request.CuentaBancaria.NumeroCuenta.Trim(),
                TipoCuenta = request.CuentaBancaria.TipoCuenta,
                EsPrincipal = true // Primera cuenta es principal
            };

            clienteCreado.ClienteCuentas.Add(cuenta);
        }

        return MapearClienteAResponse(clienteCreado);
    }

    public async Task<ClienteResponse?> ObtenerClienteAsync(int clienteId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
            return null;

        return await MapearClienteCompleto(cliente);
    }

    public async Task<ClienteResponse?> ObtenerClientePorCodigoAsync(string codigoCliente)
    {
        var cliente = await _clienteRepository.GetByCodigoClienteAsync(codigoCliente);
        if (cliente == null)
            return null;

        return await MapearClienteCompleto(cliente);
    }

    public async Task<IEnumerable<ClienteResponse>> ObtenerClientesActivosAsync()
    {
        var clientes = await _clienteRepository.GetAllActivosAsync();
        var respuestas = new List<ClienteResponse>();

        foreach (var cliente in clientes)
        {
            respuestas.Add(MapearClienteAResponse(cliente));
        }

        return respuestas;
    }

    public async Task<IEnumerable<ClienteResponse>> ObtenerClientesPorEjecutivoAsync(int ejecutivoId)
    {
        var clientes = await _clienteRepository.GetByEjecutivoIdAsync(ejecutivoId);
        var respuestas = new List<ClienteResponse>();

        foreach (var cliente in clientes)
        {
            respuestas.Add(await MapearClienteCompleto(cliente));
        }

        return respuestas;
    }

    public async Task<ClienteResponse> ActualizarClienteAsync(int clienteId, UpdateClienteRequest request)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
            throw new InvalidOperationException($"Cliente con ID {clienteId} no encontrado.");

        // Actualizar solo los campos proporcionados
        if (!string.IsNullOrWhiteSpace(request.NombreCompleto))
            cliente.NombreCompleto = request.NombreCompleto.Trim();

        if (!string.IsNullOrEmpty(request.Correo))
            cliente.Correo = request.Correo.Trim();

        if (!string.IsNullOrEmpty(request.Telefono))
            cliente.Telefono = request.Telefono.Trim();

        if (!string.IsNullOrEmpty(request.Notas))
            cliente.Notas = request.Notas.Trim();

        var clienteActualizado = await _clienteRepository.UpdateAsync(cliente);
        return await MapearClienteCompleto(clienteActualizado);
    }

    public async Task<bool> DesactivarClienteAsync(int clienteId)
    {
        return await _clienteRepository.DeactivateAsync(clienteId);
    }

    public async Task<bool> TieneContratoActivoAsync(int clienteId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
            return false;

        return cliente.Contratos?.Any(c => c.Activo) ?? false;
    }

    public async Task<IEnumerable<string>> ObtenerAdvertenciasClienteAsync(int clienteId)
    {
        var advertencias = new List<string>();
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);

        if (cliente == null)
            return advertencias;

        // Advertencia: Sin contrato
        var tieneContrato = cliente.Contratos?.Any(c => c.Activo) ?? false;
        if (!tieneContrato)
        {
            advertencias.Add("⚠️ El cliente no tiene contratos activos. Los beneficiarios y pagos requerirán un contrato.");
        }

        // Advertencia: Beneficiarios incompletos
        var sumaBeneficiarios = await _beneficiarioRepository.GetSumaPorcentajeActivosAsync(clienteId);
        var cantidadBeneficiarios = await _beneficiarioRepository.GetCantidadActivosAsync(clienteId);

        if (cantidadBeneficiarios > 0 && sumaBeneficiarios < 100m)
        {
            advertencias.Add($"⚠️ Los beneficiarios suman {sumaBeneficiarios}% en lugar de 100%. Asignación incompleta.");
        }

        return advertencias;
    }

    // Métodos auxiliares privados

    private ClienteResponse MapearClienteAResponse(Cliente cliente)
    {
        return new ClienteResponse
        {
            Id = cliente.Id,
            CodigoCliente = cliente.CodigoCliente ?? string.Empty,
            NombreCompleto = cliente.NombreCompleto ?? string.Empty,
            TipoDocumento = cliente.TipoDocumento,
            NumeroDocumento = cliente.NumeroDocumento,
            TipoPersona = cliente.TipoPersona,
            Correo = cliente.Correo,
            Telefono = cliente.Telefono,
            Notas = cliente.Notas,
            Activo = cliente.Activo,
            UsuarioEjecutivoId = cliente.UsuarioEjecutivoId,
            NombreEjecutivo = cliente.UsuarioEjecutivo?.Nombre ?? string.Empty,
            CuentasBancarias = cliente.ClienteCuentas
                .Select(cc => new ClienteCuentaResponse
                {
                    Id = cc.Id,
                    NombreBanco = cc.Banco?.Nombre ?? string.Empty,
                    NumeroCuenta = cc.NumeroCuenta ?? string.Empty,
                    TipoCuenta = cc.TipoCuenta,
                    EsPrincipal = cc.EsPrincipal
                })
                .ToList(),
            Beneficiarios = cliente.Beneficiarios
                .Select(b => new ClienteBeneficiarioResponse
                {
                    Id = b.Id,
                    NombreCompleto = b.NombreCompleto ?? string.Empty,
                    DUI = b.DUI,
                    Correo = b.Correo,
                    Telefono = b.Telefono,
                    Direccion = b.Direccion,
                    Porcentaje = b.Porcentaje,
                    TipoRelacion = b.TipoRelacion.ToString(),
                    Estado = b.Estado.ToString(),
                    Notas = b.Notas,
                    FechaCreacion = b.FechaCreacion,
                    FechaActualizacion = b.FechaActualizacion
                })
                .ToList(),
            Contratos = cliente.Contratos
                .Select(c => new ContratoResumenResponse
                {
                    Id = c.Id,
                    NumeroContrato = c.NumeroContrato ?? string.Empty,
                    CapitalInicial = c.CapitalInicial,
                    PorcentajeMensual = c.PorcentajeMensual,
                    Activo = c.Activo
                })
                .ToList(),
            TieneContratoActivo = cliente.Contratos?.Any(c => c.Activo) ?? false,
            CantidadBeneficiariosActivos = cliente.Beneficiarios?.Count(b => b.Estado.ToString() == "Activo") ?? 0,
            SumaPorcentajeBeneficiarios = cliente.Beneficiarios?
                .Where(b => b.Estado.ToString() == "Activo")
                .Sum(b => b.Porcentaje) ?? 0
        };
    }

    private async Task<ClienteResponse> MapearClienteCompleto(Cliente cliente)
    {
        var sumaBeneficiarios = await _beneficiarioRepository.GetSumaPorcentajeActivosAsync(cliente.Id);

        var response = MapearClienteAResponse(cliente);
        response.SumaPorcentajeBeneficiarios = sumaBeneficiarios;

        return response;
    }
}
