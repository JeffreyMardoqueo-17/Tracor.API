using Tradecorp.Domain.Models.Enums;
using System.Text.Json;

namespace Tradecorp.Domain.Models.Entities;

/// <summary>
/// Entidad: Auditoría de Contratos
/// Registra todos los cambios realizados a contratos para trazabilidad completa.
/// Implementa patrón de auditoría con valores antiguos y nuevos en JSONB.
/// </summary>
public class AuditoriaContrato
{
    public long Id { get; set; }
    public int ContratoId { get; set; }

    /// <summary>
    /// Tipo de movimiento (Creación, Actualización, Reinversión, etc.)
    /// </summary>
    public TipoOperacion TipoMovimiento { get; set; }

    /// <summary>
    /// Estado anterior en formato JSON.
    /// </summary>
    public JsonDocument? ValorAnterior { get; set; }

    /// <summary>
    /// Estado nuevo en formato JSON.
    /// </summary>
    public JsonDocument? ValorNuevo { get; set; }

    /// <summary>
    /// Observaciones o contexto de la operación.
    /// </summary>
    public string? Observacion { get; set; }

    /// <summary>
    /// Usuario que ejecutó la operación.
    /// </summary>
    public int UsuarioId { get; set; }

    /// <summary>
    /// Fecha de la operación.
    /// </summary>
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

    // Navegaciones
    public Contrato Contrato { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;

    /// <summary>
    /// Factory method para crear un registro de auditoría.
    /// </summary>
    public static AuditoriaContrato Crear(
        int contratoId,
        TipoOperacion tipo,
        int usuarioId,
        object? valorAnterior = null,
        object? valorNuevo = null,
        string? observacion = null)
    {
        var registro = new AuditoriaContrato
        {
            ContratoId = contratoId,
            TipoMovimiento = tipo,
            UsuarioId = usuarioId,
            Observacion = observacion,
            FechaMovimiento = DateTime.UtcNow
        };

        if (valorAnterior != null)
            registro.ValorAnterior = JsonSerializer.SerializeToDocument(valorAnterior);

        if (valorNuevo != null)
            registro.ValorNuevo = JsonSerializer.SerializeToDocument(valorNuevo);

        return registro;
    }
}