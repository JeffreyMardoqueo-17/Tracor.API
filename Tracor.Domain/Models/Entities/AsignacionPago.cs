using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class AsignacionPago
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int CicloPagoId { get; set; }
    public DateOnly FechaAsignacion { get; set; }
    public JornadaAsignacion? Jornada { get; set; }
    public int? BancoId { get; set; }
    public TipoPago? TipoPago { get; set; }
    public string? Observaciones { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public CicloPago CicloPago { get; set; } = null!;
    public Banco? Banco { get; set; }
    public ICollection<AsignacionDetalle> Detalles { get; set; } = new List<AsignacionDetalle>();
}