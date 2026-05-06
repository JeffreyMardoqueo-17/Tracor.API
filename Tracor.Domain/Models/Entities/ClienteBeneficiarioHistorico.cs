using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

/// <summary>
/// Registra el historial de cambios en beneficiarios, especialmente cuando un beneficiario fallece
/// Principio: NADA SE ELIMINA, TODO SE REGISTRA
/// </summary>
public class ClienteBeneficiarioHistorico
{
    public int Id { get; set; }

    /// <summary>
    /// Referencia al beneficiario
    /// </summary>
    public int ClienteBeneficiarioId { get; set; }

    /// <summary>
    /// Referencia al cliente
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// Nombre del beneficiario en el momento del evento
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// DUI del beneficiario en el momento del evento
    /// </summary>
    public string? DUI { get; set; }

    /// <summary>
    /// Porcentaje asignado en el momento del evento
    /// Se guarda para auditoría y cálculos históricos
    /// </summary>
    public decimal PorcentajeAsignado { get; set; }

    /// <summary>
    /// Tipo de relación en el momento del evento
    /// </summary>
    public ClienteBeneficiarioTipo TipoRelacion { get; set; }

    /// <summary>
    /// Descripción del evento que generó el registro histórico
    /// Ejemplo: "Beneficiario fallecido", "Beneficiario retirado", etc.
    /// </summary>
    public string Evento { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en que ocurrió el evento
    /// </summary>
    public DateTime FechaEvento { get; set; }

    /// <summary>
    /// Información adicional / notas sobre el evento
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// Navegación a la entidad ClienteBeneficiario
    /// </summary>
    public ClienteBeneficiario ClienteBeneficiario { get; set; } = null!;

    /// <summary>
    /// Navegación a la entidad Cliente
    /// </summary>
    public Cliente Cliente { get; set; } = null!;
}
