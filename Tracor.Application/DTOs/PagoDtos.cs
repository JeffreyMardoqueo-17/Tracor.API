namespace Tradecorp.Application.DTOs;

/// <summary>
/// Request para calcular ganancia basado en días exactos
/// Sigue las reglas de project.md
/// </summary>
public class CalculoGananciaRequest
{
    /// <summary>
    /// ID del contrato para el cálculo
    /// </summary>
    public int ContratoId { get; set; }

    /// <summary>
    /// Fecha de inicio del cálculo (normalmente fecha de inicio del contrato)
    /// </summary>
    public DateOnly FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin del cálculo (normalmente hoy o fecha de cierre)
    /// </summary>
    public DateOnly FechaFin { get; set; }

    /// <summary>
    /// Capital base para el cálculo
    /// </summary>
    public decimal Capital { get; set; }

    /// <summary>
    /// Porcentaje mensual (6% - 8.50%)
    /// </summary>
    public decimal PorcentajeMensual { get; set; }

    /// <summary>
    /// Modalidad de rendimiento: Normal o InteresCompuesto
    /// </summary>
    public string Modalidad { get; set; } = "Normal";
}

/// <summary>
/// Response con el cálculo detallado de ganancia
/// </summary>
public class CalculoGananciaResponse
{
    public int DiasTranscurridos { get; set; }
    public int DiasFebrero { get; set; }
    public int DiasOtrosMeses { get; set; }
    public decimal GananciaBruta { get; set; }
    public decimal ComisionEmpresa { get; set; }
    public decimal GananciaNeta { get; set; }
    public List<DetalleMesDto> DetallePorMes { get; set; } = new();
}

/// <summary>
/// Detalle del cálculo por mes
/// </summary>
public class DetalleMesDto
{
    public string Mes { get; set; } = string.Empty;
    public int Dias { get; set; }
    public decimal Ganancia { get; set; }
}

/// <summary>
/// DTO de respuesta para pagos
/// </summary>
public class PagoResponse
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public DateOnly FechaProgramada { get; set; }
    public DateTime? FechaPago { get; set; }
    public decimal MontoBruto { get; set; }
    public decimal Comision { get; set; }
    public decimal MontoNeto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MetodoPago { get; set; }
    public ContratoResumenResponse? Contrato { get; set; }
}

/// <summary>
/// Request para registrar una decisión de pago
/// </summary>
public class DecisionPagoRequest
{
    public int PagoId { get; set; }
    public string TipoDecision { get; set; } = string.Empty;
    public decimal? MontoRetirado { get; set; }
    public decimal? MontoReinvertido { get; set; }
    public string? MetodoRetiro { get; set; }
    public string? Observaciones { get; set; }
}
