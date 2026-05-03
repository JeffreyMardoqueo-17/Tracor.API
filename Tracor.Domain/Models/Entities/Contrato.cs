namespace Tradecorp.Domain.Models.Entities;

public class Contrato
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public decimal CapitalInicial { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public decimal ComisionRetiro { get; set; }
    public bool Activo { get; set; } = true;

    public Cliente Cliente { get; set; } = null!;
    public ICollection<ConfiguracionContrato> ConfiguracionesContrato { get; set; } = new List<ConfiguracionContrato>();
    public ICollection<CalculoPago> CalculosPago { get; set; } = new List<CalculoPago>();
    public ICollection<MovimientoContrato> MovimientosContrato { get; set; } = new List<MovimientoContrato>();
}