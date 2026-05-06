using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class ContratoRelacion
{
    public int Id { get; set; }
    public int ContratoOrigenId { get; set; }
    public int ContratoDestinoId { get; set; }
    public TipoRelacionContrato TipoRelacion { get; set; }
    public DateTime FechaRelacion { get; set; }
    public decimal? MontoTransferido { get; set; }
    public string? Observacion { get; set; }
    public Guid GrupoOperacionId { get; set; }
    public int? UsuarioId { get; set; }

    public Contrato ContratoOrigen { get; set; } = null!;
    public Contrato ContratoDestino { get; set; } = null!;
    public Usuario? Usuario { get; set; }
}
