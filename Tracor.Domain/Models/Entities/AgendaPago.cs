using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class AgendaPago
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int CicloPagoId { get; set; }
    public DateTime FechaProgramada { get; set; }
    public TipoAgendaPago? Tipo { get; set; }
    public EstadoAgendaPago? Estado { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public CicloPago CicloPago { get; set; } = null!;
}