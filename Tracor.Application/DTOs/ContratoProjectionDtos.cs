namespace Tradecorp.Application.DTOs;

public class SimularContratoRequest
{
    public DateOnly FechaInicio { get; set; }
    public decimal CapitalInicial { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public string ModalidadRendimiento { get; set; } = "Normal";
    public decimal ComisionEmpresa { get; set; } = 5m;
}

public class ContratoProjectionResponse
{
    public int? ContratoId { get; set; }
    public string? NumeroContrato { get; set; }
    public string? ClienteNombre { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal CapitalInicial { get; set; }
    public decimal CapitalProyectado { get; set; }
    public decimal GananciaBrutaTotal { get; set; }
    public decimal ComisionTotal { get; set; }
    public decimal GananciaNetaTotal { get; set; }
    public int DiasFinancierosTotales { get; set; }
    public string EstadoContrato { get; set; } = string.Empty;
    public bool EsSimulacion { get; set; }
    public DateTime GeneradoEn { get; set; }
    public ProyeccionPagoResponse? ProximoPago { get; set; }
    public List<ProyeccionPagoResponse> Pagos { get; set; } = new();
}

public class ProyeccionPagoResponse
{
    public int Periodo { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaCorte { get; set; }
    public DateOnly FechaPagoInicio { get; set; }
    public DateOnly FechaPagoFin { get; set; }
    public int DiasFinancieros { get; set; }
    public decimal CapitalBase { get; set; }
    public decimal GananciaBruta { get; set; }
    public decimal Comision { get; set; }
    public decimal GananciaNeta { get; set; }
    public decimal CapitalProyectado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EsVentanaActual { get; set; }
    public bool EsSiguientePago { get; set; }
}
