using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

/// <summary>
/// Representa un beneficiario de un cliente
/// Regla de negocio: Los porcentajes de todos los beneficiarios activos de un cliente deben sumar exactamente 100%
/// En caso de muerte, se crea un histórico con el porcentaje y datos del beneficiario
/// </summary>
public class ClienteBeneficiario
{
    public int Id { get; set; }

    /// <summary>
    /// Referencia al cliente propietario
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Nombre completo del beneficiario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// DUI del beneficiario (para identificación)
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
    /// Debe validarse que la suma de todos los beneficiarios activos sea 100%
    /// </summary>
    public decimal Porcentaje { get; set; }

    /// <summary>
    /// Tipo de relación con el cliente (cónyuge, hijo, padre, etc.)
    /// </summary>
    public ClienteBeneficiarioTipo TipoRelacion { get; set; }

    /// <summary>
    /// Estado del beneficiario (Activo, Inactivo, Fallecido)
    /// </summary>
    public ClienteBeneficiarioEstado Estado { get; set; } = ClienteBeneficiarioEstado.Activo;

    /// <summary>
    /// Información de contacto adicional
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// Fecha de creación del beneficiario
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de última modificación
    /// </summary>
    public DateTime FechaActualizacion { get; set; }

    /// <summary>
    /// Navegación a la entidad Cliente
    /// </summary>
    public Cliente Cliente { get; set; } = null!;

    /// <summary>
    /// Historial de cambios en caso de muerte del beneficiario
    /// </summary>
    public ICollection<ClienteBeneficiarioHistorico> Historico { get; set; } = new List<ClienteBeneficiarioHistorico>();
}
