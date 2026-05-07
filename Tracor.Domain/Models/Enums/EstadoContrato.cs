namespace Tradecorp.Domain.Models.Enums;

/// <summary>
/// Estados válidos para un contrato en su ciclo de vida.
/// </summary>
public enum EstadoContrato
{
    Activo,           // Contrato vigente, generando rendimientos
    Finalizado,       // Completó su ciclo natural o fue cerrado
    Unificado,        // Fue fusionado en otro contrato
    Anulado           // Cancelado sin cumplir ciclo
}

/// <summary>
/// Tipos de operaciones que pueden ejecutarse sobre contratos.
/// </summary>
public enum TipoOperacion
{
    Creacion,
    Actualizacion,
    Reinversion,
    CambioPorcentaje,
    InyeccionCapital,
    Unificacion,
    Desunificacion,
    Finalizacion,
    CambioEstado,
    CambioBeneficiarios,
    CambioCapital
}

/// <summary>
/// Modalidades de cálculo de rendimiento.
/// </summary>
public enum ModalidadRendimiento
{
    Normal,              // Interés simple
    InteresCompuesto     // Interés compuesto
}

/// <summary>
/// Tipos de relación entre contratos (unificación, inyección, etc.).
/// </summary>
public enum TipoRelacionContrato
{
    Unificacion,
    Desunificacion,
    InyeccionCapital,
    Reinversion
}
