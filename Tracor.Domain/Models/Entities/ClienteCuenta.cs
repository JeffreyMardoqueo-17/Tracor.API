namespace Tradecorp.Domain.Models.Entities;

public class ClienteCuenta
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int BancoId { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public string? TipoCuenta { get; set; }
    public bool EsPrincipal { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public Banco Banco { get; set; } = null!;
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}