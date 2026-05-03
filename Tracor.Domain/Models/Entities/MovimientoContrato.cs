using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class MovimientoContrato
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public int? CicloPagoId { get; set; }
    public TipoMovimientoContrato TipoMovimiento { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public int UsuarioId { get; set; }

    public Contrato Contrato { get; set; } = null!;
    public CicloPago? CicloPago { get; set; }
    public Usuario Usuario { get; set; } = null!;
}