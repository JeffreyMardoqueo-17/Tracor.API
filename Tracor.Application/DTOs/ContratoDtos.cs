namespace Tradecorp.Application.DTOs;

public class ContratoFiltroRequest
{
    public int? ClienteId { get; set; }
    public string? Estado { get; set; }
    public string? NumeroContrato { get; set; }
}

public class CreateContratoRequest
{
    public int ClienteId { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public string? NumeroContrato { get; set; }
    public decimal CapitalInicial { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public decimal? ComisionRetiro { get; set; } = 0;
    public string ModalidadRendimiento { get; set; } = "Normal";
    public bool PermiteUnificacion { get; set; } = true;
    public List<AsignarBeneficiarioRequest>? Beneficiarios { get; set; }
    public string? Observacion { get; set; }
}

public class UpdateContratoRequest
{
    public decimal? PorcentajeMensual { get; set; }
    public decimal? ComisionRetiro { get; set; }
    public string? ModalidadRendimiento { get; set; }
    public bool? PermiteUnificacion { get; set; }
    public string? Observacion { get; set; }
}

public class ReinversionContratoRequest
{
    public string Tipo { get; set; } = "Total";
    public decimal? MontoReinvertir { get; set; }
    public string? Observacion { get; set; }
}

public class InyeccionCapitalRequest
{
    public decimal Monto { get; set; }
    public string? Observacion { get; set; }
}

// DTO agregado desde ContratoOperacionesDtos.cs
public class InyectarCapitalRequest
{
    public int ContratoId { get; set; }
    public decimal CapitalAInyectar { get; set; }
    public bool AprobacionGerencial { get; set; } = false;
    public int? UsuarioAprobadorId { get; set; }
    public string? MotivoAprobacion { get; set; }
}

public class UnificarContratosRequest
{
    public int? ClienteId { get; set; }
    public List<int> ContratosOrigenIds { get; set; } = new();
    public string? NumeroContratoUnificado { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public decimal? PorcentajeMensual { get; set; }
    public string? Observacion { get; set; }
    public bool AprobacionGerencial { get; set; } = false;
    public int? UsuarioAprobadorId { get; set; }
    public string? MotivoAprobacion { get; set; }
}

public class DesunificarContratoRequest
{
    public string? Observacion { get; set; }
}

public class AsignarBeneficiarioRequest
{
    public int? ClienteBeneficiarioId { get; set; }
    public CreateBeneficiarioRapidoRequest? Beneficiario { get; set; }
    public decimal Porcentaje { get; set; }
}

public class CreateBeneficiarioRapidoRequest
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string? DUI { get; set; }
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
}

public class ContratoResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string NumeroContrato { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public decimal CapitalInicial { get; set; }
    public decimal CapitalActual { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public decimal ComisionRetiro { get; set; }
    public string ModalidadRendimiento { get; set; } = string.Empty;
    public bool PermiteUnificacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public bool Activo => Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase);
    public List<BeneficiarioContratoResponse> Beneficiarios { get; set; } = new();
    public List<ConfiguracionContratoResponse> Configuraciones { get; set; } = new();
    public List<ContratoRelacionResponse> Relaciones { get; set; } = new();
}

public class ContratoListaResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string NumeroContrato { get; set; } = string.Empty;
    public decimal CapitalInicial { get; set; }
    public decimal CapitalActual { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public string ModalidadRendimiento { get; set; } = string.Empty;
    public bool PermiteUnificacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Activo => Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase);
    public DateOnly FechaInicio { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCierre { get; set; }
}

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

// DTO agregado para mantener compatibilidad con nombres desde ContratoOperacionesDtos.cs
public class ContratoBeneficiarioResponse
{
    public int Id { get; set; }
    public int ClienteBeneficiarioId { get; set; }
    public string NombreBeneficiario { get; set; } = string.Empty;
    public decimal PorcentajeAsignado { get; set; }
}

public class ConfiguracionContratoResponse
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal CapitalBase { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
}

public class ContratoRelacionResponse
{
    public int Id { get; set; }
    public int ContratoOrigenId { get; set; }
    public int ContratoDestinoId { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
    public decimal? MontoTransferido { get; set; }
    public Guid GrupoOperacionId { get; set; }
    public string? Observacion { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaRelacion { get; set; }
}

public class HistorialFinancieroItemResponse
{
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal CapitalResultante { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public DateTime Fecha { get; set; }
    public string? Observacion { get; set; }
}

public class ContratoEventoResponse
{
    public long Id { get; set; }
    public int ContratoId { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaMovimiento { get; set; }
}

public class AuditoriaContratoResponse
{
    public long Id { get; set; }
    public int ContratoId { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public object? ValorAnterior { get; set; }
    public object? ValorNuevo { get; set; }
    public string? Observacion { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaMovimiento { get; set; }
}

// DTOs adicionales desde ContratoOperacionesDtos.cs
public class CreateContratoAdicionalRequest
{
    public int ClienteId { get; set; }
    public string NumeroContrato { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public decimal CapitalInicial { get; set; }
    public decimal PorcentajeMensual { get; set; }
    public decimal ComisionRetiro { get; set; } = 0;
}

public class ReinvertirGananciasRequest
{
    public int ContratoId { get; set; }
    public decimal Ganancias { get; set; }
    public decimal? NuevoPorcentajeMensual { get; set; }
    public bool AprobacionGerencial { get; set; } = false;
    public int? UsuarioAprobadorId { get; set; }
    public string? MotivoAprobacion { get; set; }
}

public class CambiarPorcentajeRequest
{
    public int ContratoId { get; set; }
    public decimal NuevoPorcentaje { get; set; }
    public bool AprobacionGerencial { get; set; } = false;
    public int? UsuarioAprobadorId { get; set; }
    public string? MotivoAprobacion { get; set; }
}

public class OperacionContratoResponse
{
    public bool Exitosa { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int? ContratoCreado { get; set; }
    public ContratoResponse? ContratoResultante { get; set; }
    public List<string> Errores { get; set; } = new();
}
