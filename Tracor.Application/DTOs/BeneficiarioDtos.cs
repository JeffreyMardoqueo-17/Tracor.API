namespace Tradecorp.Application.DTOs;

/// <summary>
/// DTO para crear un nuevo beneficiario
/// </summary>
public class CreateBeneficiarioClienteRequest
{
    public int ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? DUI { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public decimal Porcentaje { get; set; }
    public string TipoRelacion { get; set; } = string.Empty; // Conyuge, Hijo, etc.
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para actualizar beneficiario
/// </summary>
public class UpdateBeneficiarioRequest
{
    public string? NombreCompleto { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public decimal? Porcentaje { get; set; }
    public string? Estado { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO de respuesta para beneficiario
/// </summary>
public class BeneficiarioResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? DUI { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public decimal Porcentaje { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
