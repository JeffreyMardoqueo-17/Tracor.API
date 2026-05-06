using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class DecisionPago
{
    public int Id { get; set; }
    public int PagoId { get; set; }
    public TipoDecisionPago TipoDecision { get; set; }
    public MetodoPago? MetodoRetiro { get; set; }
    public decimal MontoRetirado { get; set; }
    public decimal MontoReinvertido { get; set; }
    public decimal MontoAInteresCompuesto { get; set; }
    public DateTime FechaDecision { get; set; }
    public string? Observacion { get; set; }

    public Pago Pago { get; set; } = null!;
}
