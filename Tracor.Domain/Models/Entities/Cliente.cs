namespace Tradecorp.Domain.Models.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? TipoPersona { get; set; }
    public int UsuarioEjecutivoId { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Notas { get; set; }
    public bool Activo { get; set; } = true;

    public Usuario UsuarioEjecutivo { get; set; } = null!;
    public ICollection<ClienteCuenta> ClienteCuentas { get; set; } = new List<ClienteCuenta>();
    public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<AgendaPago> AgendaPagos { get; set; } = new List<AgendaPago>();
    public ICollection<ClienteBeneficiario> Beneficiarios { get; set; } = new List<ClienteBeneficiario>();
    public ICollection<ClienteBeneficiarioHistorico> BeneficiariosHistorico { get; set; } = new List<ClienteBeneficiarioHistorico>();
}