using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UsuarioRol Rol { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    public ICollection<Cliente> ClientesAsignados { get; set; } = new List<Cliente>();
    public ICollection<MovimientoContrato> MovimientosContrato { get; set; } = new List<MovimientoContrato>();
    public ICollection<Pago> PagosEjecutados { get; set; } = new List<Pago>();
    public ICollection<AsignacionPago> AsignacionesPago { get; set; } = new List<AsignacionPago>();
    public ICollection<ContratoRelacion> RelacionesContratoEjecutadas { get; set; } = new List<ContratoRelacion>();
}