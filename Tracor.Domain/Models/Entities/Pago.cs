using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class Pago
{
    public int Id { get; set; }
    public int CalculoPagoId { get; set; }
    public int ClienteId { get; set; }
    public int CicloPagoId { get; set; }
    public decimal GananciaBruta { get; set; }
    public decimal ComisionAplicada { get; set; }
    public decimal MontoEntregado { get; set; }
    public int? DiasPagados { get; set; }
    public DateOnly? FechaInicioCalculo { get; set; }
    public DateOnly? FechaFinCalculo { get; set; }
    public MetodoPago MetodoPago { get; set; }
    public EstadoPago Estado { get; set; }
    public DateTime? FechaPago { get; set; }
    public int? EjecutadoPor { get; set; }
    public int? ClienteCuentaId { get; set; }
    public int? BancoId { get; set; }
    public ModalidadRendimiento ModalidadRendimientoAlCierre { get; set; } = ModalidadRendimiento.Normal;

    public CalculoPago CalculoPago { get; set; } = null!;
    public Cliente Cliente { get; set; } = null!;
    public CicloPago CicloPago { get; set; } = null!;
    public Usuario? UsuarioEjecutadoPor { get; set; }
    public ClienteCuenta? ClienteCuenta { get; set; }
    public Banco? Banco { get; set; }
    public AsignacionDetalle? AsignacionDetalle { get; set; }
    public ICollection<DecisionPago> Decisiones { get; set; } = new List<DecisionPago>();
}