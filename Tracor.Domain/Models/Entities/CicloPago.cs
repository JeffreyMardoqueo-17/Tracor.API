namespace Tradecorp.Domain.Models.Entities;

public class CicloPago
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    public ICollection<PlanPago> PlanesPago { get; set; } = new List<PlanPago>();
    public ICollection<CalculoPago> CalculosPago { get; set; } = new List<CalculoPago>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<MovimientoContrato> MovimientosContrato { get; set; } = new List<MovimientoContrato>();
    public ICollection<AgendaPago> AgendaPagos { get; set; } = new List<AgendaPago>();
    public ICollection<AsignacionPago> AsignacionesPago { get; set; } = new List<AsignacionPago>();
}