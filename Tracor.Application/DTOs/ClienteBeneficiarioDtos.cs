namespace Tradecorp.Application.DTOs;

/// <summary>
/// DTO para crear un beneficiario
/// </summary>
public class CreateClienteBeneficiarioRequest
{
    /// <summary>
    /// Nombre completo del beneficiario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// DUI del beneficiario
    /// </summary>
    public string? DUI { get; set; }

    /// <summary>
    /// Correo electrónico del beneficiario
    /// </summary>
    public string? Correo { get; set; }

    /// <summary>
    /// Teléfono del beneficiario
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Dirección del beneficiario
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Porcentaje de asignación (0-100)
    /// </summary>
    public decimal Porcentaje { get; set; }

    /// <summary>
    /// Tipo de relación con el cliente
    /// </summary>
    public string TipoRelacion { get; set; } = string.Empty;

    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para actualizar información de un beneficiario
/// </summary>
public class UpdateClienteBeneficiarioRequest
{
    /// <summary>
    /// Nombre completo del beneficiario
    /// </summary>
    public string? NombreCompleto { get; set; }

    /// <summary>
    /// DUI del beneficiario
    /// </summary>
    public string? DUI { get; set; }

    /// <summary>
    /// Correo electrónico del beneficiario
    /// </summary>
    public string? Correo { get; set; }

    /// <summary>
    /// Teléfono del beneficiario
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Dirección del beneficiario
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Porcentaje de asignación (0-100)
    /// </summary>
    public decimal? Porcentaje { get; set; }

    /// <summary>
    /// Tipo de relación con el cliente
    /// </summary>
    public string? TipoRelacion { get; set; }

    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string? Notas { get; set; }
}

/// <summary>
/// DTO de respuesta para un beneficiario
/// </summary>
public class ClienteBeneficiarioResponse
{
    /// <summary>
    /// ID del beneficiario
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre completo del beneficiario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// DUI del beneficiario
    /// </summary>
    public string? DUI { get; set; }

    /// <summary>
    /// Correo electrónico
    /// </summary>
    public string? Correo { get; set; }

    /// <summary>
    /// Teléfono
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Dirección
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Porcentaje de asignación
    /// </summary>
    public decimal Porcentaje { get; set; }

    /// <summary>
    /// Tipo de relación con el cliente
    /// </summary>
    public string TipoRelacion { get; set; } = string.Empty;

    /// <summary>
    /// Estado del beneficiario (Activo, Inactivo, Fallecido)
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de última modificación
    /// </summary>
    public DateTime FechaActualizacion { get; set; }
}

/// <summary>
/// DTO para respuesta de validación de beneficiarios
/// </summary>
public class ClienteBeneficiarioValidacionResponse
{
    /// <summary>
    /// ¿Los beneficiarios están completos (suman 100%)?
    /// </summary>
    public bool BeneficiariosCompletos { get; set; }

    /// <summary>
    /// Suma total del porcentaje de beneficiarios activos
    /// </summary>
    public decimal SumaPorcentaje { get; set; }

    /// <summary>
    /// Cantidad de beneficiarios activos
    /// </summary>
    public int CantidadBeneficiariosActivos { get; set; }

    /// <summary>
    /// Mensaje de validación
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
/// DTO para registrar el fallecimiento de un beneficiario
/// </summary>
public class RegistrarFallecimientoBeneficiarioRequest
{
    /// <summary>
    /// Fecha del fallecimiento
    /// </summary>
    public DateTime FechaFallecimiento { get; set; }

    /// <summary>
    /// Notas sobre el fallecimiento
    /// </summary>
    public string? Notas { get; set; }
}
