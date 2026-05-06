namespace Tradecorp.Application.DTOs;

/// <summary>
/// DTO para crear un nuevo cliente
/// </summary>
public class CreateClienteRequest
{
    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de documento (DUI, Pasaporte, etc.)
    /// </summary>
    public string? TipoDocumento { get; set; }

    /// <summary>
    /// Número de documento único
    /// </summary>
    public string? NumeroDocumento { get; set; }

    /// <summary>
    /// Tipo de persona (Natural, Jurídica)
    /// </summary>
    public string? TipoPersona { get; set; }

    /// <summary>
    /// Correo electrónico del cliente
    /// </summary>
    public string? Correo { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Notas adicionales sobre el cliente
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// ID del usuario ejecutivo asignado (opcional, usa el JWT si no se envía)
    /// </summary>
    public int? UsuarioEjecutivoId { get; set; }

    /// <summary>
    /// Información de la cuenta bancaria principal
    /// </summary>
    public CreateClienteCuentaRequest? CuentaBancaria { get; set; }
}

/// <summary>
/// DTO para crear la cuenta bancaria del cliente
/// </summary>
public class CreateClienteCuentaRequest
{
    /// <summary>
    /// ID del banco
    /// </summary>
    public int BancoId { get; set; }

    /// <summary>
    /// Número de cuenta
    /// </summary>
    public string NumeroCuenta { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de cuenta (Corriente, Ahorros, etc.)
    /// </summary>
    public string? TipoCuenta { get; set; }
}

/// <summary>
/// DTO para actualizar información básica del cliente
/// </summary>
public class UpdateClienteRequest
{
    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string? NombreCompleto { get; set; }

    /// <summary>
    /// Correo electrónico del cliente
    /// </summary>
    public string? Correo { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string? Notas { get; set; }
}

/// <summary>
/// DTO de respuesta para un cliente
/// </summary>
public class ClienteResponse
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Código único del cliente
    /// </summary>
    public string CodigoCliente { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de documento
    /// </summary>
    public string? TipoDocumento { get; set; }

    /// <summary>
    /// Número de documento
    /// </summary>
    public string? NumeroDocumento { get; set; }

    /// <summary>
    /// Tipo de persona
    /// </summary>
    public string? TipoPersona { get; set; }

    /// <summary>
    /// Correo electrónico
    /// </summary>
    public string? Correo { get; set; }

    /// <summary>
    /// Teléfono
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Notas
    /// </summary>
    public string? Notas { get; set; }

    /// <summary>
    /// ¿Está activo el cliente?
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// ID del usuario ejecutivo asignado
    /// </summary>
    public int UsuarioEjecutivoId { get; set; }

    /// <summary>
    /// Nombre del usuario ejecutivo asignado
    /// </summary>
    public string? NombreEjecutivo { get; set; }

    /// <summary>
    /// Cuentas bancarias del cliente
    /// </summary>
    public ICollection<ClienteCuentaResponse> CuentasBancarias { get; set; } = new List<ClienteCuentaResponse>();

    /// <summary>
    /// Beneficiarios del cliente
    /// </summary>
    public ICollection<ClienteBeneficiarioResponse> Beneficiarios { get; set; } = new List<ClienteBeneficiarioResponse>();

    /// <summary>
    /// Contratos del cliente
    /// </summary>
    public ICollection<ContratoResumenResponse> Contratos { get; set; } = new List<ContratoResumenResponse>();

    /// <summary>
    /// Indica si el cliente tiene al menos un contrato activo
    /// </summary>
    public bool TieneContratoActivo { get; set; }

    /// <summary>
    /// Cantidad de beneficiarios activos
    /// </summary>
    public int CantidadBeneficiariosActivos { get; set; }

    /// <summary>
    /// Suma del porcentaje de beneficiarios activos (debe ser 100% si está completo)
    /// </summary>
    public decimal SumaPorcentajeBeneficiarios { get; set; }
}

/// <summary>
/// DTO de respuesta para cuenta bancaria del cliente
/// </summary>
public class ClienteCuentaResponse
{
    /// <summary>
    /// ID de la cuenta
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del banco
    /// </summary>
    public string NombreBanco { get; set; } = string.Empty;

    /// <summary>
    /// Número de cuenta
    /// </summary>
    public string NumeroCuenta { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de cuenta
    /// </summary>
    public string? TipoCuenta { get; set; }

    /// <summary>
    /// ¿Es la cuenta principal?
    /// </summary>
    public bool EsPrincipal { get; set; }
}

/// <summary>
/// DTO de respuesta para resumen de contrato
/// </summary>
public class ContratoResumenResponse
{
    /// <summary>
    /// ID del contrato
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Número de contrato
    /// </summary>
    public string NumeroContrato { get; set; } = string.Empty;

    /// <summary>
    /// Capital inicial
    /// </summary>
    public decimal CapitalInicial { get; set; }

    /// <summary>
    /// Porcentaje mensual
    /// </summary>
    public decimal PorcentajeMensual { get; set; }

    /// <summary>
    /// ¿Está activo?
    /// </summary>
    public bool Activo { get; set; }
}
