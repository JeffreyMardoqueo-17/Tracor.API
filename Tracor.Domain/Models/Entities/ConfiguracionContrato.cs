using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class ConfiguracionContrato
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public decimal CapitalBase { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public ConfiguracionContratoTipo Tipo { get; set; }

    public Contrato Contrato { get; set; } = null!;
}