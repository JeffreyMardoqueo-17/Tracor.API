using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Domain.Models.Entities;

/// <summary>
/// Entidad: Beneficiario Asignado a un Contrato
/// Mapea el porcentaje que cada beneficiario del cliente recibe sobre un contrato específico.
/// </summary>
public class ContratoBeneficiario
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public int ClienteBeneficiarioId { get; set; }

    /// <summary>
    /// Porcentaje que este beneficiario recibe de los rendimientos del contrato.
    /// Debe estar entre 0 y 100, y la suma de todos debe ser 100%.
    /// </summary>
    public decimal PorcentajeAsignado { get; set; }

    // Navegaciones
    public Contrato Contrato { get; set; } = null!;
    public ClienteBeneficiario ClienteBeneficiario { get; set; } = null!;
}
