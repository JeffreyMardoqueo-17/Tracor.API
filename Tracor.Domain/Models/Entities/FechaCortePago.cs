namespace Tradecorp.Domain.Models.Entities;

public class FechaCortePago
{
    public int Id { get; set; }
    public int ConfiguracionCortePagoId { get; set; }
    public int Orden { get; set; }
    public int Dia { get; set; }
    public int Mes { get; set; }

    public ConfiguracionCortePago ConfiguracionCortePago { get; set; } = null!;
}
