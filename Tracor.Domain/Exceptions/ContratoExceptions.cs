namespace Tradecorp.Domain.Exceptions;

/// <summary>
/// Excepción base para reglas de negocio en el dominio de contratos.
/// </summary>
public class ReglaNegocioException : DomainException
{
    public ReglaNegocioException(string message) : base(message) { }

    public static ReglaNegocioException ClienteSinContratos(int clienteId)
        => new($"Cliente {clienteId} no tiene contratos activos.");

    public static ReglaNegocioException MaximoBeneficiarios()
        => new($"Un contrato no puede tener más de 4 beneficiarios.");

    public static ReglaNegocioException BeneficiariosPorcentajeInvalido(decimal suma)
        => new($"Los beneficiarios deben sumar 100%. Suma actual: {suma}%");

    public static ReglaNegocioException CapitalInvalido()
        => new($"El capital inicial debe ser mayor a cero.");

    public static ReglaNegocioException PorcentajeFueraDeRango(decimal porcentaje)
        => new($"El porcentaje debe estar entre 0 y 100%. Valor: {porcentaje}%");

    public static ReglaNegocioException EstadoContratoInvalido(string estado)
        => new($"El contrato no puede estar en estado '{estado}' para esta operación.");

    public static ReglaNegocioException ContratoNoActivo()
        => new($"El contrato debe estar activo para esta operación.");

    public static ReglaNegocioException UnificacionNoPermitida()
        => new($"Este contrato no permite unificación.");

    public static ReglaNegocioException MontoTransferenciaInvalido()
        => new($"El monto a transferir debe ser válido.");
}

/// <summary>
/// Excepción de dominio genérica.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>
/// Excepción cuando un contrato no existe.
/// </summary>
public class ContratoNoEncontradoException : DomainException
{
    public ContratoNoEncontradoException(int contratoId)
        : base($"Contrato con ID {contratoId} no encontrado.") { }
}

/// <summary>
/// Excepción cuando una operación no es permitida en el estado actual.
/// </summary>
public class OperacionNoPermitidaException : DomainException
{
    public OperacionNoPermitidaException(string operacion, string estadoActual)
        : base($"La operación '{operacion}' no es permitida en estado '{estadoActual}'.") { }
}
