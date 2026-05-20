using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;
using Tradecorp.Domain.Models.Rules;
using Tradecorp.Domain.Models.ValueObjects;

namespace Tradecorp.Infrastructure.Services;

public sealed class ContratoProjectionService : IContratoProjectionService
{
    private readonly IContratoRepository _contratoRepository;
    private readonly IPagoService _pagoService;

    public ContratoProjectionService(
        IContratoRepository contratoRepository,
        IPagoService pagoService)
    {
        _contratoRepository = contratoRepository ?? throw new ArgumentNullException(nameof(contratoRepository));
        _pagoService = pagoService ?? throw new ArgumentNullException(nameof(pagoService));
    }

    public async Task<ContratoProjectionResponse?> ObtenerProyeccion24MesesAsync(int contratoId)
    {
        var contrato = await _contratoRepository.GetByIdAsync(contratoId);
        if (contrato is null)
        {
            return null;
        }

        return await ConstruirRespuestaAsync(
            contrato.Id,
            contrato.NumeroContrato,
            contrato.Cliente?.NombreCompleto,
            contrato.FechaInicio,
            contrato.CapitalActual,
            contrato.PorcentajeMensual,
            contrato.ModalidadRendimiento.ToString(),
            contrato.Estado.ToString(),
            esSimulacion: false);
    }

    public Task<ContratoProjectionResponse> SimularAsync(SimularContratoRequest request)
    {
        if (request.CapitalInicial <= 0)
            throw new ArgumentException("El capital inicial debe ser mayor a cero.");

        if (request.PorcentajeMensual < 6m || request.PorcentajeMensual > 8.5m)
            throw new ArgumentException("El porcentaje mensual debe estar entre 6% y 8.5%.");

        return ConstruirRespuestaAsync(
            contratoId: null,
            numeroContrato: null,
            clienteNombre: null,
            fechaInicio: request.FechaInicio,
            capitalInicial: request.CapitalInicial,
            porcentajeMensual: request.PorcentajeMensual,
            modalidadRendimiento: request.ModalidadRendimiento,
            estadoContrato: "Simulado",
            esSimulacion: true);
    }

    private async Task<ContratoProjectionResponse> ConstruirRespuestaAsync(
        int? contratoId,
        string? numeroContrato,
        string? clienteNombre,
        DateOnly fechaInicio,
        decimal capitalInicial,
        decimal porcentajeMensual,
        string modalidadRendimiento,
        string estadoContrato,
        bool esSimulacion)
    {
        var periodo = new PeriodoPago();
        var ventanasPago = CalendarioContratoRules.ObtenerVentanasDesde(fechaInicio, periodo).ToList();
        var proyecciones = new List<ProyeccionPagoResponse>();
        var capitalBase = capitalInicial;
        var fechaBase = fechaInicio;
        var now = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var diasTotales = 0;
        var gananciaBrutaTotal = 0m;
        var comisionTotal = 0m;
        var gananciaNetaTotal = 0m;
        var proximoPagoAsignado = false;

        for (var indice = 0; indice < periodo.DuracionMeses / periodo.PeriodoMeses; indice++)
        {
            var fechaCorte = fechaInicio.AddMonths(periodo.PeriodoMeses * (indice + 1));
            var ventanaPago = ventanasPago.ElementAtOrDefault(indice);

            var calculo = await _pagoService.CalcularGananciaAsync(new CalculoGananciaRequest
            {
                ContratoId = contratoId ?? 0,
                FechaInicio = fechaBase,
                FechaFin = fechaCorte,
                Capital = capitalBase,
                PorcentajeMensual = porcentajeMensual,
                Modalidad = modalidadRendimiento
            });

            var gananciaBruta = calculo.GananciaBruta;
            var comision = calculo.ComisionEmpresa;
            var gananciaNeta = calculo.GananciaNeta;

            var ventanaInicioPago = ventanaPago?.FechaInicio ?? fechaCorte.AddDays(1);
            var ventanaFinPago = ventanaPago?.FechaFin ?? fechaCorte.AddDays(periodo.VentanaDiasHabiles);

            var estado = fechaCorte < now ? "Cerrado" : fechaBase <= now && now <= fechaCorte ? "Vigente" : "Programado";

            proyecciones.Add(new ProyeccionPagoResponse
            {
                Periodo = indice + 1,
                FechaInicio = fechaBase,
                FechaCorte = fechaCorte,
                FechaPagoInicio = ventanaInicioPago,
                FechaPagoFin = ventanaFinPago,
                DiasFinancieros = calculo.DiasTranscurridos,
                CapitalBase = capitalBase,
                GananciaBruta = gananciaBruta,
                Comision = comision,
                GananciaNeta = gananciaNeta,
                CapitalProyectado = capitalBase + gananciaNeta,
                Estado = estado,
                EsVentanaActual = ventanaPago?.Incluye(DateTime.UtcNow) ?? false,
                EsSiguientePago = !proximoPagoAsignado && fechaCorte >= now
            });

            if (!proximoPagoAsignado && fechaCorte >= now)
            {
                proximoPagoAsignado = true;
            }

            diasTotales += calculo.DiasTranscurridos;
            gananciaBrutaTotal += gananciaBruta;
            comisionTotal += comision;
            gananciaNetaTotal += gananciaNeta;

            if (modalidadRendimiento.Equals("InteresCompuesto", StringComparison.OrdinalIgnoreCase))
            {
                capitalBase += gananciaNeta;
            }

            fechaBase = fechaCorte.AddDays(1);
        }

        var proximoPago = proyecciones.FirstOrDefault(x => x.FechaCorte >= now);

        return new ContratoProjectionResponse
        {
            ContratoId = contratoId,
            NumeroContrato = numeroContrato,
            ClienteNombre = clienteNombre,
            FechaInicio = fechaInicio,
            FechaFin = fechaInicio.AddMonths(periodo.DuracionMeses),
            CapitalInicial = capitalInicial,
            CapitalProyectado = proyecciones.LastOrDefault()?.CapitalProyectado ?? capitalInicial,
            GananciaBrutaTotal = gananciaBrutaTotal,
            ComisionTotal = comisionTotal,
            GananciaNetaTotal = gananciaNetaTotal,
            DiasFinancierosTotales = diasTotales,
            EstadoContrato = estadoContrato,
            EsSimulacion = esSimulacion,
            GeneradoEn = DateTime.UtcNow,
            ProximoPago = proximoPago,
            Pagos = proyecciones
        };
    }
}
