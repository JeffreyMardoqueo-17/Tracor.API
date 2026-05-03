namespace Tradecorp.Domain.Models.Entities;

public class AsignacionDetalle
{
    public int Id { get; set; }
    public int AsignacionId { get; set; }
    public int PagoId { get; set; }

    public AsignacionPago Asignacion { get; set; } = null!;
    public Pago Pago { get; set; } = null!;
}