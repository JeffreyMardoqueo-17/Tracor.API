namespace Tradecorp.Domain.Models.Entities;

public class CalculoPago
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public int CicloPagoId { get; set; }
    public DateOnly FechaCorte { get; set; }
    public int DiasCalculados { get; set; }
    public decimal MontoCalculado { get; set; }

    public Contrato Contrato { get; set; } = null!;
    public CicloPago CicloPago { get; set; } = null!;
    public Pago? Pago { get; set; }
}