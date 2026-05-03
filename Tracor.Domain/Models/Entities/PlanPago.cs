namespace Tradecorp.Domain.Models.Entities;

public class PlanPago
{
    public int Id { get; set; }
    public int CicloPagoId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public CicloPago CicloPago { get; set; } = null!;
    public ICollection<ReglaPago> ReglasPago { get; set; } = new List<ReglaPago>();
}