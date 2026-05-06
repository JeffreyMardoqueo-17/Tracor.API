namespace Tradecorp.Domain.Models.Entities;

public class ConfiguracionCortePago
{
    public int Id { get; set; }
    public int ConfiguracionSistemaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ConfiguracionSistema ConfiguracionSistema { get; set; } = null!;
    public ICollection<FechaCortePago> FechasCorte { get; set; } = new List<FechaCortePago>();
}
