namespace Tradecorp.Domain.Models.Entities;

public class Banco
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public ICollection<ClienteCuenta> ClienteCuentas { get; set; } = new List<ClienteCuenta>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<AsignacionPago> AsignacionesPago { get; set; } = new List<AsignacionPago>();
}