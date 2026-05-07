using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;
using Tradecorp.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace Tradecorp.Infrastructure.Services;

/// <summary>
/// Servicio para cálculos de pagos siguiendo las reglas de project.md
/// - Febrero siempre 28 días
/// - Otros meses siempre 30 días
/// - Comisión fija del 5% solo de la ganancia
/// </summary>
public class PagoService : IPagoService
{
    private readonly IContratoRepository _contratoRepository;
    private readonly IPagoRepository _pagoRepository;
    private readonly ILogger<PagoService> _logger;

    public PagoService(
        IContratoRepository contratoRepository,
        IPagoRepository pagoRepository,
        ILogger<PagoService> logger)
    {
        _contratoRepository = contratoRepository ?? throw new ArgumentNullException(nameof(contratoRepository));
        _pagoRepository = pagoRepository ?? throw new ArgumentNullException(nameof(pagoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CalculoGananciaResponse> CalcularGananciaAsync(CalculoGananciaRequest request)
    {
        var response = new CalculoGananciaResponse();
        var detallePorMes = new List<DetalleMesDto>();

        // Calcular días por mes siguiendo las reglas
        var fechaActual = request.FechaInicio;
        var diasTotales = 0;
        var diasFebrero = 0;
        var diasOtrosMeses = 0;
        var gananciaTotal = 0m;

        // Si el contrato inició antes del 1 de enero, usar cuatrimestres de 120 días
        var usarCuatrimestresExactos = fechaActual < new DateOnly(DateTime.Now.Year, 1, 1);

        while (fechaActual <= request.FechaFin)
        {
            var mes = fechaActual.Month;
            var año = fechaActual.Year;
            int diasEnMes;

            // Determinar días del mes según reglas
            if (mes == 2)
            {
                diasEnMes = 28; // Febrero siempre 28 días
                diasFebrero += diasEnMes;
            }
            else if (usarCuatrimestresExactos)
            {
                // Para contratos antes del 1 de enero, usar 120 días por cuatrimestre
                diasEnMes = Math.Min(120 - (diasTotales % 120), 30);
                diasOtrosMeses += diasEnMes;
            }
            else
            {
                diasEnMes = 30; // Otros meses siempre 30 días
                diasOtrosMeses += diasEnMes;
            }

            if (fechaActual.AddDays(diasEnMes - 1) > request.FechaFin)
            {
                diasEnMes = request.FechaFin.DayNumber - fechaActual.DayNumber + 1;
            }

            diasTotales += diasEnMes;

            // Calcular ganancia para este mes
            var gananciaMes = request.Capital * (request.PorcentajeMensual / 100m) * (diasEnMes / 30m);
            gananciaTotal += gananciaMes;

            detallePorMes.Add(new DetalleMesDto
            {
                Mes = new DateOnly(año, mes, 1).ToString("MMMM yyyy"),
                Dias = diasEnMes,
                Ganancia = Math.Round(gananciaMes, 2)
            });

            fechaActual = fechaActual.AddDays(diasEnMes);
        }

        response.DiasTranscurridos = diasTotales;
        response.DiasFebrero = diasFebrero;
        response.DiasOtrosMeses = diasOtrosMeses;
        response.GananciaBruta = Math.Round(gananciaTotal, 2);
        response.ComisionEmpresa = Math.Round(gananciaTotal * 0.05m, 2); // 5% fijo
        response.GananciaNeta = Math.Round(gananciaTotal - response.ComisionEmpresa, 2);
        response.DetallePorMes = detallePorMes;

        return response;
    }

    public async Task<IEnumerable<PagoResponse>> ObtenerPagosContratoAsync(int contratoId)
    {
        var pagos = await _pagoRepository.GetByContratoIdAsync(contratoId);
        var responses = new List<PagoResponse>();

        foreach (var pago in pagos)
        {
            responses.Add(new PagoResponse
            {
                Id = pago.Id,
                ContratoId = contratoId,
                FechaProgramada = pago.CalculoPago?.FechaCorte ?? default,
                FechaPago = pago.FechaPago,
                MontoBruto = pago.GananciaBruta,
                Comision = pago.ComisionAplicada,
                MontoNeto = pago.MontoEntregado,
                Estado = pago.Estado.ToString(),
                MetodoPago = pago.MetodoPago.ToString()
            });
        }

        return responses;
    }

    public async Task<PagoResponse> RegistrarDecisionAsync(DecisionPagoRequest request)
    {
        // Implementar lógica de decisión de pago
        // (RetiroTotal, ReinversionTotal, ReinversionParcial, etc.)
        throw new NotImplementedException("Registro de decisión pendiente de implementar");
    }

    public async Task<PagoResponse?> ObtenerProximoPagoAsync(int contratoId)
    {
        var proximoPago = await _pagoRepository.GetProximoPagoAsync(contratoId);
        if (proximoPago == null) return null;

        return new PagoResponse
        {
            Id = proximoPago.Id,
            ContratoId = contratoId,
            FechaProgramada = proximoPago.CalculoPago.FechaCorte,
            MontoBruto = proximoPago.GananciaBruta,
            Comision = proximoPago.ComisionAplicada,
            MontoNeto = proximoPago.MontoEntregado,
            Estado = proximoPago.Estado.ToString()
        };
    }
}
