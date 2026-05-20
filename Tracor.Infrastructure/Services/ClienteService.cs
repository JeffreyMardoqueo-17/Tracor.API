using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Infrastructure.Services;

/// <summary>
/// Servicio de aplicación para clientes.
/// Separa comandos del agregado Cliente de las consultas de lectura optimizadas.
/// </summary>
public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IClienteQueryRepository _clienteQueryRepository;
    private readonly IClienteBeneficiarioRepository _beneficiarioRepository;
    private readonly IContratoRepository _contratoRepository;

    public ClienteService(
        IClienteRepository clienteRepository,
        IClienteQueryRepository clienteQueryRepository,
        IClienteBeneficiarioRepository beneficiarioRepository,
        IContratoRepository contratoRepository)
    {
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        _clienteQueryRepository = clienteQueryRepository ?? throw new ArgumentNullException(nameof(clienteQueryRepository));
        _beneficiarioRepository = beneficiarioRepository ?? throw new ArgumentNullException(nameof(beneficiarioRepository));
        _contratoRepository = contratoRepository ?? throw new ArgumentNullException(nameof(contratoRepository));
    }

    public async Task<CreateClienteResponse> RegistrarClienteAsync(CreateClienteRequest request, int usuarioEjecutivoId)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCompleto))
            throw new ArgumentException("El nombre del cliente es requerido.");

        if (!request.TipoPersona.HasValue)
            throw new ArgumentException("El tipo de persona es requerido.");

        if (!Enum.IsDefined(typeof(ClienteTipoPersona), request.TipoPersona.Value))
            throw new ArgumentException("El tipo de persona proporcionado no es válido.");

        if (!string.IsNullOrEmpty(request.NumeroDocumento))
        {
            var existe = await _clienteRepository.ExistsByNumeroDocumentoAsync(request.NumeroDocumento);
            if (existe)
                throw new InvalidOperationException($"Ya existe un cliente con el documento {request.NumeroDocumento}.");
        }

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

        if (request.CuentaBancaria != null)
        {
            cliente.ClienteCuentas.Add(new ClienteCuenta
            {
                BancoId = request.CuentaBancaria.BancoId,
                NumeroCuenta = request.CuentaBancaria.NumeroCuenta.Trim(),
                TipoCuenta = request.CuentaBancaria.TipoCuenta,
                EsPrincipal = true
            });
        }

        var clienteCreado = await _clienteRepository.CreateAsync(cliente);

        return new CreateClienteResponse
        {
            Id = clienteCreado.Id,
            CodigoCliente = clienteCreado.CodigoCliente,
            NombreCompleto = clienteCreado.NombreCompleto
        };
    }

    public Task<ClienteDetalleResponse?> ObtenerClienteAsync(int clienteId)
    {
        return _clienteQueryRepository.GetDetalleByIdAsync(clienteId);
    }

    public Task<ClienteDetalleResponse?> ObtenerClientePorCodigoAsync(string codigoCliente)
    {
        return _clienteQueryRepository.GetDetalleByCodigoAsync(codigoCliente);
    }

    public async Task<IEnumerable<ClienteResumenResponse>> ObtenerClientesActivosAsync()
    {
        return await _clienteQueryRepository.GetActivosAsync();
    }

    public async Task<IEnumerable<ClienteResumenResponse>> ObtenerClientesPorEjecutivoAsync(int ejecutivoId)
    {
        return await _clienteQueryRepository.GetByEjecutivoIdAsync(ejecutivoId);
    }

    public async Task<ClienteDetalleResponse> ActualizarClienteAsync(int clienteId, UpdateClienteRequest request)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
            throw new InvalidOperationException($"Cliente con ID {clienteId} no encontrado.");

        if (!string.IsNullOrWhiteSpace(request.NombreCompleto))
            cliente.NombreCompleto = request.NombreCompleto.Trim();

        if (!string.IsNullOrEmpty(request.Correo))
            cliente.Correo = request.Correo.Trim();

        if (!string.IsNullOrEmpty(request.Telefono))
            cliente.Telefono = request.Telefono.Trim();

        if (!string.IsNullOrEmpty(request.Notas))
            cliente.Notas = request.Notas.Trim();

        await _clienteRepository.UpdateAsync(cliente);

        return await _clienteQueryRepository.GetDetalleByIdAsync(clienteId)
            ?? throw new InvalidOperationException("No se pudo recuperar el cliente actualizado.");
    }

    public Task<bool> DesactivarClienteAsync(int clienteId)
    {
        return _clienteRepository.DeactivateAsync(clienteId);
    }

    public async Task<bool> TieneContratoActivoAsync(int clienteId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
            return false;

        return await _contratoRepository.ClienteTieneContratosActivosAsync(clienteId);
    }

    public async Task<IEnumerable<string>> ObtenerAdvertenciasClienteAsync(int clienteId)
    {
        var advertencias = new List<string>();
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);

        if (cliente == null)
            return advertencias;

        var tieneContrato = await _contratoRepository.ClienteTieneContratosActivosAsync(clienteId);
        if (!tieneContrato)
        {
            advertencias.Add("El cliente no tiene contratos activos. Los beneficiarios y pagos requerirán un contrato.");
        }

        var sumaBeneficiarios = await _beneficiarioRepository.GetSumaPorcentajeActivosAsync(clienteId);
        var cantidadBeneficiarios = await _beneficiarioRepository.GetCantidadActivosAsync(clienteId);

        if (cantidadBeneficiarios > 0 && sumaBeneficiarios < 100m)
        {
            advertencias.Add($"Los beneficiarios suman {sumaBeneficiarios}% en lugar de 100%. Asignación incompleta.");
        }

        return advertencias;
    }
}
