using Tradecorp.Domain.Models.Enums;
using Tradecorp.Domain.Models.ValueObjects;
using Tradecorp.Domain.Exceptions;

namespace Tradecorp.Domain.Models.Rules;

/// <summary>
/// Contenedor de reglas de negocio del módulo de contratos.
/// Cada regla es una clase que encapsula validaciones de dominio puras.
/// </summary>
public static class ContratoRules
{
    /// <summary>
    /// Un contrato debe tener capital inicial mayor a cero.
    /// </summary>
    public static void ValidarCapitalInicial(Capital capital)
    {
        if (capital.Valor <= 0)
            throw ReglaNegocioException.CapitalInvalido();
    }

    /// <summary>
    /// El porcentaje mensual debe estar en rango válido.
    /// </summary>
    public static void ValidarPorcentajeMensual(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw ReglaNegocioException.PorcentajeFueraDeRango(porcentaje);
    }

    /// <summary>
    /// Máximo 4 beneficiarios por contrato.
    /// </summary>
    public static void ValidarCantidadBeneficiarios(int cantidad)
    {
        if (cantidad > 4)
            throw ReglaNegocioException.MaximoBeneficiarios();
    }

    /// <summary>
    /// Los porcentajes de beneficiarios deben sumar exactamente 100%.
    /// </summary>
    public static void ValidarPorcentajesBeneficiarios(decimal sumaPorcentajes)
    {
        if (Math.Abs(sumaPorcentajes - 100m) > 0.01m) // Permitir pequeños redondeos
            throw ReglaNegocioException.BeneficiariosPorcentajeInvalido(sumaPorcentajes);
    }

    /// <summary>
    /// Un contrato solo puede ser unificado si su estado lo permite.
    /// </summary>
    public static void ValidarEstadoParaUnificacion(EstadoContrato estado)
    {
        if (estado != EstadoContrato.Activo)
            throw ReglaNegocioException.EstadoContratoInvalido(estado.ToString());
    }

    /// <summary>
    /// Validar que se puede cerrar el contrato para una operación.
    /// </summary>
    public static void ValidarEstadoParaCierre(EstadoContrato estado)
    {
        if (estado != EstadoContrato.Activo)
            throw ReglaNegocioException.ContratoNoActivo();
    }

    /// <summary>
    /// Validar que la modalidad de rendimiento es válida.
    /// </summary>
    public static void ValidarModalidadRendimiento(ModalidadRendimiento modalidad)
    {
        if (!Enum.IsDefined(typeof(ModalidadRendimiento), modalidad))
            throw ReglaNegocioException.EstadoContratoInvalido("Modalidad inválida");
    }

    /// <summary>
    /// Comisión de retiro debe estar entre 0 y 10%.
    /// </summary>
    public static void ValidarComisionRetiro(decimal comision)
    {
        if (comision < 0 || comision > 10)
            throw new ReglaNegocioException("La comisión de retiro debe estar entre 0% y 10%.");
    }

    /// <summary>
    /// Un contrato que ha sido unificado no puede ser desunificado directamente.
    /// </summary>
    public static void ValidarEstadoParaDesunificacion(EstadoContrato estado)
    {
        if (estado != EstadoContrato.Unificado)
            throw ReglaNegocioException.EstadoContratoInvalido("Solo contratos unificados pueden desunificarse");
    }

    /// <summary>
    /// El capital a inyectar debe ser positivo.
    /// </summary>
    public static void ValidarCapitalInyectado(Capital capital)
    {
        if (capital.Valor <= 0)
            throw new ReglaNegocioException("El capital a inyectar debe ser mayor a cero.");
    }
}

/// <summary>
/// Reglas para operaciones de unificación de contratos.
/// </summary>
public static class UnificacionRules
{
    /// <summary>
    /// No se puede unificar un solo contrato.
    /// </summary>
    public static void ValidarCantidadContratosAUnificar(int cantidad)
    {
        if (cantidad < 2)
            throw new ReglaNegocioException("Se requieren al menos 2 contratos para unificar.");
    }

    /// <summary>
    /// Solo contratos activos pueden ser unificados.
    /// </summary>
    public static void ValidarTodosActivosParaUnificacion(IEnumerable<EstadoContrato> estados)
    {
        if (estados.Any(e => e != EstadoContrato.Activo))
            throw new ReglaNegocioException("Solo se pueden unificar contratos en estado Activo.");
    }

    /// <summary>
    /// Verificar que los contratos permiten unificación.
    /// </summary>
    public static void ValidarPermiteUnificacion(bool permite)
    {
        if (!permite)
            throw ReglaNegocioException.UnificacionNoPermitida();
    }
}

/// <summary>
/// Reglas para reinversión de ganancias.
/// </summary>
public static class ReinversionRules
{
    /// <summary>
    /// El porcentaje para reinversión debe estar en rango válido.
    /// </summary>
    public static void ValidarPorcentajeReinversion(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw ReglaNegocioException.PorcentajeFueraDeRango(porcentaje);
    }

    /// <summary>
    /// El nuevo porcentaje mensual debe estar en rango permisible.
    /// </summary>
    public static void ValidarNuevoPorcentajeMensual(decimal porcentaje, decimal minimo = 0, decimal maximo = 8.5m)
    {
        if (porcentaje < minimo || porcentaje > maximo)
            throw new ReglaNegocioException($"El porcentaje mensual debe estar entre {minimo}% y {maximo}%.");
    }
}

/// <summary>
/// Reglas para gestión de beneficiarios por contrato.
/// </summary>
public static class BeneficiarioRules
{
    /// <summary>
    /// Validar que el porcentaje asignado al beneficiario es válido.
    /// </summary>
    public static void ValidarPorcentajeAsignado(decimal porcentaje)
    {
        if (porcentaje <= 0 || porcentaje > 100)
            throw ReglaNegocioException.PorcentajeFueraDeRango(porcentaje);
    }

    /// <summary>
    /// El porcentaje del nuevo beneficiario no debe exceder el disponible.
    /// </summary>
    public static void ValidarPorcentajeDisponible(decimal porcentajeNuevo, decimal sumaBeneficiarios)
    {
        var total = sumaBeneficiarios + porcentajeNuevo;
        if (total > 100)
            throw ReglaNegocioException.BeneficiariosPorcentajeInvalido(total);
    }
}
