using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class ReglaPago
{
    public int Id { get; set; }
    public int PlanPagoId { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal MontoMin { get; set; }
    public decimal MontoMax { get; set; }
    public TipoPago TipoPago { get; set; }

    public PlanPago PlanPago { get; set; } = null!;
}