using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using Tradecorp.Domain.Models.ValueObjects;
using Tradecorp.Domain.Exceptions;
using Tradecorp.Domain.Models.Rules;
using Tradecorp.Application.DTOs;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Services;

/// <summary>
/// Servicio de aplicación para contratos.
/// Orquesta casos de uso, valida reglas de dominio y coordina persistencia.
/// </summary>
public class ContratoApplicationService : IContratoApplicationService
{
    private readonly IContratoRepository _contratoRepository;
    private readonly IContratoRelacionRepository _relacionRepository;
    private readonly IContratoBeneficiarioRepository _beneficiarioRepository;
    private readonly IAuditoriaContratoRepository _auditoriaRepository;
    private readonly IAuditoriaContratoService _auditoriaService;
    private readonly IClienteRepository _clienteRepository;

    public ContratoApplicationService(
        IContratoRepository contratoRepository,
        IContratoRelacionRepository relacionRepository,
        IContratoBeneficiarioRepository beneficiarioRepository,
        IAuditoriaContratoRepository auditoriaRepository,
        IAuditoriaContratoService auditoriaService,
        IClienteRepository clienteRepository)
    {
        _contratoRepository = contratoRepository ?? throw new ArgumentNullException(nameof(contratoRepository));
        _relacionRepository = relacionRepository ?? throw new ArgumentNullException(nameof(relacionRepository));
        _beneficiarioRepository = beneficiarioRepository ?? throw new ArgumentNullException(nameof(beneficiarioRepository));
        _auditoriaRepository = auditoriaRepository ?? throw new ArgumentNullException(nameof(auditoriaRepository));
        _auditoriaService = auditoriaService ?? throw new ArgumentNullException(nameof(auditoriaService));
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
    }

    /// <summary>
    /// Crear un nuevo contrato para un cliente.
    /// </summary>
    public async Task<OperacionContratoResponse> CrearContratoAsync(CreateContratoRequest request, int usuarioEjecutivoId)
    {
        try
        {
            // 1. Validar que el cliente existe
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
            if (cliente == null)
                return ErrorResponse($"Cliente {request.ClienteId} no encontrado.");

            // 2. Parsear enum
            if (!Enum.TryParse<ModalidadRendimiento>(request.ModalidadRendimiento, out var modalidad))
                return ErrorResponse($"Modalidad '{request.ModalidadRendimiento}' no válida.");

            // 3. Crear contrato usando Factory Method (encapsula validaciones de dominio)
            var contrato = Contrato.Crear(
                request.ClienteId,
                request.NumeroContrato ?? string.Empty,
                request.FechaInicio ?? DateOnly.FromDateTime(DateTime.UtcNow),
                request.CapitalInicial,
                request.PorcentajeMensual,
                request.ComisionRetiro ?? 0m,
                modalidad,
                request.PermiteUnificacion);

            // 4. Persistir
            var contratoCreado = await _contratoRepository.CreateAsync(contrato);

            // 5. Registrar auditoría
            await _auditoriaService.RegistrarAsync(
                contratoCreado.Id,
                "Creacion",
                usuarioEjecutivoId,
                null,
                new { contratoCreado.NumeroContrato, contratoCreado.CapitalInicial },
                "Creación de contrato nuevo");

            // 6. Mapear y retornar
            var response = MapearContratoAResponse(contratoCreado);
            return new OperacionContratoResponse
            {
                Exitosa = true,
                Mensaje = "Contrato creado exitosamente.",
                ContratoCreado = contratoCreado.Id,
                ContratoResultante = response
            };
        }
        catch (ReglaNegocioException ex)
        {
            return ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Error al crear contrato: {ex.Message}");
        }
    }

    /// <summary>
    /// Crear un contrato adicional para un cliente.
    /// </summary>
    public async Task<OperacionContratoResponse> CrearContratoAdicionalAsync(CreateContratoAdicionalRequest request, int usuarioEjecutivoId)
    {
        try
        {
            // 1. Validar cliente
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
            if (cliente == null)
                return ErrorResponse($"Cliente no encontrado.");

            // 2. Crear contrato con lógica de dominio
            var contrato = Contrato.Crear(
                request.ClienteId,
                request.NumeroContrato,
                request.FechaInicio,
                request.CapitalInicial,
                request.PorcentajeMensual,
                request.ComisionRetiro,
                ModalidadRendimiento.Normal,
                true);

            // 3. Persistir
            var contratoCreado = await _contratoRepository.CreateAsync(contrato);

            // 4. Auditoría
            await _auditoriaService.RegistrarAsync(
                contratoCreado.Id,
                "Creacion",
                usuarioEjecutivoId,
                null,
                new { contratoCreado.NumeroContrato, contratoCreado.CapitalInicial },
                "Contrato adicional creado");

            var response = MapearContratoAResponse(contratoCreado);
            return new OperacionContratoResponse
            {
                Exitosa = true,
                Mensaje = "Contrato adicional creado.",
                ContratoCreado = contratoCreado.Id,
                ContratoResultante = response
            };
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Unificar múltiples contratos en uno nuevo.
    /// </summary>
    public async Task<OperacionContratoResponse> UnificarContratosAsync(UnificarContratosRequest request, int usuarioId)
    {
        try
        {
            // 1. Validar cantidad
            UnificacionRules.ValidarCantidadContratosAUnificar(request.ContratosOrigenIds.Count);

            // 2. Obtener contratos
            var contratos = new List<Contrato>();
            foreach (var id in request.ContratosOrigenIds)
            {
                var c = await _contratoRepository.GetByIdAsync(id);
                if (c == null)
                    return ErrorResponse($"Contrato {id} no encontrado.");
                contratos.Add(c);
            }

            // 3. Validar estados
            UnificacionRules.ValidarTodosActivosParaUnificacion(contratos.Select(c => c.Estado).ToList());
            foreach (var c in contratos)
            {
                UnificacionRules.ValidarPermiteUnificacion(c.PermiteUnificacion);
            }

            // 4. Calcular capital total
            var capitalTotal = contratos.Aggregate(0m, (sum, c) => sum + c.CapitalActual);
            var contratoReferencia = contratos[0];

            // 5. Crear contrato unificado
            var contratoUnificado = Contrato.Crear(
                request.ClienteId ?? contratoReferencia.ClienteId,
                request.NumeroContratoUnificado ?? string.Empty,
                request.FechaInicio ?? DateOnly.FromDateTime(DateTime.UtcNow),
                capitalTotal,
                request.PorcentajeMensual ?? contratoReferencia.PorcentajeMensual,
                0m,
                ModalidadRendimiento.Normal,
                true);

            var contratoCreado = await _contratoRepository.CreateAsync(contratoUnificado);

            // 6. Marcar como unificados los contratos origen
            foreach (var c in contratos)
            {
                c.MarcarComoUnificado(request.AprobacionGerencial);
                await _contratoRepository.UpdateAsync(c);

                // Registrar relación
                var relacion = new ContratoRelacion
                {
                    ContratoOrigenId = c.Id,
                    ContratoDestinoId = contratoCreado.Id,
                    TipoRelacion = TipoRelacionContrato.Unificacion,
                    MontoTransferido = c.CapitalActual,
                    UsuarioId = usuarioId,
                    Observacion = request.NumeroContratoUnificado
                };
                await _relacionRepository.CreateAsync(relacion);
            }

            // 7. Auditoría
            var observacionUnif = request.NumeroContratoUnificado;
            if (request.AprobacionGerencial && request.UsuarioAprobadorId.HasValue)
                observacionUnif += $" | AprobacionGerencial por Usuario:{request.UsuarioAprobadorId}";

            await _auditoriaService.RegistrarAsync(
                contratoCreado.Id,
                "Unificacion",
                usuarioId,
                null,
                new { contratoCreado.NumeroContrato, CapitalTotal = capitalTotal, ContratosUnificados = request.ContratosOrigenIds.Count },
                observacionUnif);

            var response = MapearContratoAResponse(contratoCreado);
            return new OperacionContratoResponse
            {
                Exitosa = true,
                Mensaje = $"Unificación exitosa. {request.ContratosOrigenIds.Count} contratos unificados.",
                ContratoCreado = contratoCreado.Id,
                ContratoResultante = response
            };
        }
        catch (ReglaNegocioException ex)
        {
            return ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Inyectar capital a un contrato.
    /// </summary>
    public async Task<OperacionContratoResponse> InyectarCapitalAsync(InyectarCapitalRequest request, int usuarioId)
    {
        try
        {
            // 1. Obtener contrato
            var contrato = await _contratoRepository.GetByIdAsync(request.ContratoId);
            if (contrato == null)
                return ErrorResponse("Contrato no encontrado.");
            // 2. Validar con reglas de dominio
            var capitalInyectar = new Capital(request.CapitalAInyectar);
            var capitalAnterior = contrato.CapitalActual;
            // Si viene aprobación gerencial verificar que exista usuario aprobador
            if (request.AprobacionGerencial && !request.UsuarioAprobadorId.HasValue)
                return ErrorResponse("Usuario aprobador requerido para aprobación gerencial.");

            contrato.InyectarCapital(capitalInyectar, request.AprobacionGerencial);

            // 3. Persistir cambios
            await _contratoRepository.UpdateAsync(contrato);

            // 4. Auditoría
            var observacion = $"Capital inyectado: ${request.CapitalAInyectar}";
            if (request.AprobacionGerencial)
                observacion += $" | AprobacionGerencial por Usuario:{request.UsuarioAprobadorId} Motivo:{request.MotivoAprobacion}";

            await _auditoriaService.RegistrarAsync(
                request.ContratoId,
                "InyeccionCapital",
                usuarioId,
                new { CapitalAnterior = capitalAnterior },
                new { CapitalNuevo = contrato.CapitalActual, AprobacionGerencial = request.AprobacionGerencial },
                observacion);

            var response = MapearContratoAResponse(contrato);
            return new OperacionContratoResponse
            {
                Exitosa = true,
                Mensaje = "Capital inyectado exitosamente.",
                ContratoResultante = response
            };
        }
        catch (ReglaNegocioException ex)
        {
            return ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Reinvertir ganancias en un contrato.
    /// </summary>
    public async Task<OperacionContratoResponse> ReinvertirGananciasAsync(ReinvertirGananciasRequest request, int usuarioId)
    {
        try
        {
            // 1. Obtener contrato
            var contrato = await _contratoRepository.GetByIdAsync(request.ContratoId);
            if (contrato == null)
                return ErrorResponse("Contrato no encontrado.");
            // 2. Validar y reinvertir
            var ganancias = new Capital(request.Ganancias);
            var porcentajeAnterior = contrato.PorcentajeMensual;
            var capitalAnterior = contrato.CapitalActual;
            if (request.AprobacionGerencial && !request.UsuarioAprobadorId.HasValue)
                return ErrorResponse("Usuario aprobador requerido para aprobación gerencial.");

            contrato.Reinvertir(ganancias, request.NuevoPorcentajeMensual, request.AprobacionGerencial);

            // 3. Persistir
            await _contratoRepository.UpdateAsync(contrato);

            // 4. Auditoría
            var observacionReinv = $"Ganancias reinvertidas: ${request.Ganancias}";
            if (request.AprobacionGerencial)
                observacionReinv += $" | AprobacionGerencial por Usuario:{request.UsuarioAprobadorId} Motivo:{request.MotivoAprobacion}";

            await _auditoriaService.RegistrarAsync(
                request.ContratoId,
                "Reinversion",
                usuarioId,
                new { PorcentajeAnterior = porcentajeAnterior, CapitalAnterior = capitalAnterior },
                new { CapitalNuevo = contrato.CapitalActual, PorcentajeNuevo = contrato.PorcentajeMensual, AprobacionGerencial = request.AprobacionGerencial },
                observacionReinv);

            var response = MapearContratoAResponse(contrato);
            return new OperacionContratoResponse
            {
                Exitosa = true,
                Mensaje = "Reinversión registrada.",
                ContratoResultante = response
            };
        }
        catch (ReglaNegocioException ex)
        {
            return ErrorResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Cambiar porcentaje mensual.
    /// </summary>
    public async Task<OperacionContratoResponse> CambiarPorcentajeAsync(CambiarPorcentajeRequest request, int usuarioId)
    {
        try
        {
            var contrato = await _contratoRepository.GetByIdAsync(request.ContratoId);
            if (contrato == null)
                return ErrorResponse("Contrato no encontrado.");

            var porcentajeAnterior = contrato.PorcentajeMensual;
            if (request.AprobacionGerencial && !request.UsuarioAprobadorId.HasValue)
                return ErrorResponse("Usuario aprobador requerido para aprobación gerencial.");

            contrato.CambiarPorcentajeMensual(request.NuevoPorcentaje, request.AprobacionGerencial);

            await _contratoRepository.UpdateAsync(contrato);

            var observacionCambio = $"Porcentaje cambiadode {porcentajeAnterior}% a {request.NuevoPorcentaje}%";
            if (request.AprobacionGerencial)
                observacionCambio += $" | AprobacionGerencial por Usuario:{request.UsuarioAprobadorId} Motivo:{request.MotivoAprobacion}";

            await _auditoriaService.RegistrarAsync(
                request.ContratoId,
                "CambioPorcentaje",
                usuarioId,
                new { PorcentajeAnterior = porcentajeAnterior },
                new { PorcentajeNuevo = request.NuevoPorcentaje, AprobacionGerencial = request.AprobacionGerencial },
                observacionCambio);

            var response = MapearContratoAResponse(contrato);
            return new OperacionContratoResponse
            {
                Exitosa = true,
                Mensaje = "Porcentaje actualizado.",
                ContratoResultante = response
            };
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }

    /// <summary>
    /// Obtener un contrato.
    /// </summary>
    public async Task<ContratoResponse?> ObtenerContratoAsync(int contratoId)
    {
        var contrato = await _contratoRepository.GetByIdAsync(contratoId);
        return contrato != null ? MapearContratoAResponse(contrato) : null;
    }

    /// <summary>
    /// Obtener contratos activos.
    /// </summary>
    public async Task<IEnumerable<ContratoResponse>> ObtenerContratosActivosAsync(int clienteId)
    {
        var contratos = await _contratoRepository.GetActivosPorClienteAsync(clienteId);
        return contratos.Select(MapearContratoAResponse).ToList();
    }

    /// <summary>
    /// Obtener auditoría.
    /// </summary>
    public async Task<IEnumerable<AuditoriaContratoResponse>> ObtenerAuditoriaAsync(int contratoId)
    {
        var auditorias = await _auditoriaRepository.GetPorContratoAsync(contratoId);
        return auditorias.Select(a => new AuditoriaContratoResponse
        {
            Id = a.Id,
            ContratoId = a.ContratoId,
            TipoMovimiento = a.TipoMovimiento.ToString(),
            Observacion = a.Observacion,
            UsuarioId = a.UsuarioId,
            FechaMovimiento = a.FechaMovimiento
        }).ToList();
    }

    #region Helpers

    private ContratoResponse MapearContratoAResponse(Contrato contrato)
    {
        return new ContratoResponse
        {
            Id = contrato.Id,
            ClienteId = contrato.ClienteId,
            NumeroContrato = contrato.NumeroContrato,
            FechaInicio = contrato.FechaInicio,
            CapitalInicial = contrato.CapitalInicial,
            CapitalActual = contrato.CapitalActual,
            PorcentajeMensual = contrato.PorcentajeMensual,
            ComisionRetiro = contrato.ComisionRetiro,
            ModalidadRendimiento = contrato.ModalidadRendimiento.ToString(),
            Estado = contrato.Estado.ToString(),
            PermiteUnificacion = contrato.PermiteUnificacion,
            FechaCreacion = contrato.FechaCreacion,
            FechaCierre = contrato.FechaCierre
        };
    }

    private OperacionContratoResponse ErrorResponse(string mensaje)
    {
        return new OperacionContratoResponse
        {
            Exitosa = false,
            Mensaje = mensaje,
            Errores = new List<string> { mensaje }
        };
    }

    #endregion
}

/// <summary>
/// Servicio de Auditoría.
/// Desacoplado del servicio principal para manejar auditoría como cross-cutting concern.
/// </summary>
public class AuditoriaContratoService : IAuditoriaContratoService
{
    private readonly IAuditoriaContratoRepository _repository;

    public AuditoriaContratoService(IAuditoriaContratoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<long> RegistrarAsync(
        int contratoId,
        string tipoMovimiento,
        int usuarioId,
        object? valorAnterior = null,
        object? valorNuevo = null,
        string? observacion = null)
    {
        if (!Enum.TryParse<TipoOperacion>(tipoMovimiento, out var tipo))
            throw new InvalidOperationException($"Tipo de movimiento '{tipoMovimiento}' no válido.");

        var auditoria = AuditoriaContrato.Crear(contratoId, tipo, usuarioId, valorAnterior, valorNuevo, observacion);
        var creada = await _repository.CreateAsync(auditoria);
        return creada.Id;
    }
}
