using Tradecorp.Application.DTOs;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Services;

public class ContratoService : IContratoService
{
    private const decimal PorcentajeMinimo = 6m;
    private const decimal PorcentajeMaximo = 8.50m;
    private const decimal CapitalMinimo = 0.01m;

    private readonly IContratoRepository _contratoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IClienteBeneficiarioRepository _clienteBeneficiarioRepository;

    public ContratoService(
        IContratoRepository contratoRepository,
        IClienteRepository clienteRepository,
        IClienteBeneficiarioRepository clienteBeneficiarioRepository)
    {
        _contratoRepository = contratoRepository ?? throw new ArgumentNullException(nameof(contratoRepository));
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        _clienteBeneficiarioRepository = clienteBeneficiarioRepository ?? throw new ArgumentNullException(nameof(clienteBeneficiarioRepository));
    }

    public async Task<ContratoResponse> CrearContratoAsync(CreateContratoRequest request, int usuarioId)
    {
        await ValidarCreateUpdateAsync(request.CapitalInicial, request.PorcentajeMensual);

        if (!await _contratoRepository.ClienteExisteAsync(request.ClienteId))
            throw new InvalidOperationException($"Cliente {request.ClienteId} no existe.");

        if (request.Beneficiarios is { Count: > 0 })
            ValidarBeneficiarios(request.Beneficiarios);

        var numeroContrato = await _contratoRepository.GenerarNumeroContratoAsync(request.ClienteId);
        var contratoId = await _contratoRepository.CrearContratoAsync(request, usuarioId, numeroContrato);
        var contrato = await _contratoRepository.ObtenerContratoDetalleAsync(contratoId);

        return contrato ?? throw new InvalidOperationException("No se pudo recuperar el contrato recién creado.");
    }

    public async Task<ContratoResponse?> ObtenerContratoAsync(int contratoId)
    {
        return await _contratoRepository.ObtenerContratoDetalleAsync(contratoId);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerContratosAsync(ContratoFiltroRequest? filtro)
    {
        return await _contratoRepository.ObtenerContratosAsync(filtro);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerContratosClienteAsync(int clienteId)
    {
        return await _contratoRepository.ObtenerContratosClienteAsync(clienteId);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerContratosActivosClienteAsync(int clienteId)
    {
        return await _contratoRepository.ObtenerContratosActivosClienteAsync(clienteId);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerTodosContratosActivosAsync()
    {
        return await _contratoRepository.ObtenerTodosContratosActivosAsync();
    }

    public async Task<ContratoResponse> ActualizarContratoAsync(int contratoId, UpdateContratoRequest request, int usuarioId)
    {
        var detalleActual = await _contratoRepository.ObtenerContratoDetalleAsync(contratoId);
        if (detalleActual is null)
            throw new InvalidOperationException($"Contrato con ID {contratoId} no encontrado.");

        if (!detalleActual.Activo)
            throw new InvalidOperationException("No se puede modificar un contrato finalizado.");

        if (request.PorcentajeMensual.HasValue)
        {
            ValidarPorcentaje(request.PorcentajeMensual.Value);
        }

        var updated = await _contratoRepository.ActualizarContratoAsync(contratoId, request, usuarioId);
        if (!updated)
            throw new InvalidOperationException("No se pudo actualizar el contrato.");

        return await _contratoRepository.ObtenerContratoDetalleAsync(contratoId)
            ?? throw new InvalidOperationException("No se pudo recuperar el contrato actualizado.");
    }

    public async Task<bool> FinalizarContratoAsync(int contratoId, int usuarioId, string? observacion = null)
    {
        if (!await _contratoRepository.ContratoActivoAsync(contratoId))
            throw new InvalidOperationException("Solo se pueden finalizar contratos activos.");

        return await _contratoRepository.FinalizarContratoAsync(contratoId, usuarioId, observacion);
    }

    public async Task<IEnumerable<BeneficiarioContratoResponse>> AsignarBeneficiariosAsync(int contratoId, List<AsignarBeneficiarioRequest> beneficiarios, int usuarioId)
    {
        if (!await _contratoRepository.ContratoActivoAsync(contratoId))
            throw new InvalidOperationException("Solo se pueden editar beneficiarios de contratos activos.");

        if (beneficiarios.Count == 0)
            throw new ArgumentException("Debe enviar beneficiarios.");

        ValidarBeneficiarios(beneficiarios);
        return await _contratoRepository.AsignarBeneficiariosAsync(contratoId, beneficiarios, usuarioId);
    }

    public async Task<IEnumerable<BeneficiarioContratoResponse>> ObtenerBeneficiariosContratoAsync(int contratoId)
    {
        return await _contratoRepository.ObtenerBeneficiariosContratoAsync(contratoId);
    }

    public async Task<ContratoResponse> RegistrarReinversionAsync(int contratoId, ReinversionContratoRequest request, int usuarioId)
    {
        if (!await _contratoRepository.ContratoActivoAsync(contratoId))
            throw new InvalidOperationException("La reinversión solo aplica a contratos activos.");

        if (request.Tipo.Equals("Parcial", StringComparison.OrdinalIgnoreCase) && (!request.MontoReinvertir.HasValue || request.MontoReinvertir.Value <= 0m))
            throw new ArgumentException("Para reinversión parcial debes enviar un monto mayor a 0.");

        var ok = await _contratoRepository.RegistrarReinversionAsync(contratoId, request, usuarioId);
        if (!ok)
            throw new InvalidOperationException("No se pudo registrar la reinversión.");

        return await _contratoRepository.ObtenerContratoDetalleAsync(contratoId)
            ?? throw new InvalidOperationException("No se pudo recuperar el contrato actualizado.");
    }

    public async Task<ContratoResponse> RegistrarInyeccionCapitalAsync(int contratoId, InyeccionCapitalRequest request, int usuarioId)
    {
        if (!await _contratoRepository.ContratoActivoAsync(contratoId))
            throw new InvalidOperationException("La inyección de capital solo aplica a contratos activos.");

        if (request.Monto <= 0)
            throw new ArgumentException("El monto de inyección debe ser mayor a 0.");

        var ok = await _contratoRepository.RegistrarInyeccionCapitalAsync(contratoId, request, usuarioId);
        if (!ok)
            throw new InvalidOperationException("No se pudo registrar la inyección de capital.");

        return await _contratoRepository.ObtenerContratoDetalleAsync(contratoId)
            ?? throw new InvalidOperationException("No se pudo recuperar el contrato actualizado.");
    }

    public async Task<ContratoResponse> UnificarContratosAsync(int contratoDestinoId, UnificarContratosRequest request, int usuarioId)
    {
        if (request.ContratosOrigenIds.Count == 0)
            throw new ArgumentException("Debes seleccionar contratos origen.");

        if (!await _contratoRepository.ContratoActivoAsync(contratoDestinoId))
            throw new InvalidOperationException("El contrato destino debe estar activo.");

        var clienteDestino = await _contratoRepository.ObtenerClienteIdContratoAsync(contratoDestinoId);
        foreach (var origenId in request.ContratosOrigenIds)
        {
            if (!await _contratoRepository.ContratoActivoAsync(origenId))
                throw new InvalidOperationException("Todos los contratos a unificar deben estar activos.");

            var clienteOrigen = await _contratoRepository.ObtenerClienteIdContratoAsync(origenId);
            if (clienteOrigen != clienteDestino)
                throw new InvalidOperationException("Solo se pueden unificar contratos del mismo cliente.");
        }

        var ok = await _contratoRepository.UnificarContratosAsync(contratoDestinoId, request, usuarioId);
        if (!ok)
            throw new InvalidOperationException("No se pudo realizar la unificación.");

        return await _contratoRepository.ObtenerContratoDetalleAsync(contratoDestinoId)
            ?? throw new InvalidOperationException("No se pudo recuperar el contrato unificado.");
    }

    public async Task<bool> DesunificarContratoAsync(int contratoId, DesunificarContratoRequest request, int usuarioId)
    {
        if (!await _contratoRepository.ContratoExisteAsync(contratoId))
            throw new InvalidOperationException("Contrato no existe.");

        return await _contratoRepository.DesunificarContratoAsync(contratoId, request, usuarioId);
    }

    public async Task<IEnumerable<HistorialFinancieroItemResponse>> ObtenerHistorialFinancieroAsync(int contratoId)
    {
        return await _contratoRepository.ObtenerHistorialFinancieroAsync(contratoId);
    }

    public async Task<IEnumerable<ContratoEventoResponse>> ObtenerEventosAsync(int contratoId)
    {
        return await _contratoRepository.ObtenerEventosAsync(contratoId);
    }

    public async Task<IEnumerable<AuditoriaContratoResponse>> ObtenerAuditoriaAsync(int contratoId)
    {
        return await _contratoRepository.ObtenerAuditoriaAsync(contratoId);
    }

    private static async Task ValidarCreateUpdateAsync(decimal capital, decimal porcentaje)
    {
        await Task.CompletedTask;

        if (capital < CapitalMinimo)
            throw new ArgumentException("El capital debe ser mayor a 0.");

        ValidarPorcentaje(porcentaje);
    }

    private static void ValidarPorcentaje(decimal porcentaje)
    {
        if (porcentaje < PorcentajeMinimo || porcentaje > PorcentajeMaximo)
            throw new ArgumentException($"El porcentaje mensual debe estar entre {PorcentajeMinimo} y {PorcentajeMaximo}.");
    }

    private static void ValidarBeneficiarios(List<AsignarBeneficiarioRequest> beneficiarios)
    {
        if (beneficiarios.Any(x => x.Porcentaje < 0 || x.Porcentaje > 100))
            throw new ArgumentException("Cada beneficiario debe tener porcentaje entre 0 y 100.");

        var suma = beneficiarios.Sum(x => x.Porcentaje);
        if (suma != 100m)
            throw new ArgumentException("Los beneficiarios deben sumar 100%.");

        if (beneficiarios.Any(x => !x.ClienteBeneficiarioId.HasValue && x.Beneficiario is null))
            throw new ArgumentException("Cada beneficiario debe referenciar uno existente o incluir datos rápidos.");

        if (beneficiarios.Any(x => x.ClienteBeneficiarioId.HasValue && x.ClienteBeneficiarioId <= 0))
            throw new ArgumentException("ClienteBeneficiarioId inválido.");
    }
}
