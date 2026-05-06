namespace Tradecorp.Application.DTOs;

/// <summary>
/// DTO para crear un nuevo contrato
/// Regla: El capital debe ser > 0, porcentaje entre 6% y 8.50%
/// </summary>
public class CreateContratoRequest
{
    /// <summary>
    /// ID del cliente propietario del contrato
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Capital inicial a invertir (debe ser > 0)
    /// </summary>
    public decimal CapitalInicial { get; set; }

    /// <summary>
    /// Porcentaje de rendimiento mensual (6% - 8.50%)
    /// </summary>
    public decimal PorcentajeMensual { get; set; }

    /// <summary>
    /// Comisión por retiro (opcional, default 0)
    /// </summary>
    public decimal? ComisionRetiro { get; set; } = 0;

    /// <summary>
    /// Modalidad de rendimiento: Normal o InteresCompuesto
    /// </summary>
    public string ModalidadRendimiento { get; set; } = "Normal";

    /// <summary>
    /// Permite unificación de contratos
    /// </summary>
    public bool PermiteUnificacion { get; set; } = true;

    /// <summary>
    /// Beneficiarios a asignar al contrato (opcional, pueden reutilizarse de otros contratos)
    /// </summary>
    public List<AsignarBeneficiarioRequest>? Beneficiarios { get; set; }
}

/// <summary>
/// DTO para asignar un beneficiario a un contrato
/// </summary>
public class AsignarBeneficiarioRequest
{
    /// <summary>
    /// ID del beneficiario existente del cliente (si es null, se crea uno nuevo)
    /// </summary>
    public int? ClienteBeneficiarioId { get; set; }

    /// <summary>
    /// Datos para crear nuevo beneficiario si no existe
    /// </summary>
    public CreateBeneficiarioRequest? Beneficiario { get; set; }

    /// <summary>
    /// Porcentaje asignado al beneficiario en este contrato
    /// </summary>
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para crear un nuevo beneficiario
/// </summary>
public class CreateBeneficiarioRequest
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string? DUI { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string TipoRelacion { get; set; } = string.Empty; // Conyuge, Hijo, etc.
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para actualizar un contrato
/// </summary>
public class UpdateContratoRequest
{
    public decimal? PorcentajeMensual { get; set; }
    public decimal? ComisionRetiro { get; set; }
    public string? ModalidadRendimiento { get; set; }
    public bool? PermiteUnificacion { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO de respuesta para un contrato
/// </summary>
public class ContratoResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public decimal CapitalInicial { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public decimal ComisionRetiro { get; set; }
    public string ModalidadRendimiento { get; set; } = string.Empty;
    public bool PermiteUnificacion { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public ClienteResponse? Cliente { get; set; }
    public List<BeneficiarioContratoResponse>? Beneficiarios { get; set; }
}

/// <summary>
/// DTO para mostrar beneficiarios asignados a un contrato
/// </summary>
public class BeneficiarioContratoResponse
{
    public int Id { get; set; }
    public int ClienteBeneficiarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? DUI { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para listar contratos de un cliente
/// </summary>
public class ContratoListaResponse
{
    public int Id { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public decimal CapitalInicial { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public string ModalidadRendimiento { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateTime? FechaCierre { get; set; }
}
