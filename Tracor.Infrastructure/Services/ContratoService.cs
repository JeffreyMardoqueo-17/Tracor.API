using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using Tradecorp.Application.DTOs;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Services;

/// <summary>
/// Servicio de negocios para Contratos
/// Aplica reglas de inversión definidas en el documento de negocio
/// </summary>
public class ContratoService : IContratoService
{
    private const decimal PORCENTAJE_MINIMO = 6m;
    private const decimal PORCENTAJE_MAXIMO = 8.50m;
    private const decimal CAPITAL_MINIMO = 0.01m;

    private readonly IContratoRepository _contratoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IClienteBeneficiarioRepository _beneficiarioRepository;

    public ContratoService(
        IContratoRepository contratoRepository,
        IClienteRepository clienteRepository,
        IClienteBeneficiarioRepository beneficiarioRepository)
    {
        _contratoRepository = contratoRepository ?? throw new ArgumentNullException(nameof(contratoRepository));
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        _beneficiarioRepository = beneficiarioRepository ?? throw new ArgumentNullException(nameof(beneficiarioRepository));
    }

    public async Task<ContratoResponse> CrearContratoAsync(CreateContratoRequest request)
    {
        // VALIDACIONES SEGÚN REGLAS DE NEGOCIO

        // 1. Validar que el cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
        if (cliente == null)
            throw new InvalidOperationException($"Cliente con ID {request.ClienteId} no encontrado.");

        // 2. Validar capital inicial
        if (request.CapitalInicial < CAPITAL_MINIMO)
            throw new ArgumentException($"El capital inicial debe ser mayor a {CAPITAL_MINIMO}.");

        // 3. Validar porcentaje mensual (6% - 8.50%)
        if (request.PorcentajeMensual < PORCENTAJE_MINIMO || request.PorcentajeMensual > PORCENTAJE_MAXIMO)
            throw new ArgumentException($"El porcentaje mensual debe estar entre {PORCENTAJE_MINIMO}% y {PORCENTAJE_MAXIMO}%.");

        // 4. Parsear modalidad rendimiento
        if (!Enum.TryParse<ModalidadRendimiento>(request.ModalidadRendimiento, out var modalidad))
            modalidad = ModalidadRendimiento.Normal;

        // CREAR CONTRATO
        var contrato = new Contrato
        {
            ClienteId = request.ClienteId,
            FechaInicio = DateOnly.FromDateTime(DateTime.Now),
            CapitalInicial = request.CapitalInicial,
            PorcentajeMensual = request.PorcentajeMensual,
            ComisionRetiro = request.ComisionRetiro ?? 0,
            ModalidadRendimiento = modalidad,
            PermiteUnificacion = request.PermiteUnificacion,
            Activo = true
        };

        // Guardar contrato (genera número automático)
        var contratoCreado = await _contratoRepository.CreateAsync(contrato);

        // ASIGNAR BENEFICIARIOS si se proporcionan
        if (request.Beneficiarios != null && request.Beneficiarios.Any())
        {
            await AsignarBeneficiariosAsync(contratoCreado.Id, request.Beneficiarios);
        }

        return await MapearContratoAResponse(contratoCreado, incluirBeneficiarios: true);
    }

    public async Task<ContratoResponse?> ObtenerContratoAsync(int contratoId)
    {
        var contrato = await _contratoRepository.GetByIdAsync(contratoId);
        if (contrato == null)
            return null;

        return await MapearContratoAResponse(contrato, incluirBeneficiarios: true);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerContratosClienteAsync(int clienteId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
            return new List<ContratoListaResponse>();

        var contratos = await _contratoRepository.GetByClienteIdAsync(clienteId);
        return contratos.Select(MapearContratoAListaResponse);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerContratosActivosClienteAsync(int clienteId)
    {
        var contratos = await _contratoRepository.GetActivosByClienteIdAsync(clienteId);
        return contratos.Select(MapearContratoAListaResponse);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerTodosContratosActivosAsync()
    {
        var contratos = await _contratoRepository.GetAllActivosAsync();
        return contratos.Select(MapearContratoAListaResponse);
    }

    public async Task<ContratoResponse> ActualizarContratoAsync(int contratoId, UpdateContratoRequest request)
    {
        var contrato = await _contratoRepository.GetByIdAsync(contratoId);
        if (contrato == null)
            throw new InvalidOperationException($"Contrato con ID {contratoId} no encontrado.");

        if (!contrato.Activo)
            throw new InvalidOperationException("No se puede modificar un contrato finalizado.");

        // Validar si cambia el porcentaje
        if (request.PorcentajeMensual.HasValue)
        {
            if (request.PorcentajeMensual.Value < PORCENTAJE_MINIMO || request.PorcentajeMensual.Value > PORCENTAJE_MAXIMO)
                throw new ArgumentException($"El porcentaje mensual debe estar entre {PORCENTAJE_MINIMO}% y {PORCENTAJE_MAXIMO}%.");

            contrato.PorcentajeMensual = request.PorcentajeMensual.Value;
        }

        if (request.ComisionRetiro.HasValue)
            contrato.ComisionRetiro = request.ComisionRetiro.Value;

        if (request.PermiteUnificacion.HasValue)
            contrato.PermiteUnificacion = request.PermiteUnificacion.Value;

        if (!string.IsNullOrEmpty(request.ModalidadRendimiento))
        {
            if (Enum.TryParse<ModalidadRendimiento>(request.ModalidadRendimiento, out var modalidad))
                contrato.ModalidadRendimiento = modalidad;
        }

        var contratoActualizado = await _contratoRepository.UpdateAsync(contrato);
        return await MapearContratoAResponse(contratoActualizado, incluirBeneficiarios: true);
    }

    public async Task<bool> FinalizarContratoAsync(int contratoId)
    {
        return await _contratoRepository.FinalizarAsync(contratoId);
    }

    public async Task<IEnumerable<BeneficiarioContratoResponse>> AsignarBeneficiariosAsync(int contratoId, List<AsignarBeneficiarioRequest> beneficiarios)
    {
        var contrato = await _contratoRepository.GetByIdAsync(contratoId);
        if (contrato == null)
            throw new InvalidOperationException($"Contrato con ID {contratoId} no encontrado.");

        var respuestas = new List<BeneficiarioContratoResponse>();

        foreach (var benef in beneficiarios)
        {
            ClienteBeneficiario beneficiario;

            // Si tiene ID de beneficiario existente, reutilizarlo
            if (benef.ClienteBeneficiarioId.HasValue && benef.ClienteBeneficiarioId.Value > 0)
            {
                beneficiario = await _beneficiarioRepository.GetByIdAsync(benef.ClienteBeneficiarioId.Value);
                if (beneficiario == null)
                    throw new InvalidOperationException($"Beneficiario con ID {benef.ClienteBeneficiarioId} no encontrado.");
            }
            else if (benef.Beneficiario != null)
            {
                // Crear nuevo beneficiario
                beneficiario = new ClienteBeneficiario
                {
                    ClienteId = contrato.ClienteId,
                    NombreCompleto = benef.Beneficiario.NombreCompleto.Trim(),
                    DUI = benef.Beneficiario.DUI,
                    Correo = benef.Beneficiario.Correo,
                    Telefono = benef.Beneficiario.Telefono,
                    Direccion = benef.Beneficiario.Direccion,
                    Porcentaje = benef.Porcentaje,
                    Notas = benef.Beneficiario.Notas,
                    Estado = ClienteBeneficiarioEstado.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };

                // Parsear tipo relación
                if (Enum.TryParse<ClienteBeneficiarioTipo>(benef.Beneficiario.TipoRelacion, out var tipoRel))
                    beneficiario.TipoRelacion = tipoRel;
                else
                    beneficiario.TipoRelacion = ClienteBeneficiarioTipo.Otro;

                beneficiario = await _beneficiarioRepository.CreateAsync(beneficiario);
            }
            else
            {
                throw new ArgumentException("Debe proporcionar ID de beneficiario o datos para crear uno nuevo.");
            }

            respuestas.Add(new BeneficiarioContratoResponse
            {
                Id = beneficiario.Id,
                ClienteBeneficiarioId = beneficiario.Id,
                NombreCompleto = beneficiario.NombreCompleto,
                DUI = beneficiario.DUI,
                TipoRelacion = beneficiario.TipoRelacion.ToString(),
                Estado = beneficiario.Estado.ToString(),
                Porcentaje = benef.Porcentaje
            });
        }

        return respuestas;
    }
    public async Task<IEnumerable<BeneficiarioContratoResponse>> ObtenerBeneficiariosContratoAsync(int contratoId)
    {
        var contrato = await _contratoRepository.GetByIdAsync(contratoId);
        if (contrato == null)
            return new List<BeneficiarioContratoResponse>();

        // Obtener beneficiarios del cliente (asociados al contrato mediante el cliente)
        var beneficiarios = await _beneficiarioRepository.GetByClienteIdAsync(contrato.ClienteId);

        return beneficiarios
            .Where(b => b.Estado == ClienteBeneficiarioEstado.Activo)
            .Select(b => new BeneficiarioContratoResponse
            {
                Id = b.Id,
                ClienteBeneficiarioId = b.Id,
                NombreCompleto = b.NombreCompleto,
                DUI = b.DUI,
                TipoRelacion = b.TipoRelacion.ToString(),
                Estado = b.Estado.ToString(),
                Porcentaje = b.Porcentaje
            });
    }

    public async Task<ContratoResponse> UnificarContratosAsync(int contratoDestinoId, List<int> contratosOrigenIds)
    {
        var contratoDestino = await _contratoRepository.GetByIdAsync(contratoDestinoId);
        if (contratoDestino == null)
            throw new InvalidOperationException($"Contrato destino con ID {contratoDestinoId} no encontrado.");

        if (!contratoDestino.PermiteUnificacion)
            throw new InvalidOperationException("Este contrato no permite unificación.");

        // Sumar capitales de contratos origen
        decimal capitalTotal = contratoDestino.CapitalInicial;

        foreach (var origenId in contratosOrigenIds)
        {
            var contratoOrigen = await _contratoRepository.GetByIdAsync(origenId);
            if (contratoOrigen == null)
                throw new InvalidOperationException($"Contrato origen con ID {origenId} no encontrado.");

            if (contratoOrigen.ClienteId != contratoDestino.ClienteId)
                throw new InvalidOperationException("Solo se pueden unificar contratos del mismo cliente.");

            capitalTotal += contratoOrigen.CapitalInicial;
            
            // Finalizar contrato origen
            await _contratoRepository.FinalizarAsync(origenId);
        }

        // Actualizar capital del destino
        contratoDestino.CapitalInicial = capitalTotal;
        await _contratoRepository.UpdateAsync(contratoDestino);

        return await MapearContratoAResponse(contratoDestino, incluirBeneficiarios: true);
    }

    public async Task<bool> DesunificarContratoAsync(int contratoId)
    {
        var contrato = await _contratoRepository.GetByIdAsync(contratoId);
        if (contrato == null)
            return false;

        // La desunificación simplemente marca el contrato como no unificable
        contrato.PermiteUnificacion = false;
        await _contratoRepository.UpdateAsync(contrato);
        return true;
    }

    // ===== MÉTODOS AUXILIARES =====

    private async Task<ContratoResponse> MapearContratoAResponse(Contrato contrato, bool incluirBeneficiarios = false)
    {
        var response = new ContratoResponse
        {
            Id = contrato.Id,
            ClienteId = contrato.ClienteId,
            NumeroContrato = contrato.NumeroContrato ?? string.Empty,
            FechaInicio = contrato.FechaInicio,
            CapitalInicial = contrato.CapitalInicial,
            PorcentajeMensual = contrato.PorcentajeMensual,
            ComisionRetiro = contrato.ComisionRetiro,
            ModalidadRendimiento = contrato.ModalidadRendimiento.ToString(),
            PermiteUnificacion = contrato.PermiteUnificacion,
            Activo = contrato.Activo,
            FechaCreacion = contrato.FechaCreacion,
            FechaCierre = contrato.FechaCierre
        };

        if (incluirBeneficiarios && contrato.Cliente != null)
        {
            var beneficiarios = await _beneficiarioRepository.GetByClienteIdAsync(contrato.ClienteId);
            response.Beneficiarios = beneficiarios
                .Where(b => b.Estado == ClienteBeneficiarioEstado.Activo)
                .Select(b => new BeneficiarioContratoResponse
                {
                    Id = b.Id,
                    ClienteBeneficiarioId = b.Id,
                    NombreCompleto = b.NombreCompleto,
                    DUI = b.DUI,
                    TipoRelacion = b.TipoRelacion.ToString(),
                    Estado = b.Estado.ToString(),
                    Porcentaje = b.Porcentaje
                })
                .ToList();
        }

        return response;
    }

    private ContratoListaResponse MapearContratoAListaResponse(Contrato contrato)
    {
        return new ContratoListaResponse
        {
            Id = contrato.Id,
            NumeroContrato = contrato.NumeroContrato ?? string.Empty,
            CapitalInicial = contrato.CapitalInicial,
            PorcentajeMensual = contrato.PorcentajeMensual,
            ModalidadRendimiento = contrato.ModalidadRendimiento.ToString(),
            Activo = contrato.Activo,
            FechaInicio = contrato.FechaInicio,
            FechaCierre = contrato.FechaCierre
        };
    }
}
