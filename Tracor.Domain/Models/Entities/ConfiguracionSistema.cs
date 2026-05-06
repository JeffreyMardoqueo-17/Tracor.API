namespace Tradecorp.Domain.Models.Entities;

public class ConfiguracionSistema
{
    public int Id { get; set; }
    public int DiasCuatrimestreBase { get; set; } = 120;
    public int DiasMesBase { get; set; } = 30;
    public decimal ComisionEmpresaPorcentaje { get; set; } = 5m;
    public bool UsarDiasExactosPrimerCuatrimestre { get; set; } = true;
    public bool AplicarReglaAnioBisiesto { get; set; } = true;
    public bool Activo { get; set; } = true;
    public DateTime FechaActualizacion { get; set; }

    public ICollection<ConfiguracionCortePago> CortesPago { get; set; } = new List<ConfiguracionCortePago>();
}
