using Tradecorp.Domain.Models.Enums;
using Tradecorp.Domain.Models.ValueObjects;
using Tradecorp.Domain.Exceptions;
using Tradecorp.Domain.Models.Rules;

namespace Tradecorp.Domain.Models.Entities;

/// <summary>
/// Entidad de Dominio: Contrato
/// Encapsula la lógica de negocio del ciclo de vida de contratos.
/// Siempre valida reglas de negocio antes de permitir cambios de estado.
/// </summary>
public class Contrato
{
    private Capital _capitalInicial = Capital.Cero;
    private Capital _capitalActual = Capital.Cero;
    private EstadoContrato _estado;
    private decimal _porcentajeMensual;
    private decimal _comisionRetiro;
    private readonly PeriodoPago _periodoPago = new PeriodoPago();

    #region Propiedades

    public int Id { get; private set; }
    public int ClienteId { get; private set; }
    public string NumeroContrato { get; private set; } = string.Empty;
    public DateOnly FechaInicio { get; private set; }

    public decimal CapitalInicial
    {
        get => _capitalInicial.Valor;
        private set => _capitalInicial = new Capital(value);
    }
    public decimal CapitalActual
    {
        get => _capitalActual.Valor;
        private set => _capitalActual = new Capital(value);
    }
    
    public decimal PorcentajeMensual => _porcentajeMensual;
    public decimal ComisionRetiro => _comisionRetiro;
    public ModalidadRendimiento ModalidadRendimiento { get; private set; } = ModalidadRendimiento.Normal;
    public EstadoContrato Estado => _estado;
    public bool Activo => _estado == EstadoContrato.Activo;
    public bool PermiteUnificacion { get; private set; } = true;
    public DateTime FechaCreacion { get; private set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; private set; }

    public PeriodoPago PeriodoPago => _periodoPago;

    // Navegaciones
    public Cliente Cliente { get; private set; } = null!;
    public ICollection<ContratoBeneficiario> Beneficiarios { get; private set; } = new List<ContratoBeneficiario>();
    public ICollection<ConfiguracionContrato> ConfiguracionesContrato { get; private set; } = new List<ConfiguracionContrato>();
    public ICollection<CalculoPago> CalculosPago { get; private set; } = new List<CalculoPago>();
    public ICollection<MovimientoContrato> MovimientosContrato { get; private set; } = new List<MovimientoContrato>();
    public ICollection<ContratoRelacion> RelacionesComoOrigen { get; private set; } = new List<ContratoRelacion>();
    public ICollection<ContratoRelacion> RelacionesComoDestino { get; private set; } = new List<ContratoRelacion>();

    #endregion

    #region Constructores

    // EF Core
    private Contrato() { }

    /// <summary>
    /// Factory Method: Crear un nuevo contrato con todas las validaciones.
    /// </summary>
    public static Contrato Crear(
        int clienteId,
        string numeroContrato,
        DateOnly fechaInicio,
        decimal capitalInicial,
        decimal porcentajeMensual,
        decimal comisionRetiro,
        ModalidadRendimiento modalidad = ModalidadRendimiento.Normal,
        bool permiteUnificacion = true)
    {
        // Validar reglas de negocio
        var capital = new Capital(capitalInicial);
        ContratoRules.ValidarCapitalInicial(capital);
        ContratoRules.ValidarPorcentajeMensual(porcentajeMensual);
        ContratoRules.ValidarComisionRetiro(comisionRetiro);
        ContratoRules.ValidarModalidadRendimiento(modalidad);

        return new Contrato
        {
            ClienteId = clienteId,
            NumeroContrato = numeroContrato,
            FechaInicio = fechaInicio,
            _capitalInicial = capital,
            _capitalActual = capital,
            _porcentajeMensual = porcentajeMensual,
            _comisionRetiro = comisionRetiro,
            ModalidadRendimiento = modalidad,
            PermiteUnificacion = permiteUnificacion,
            _estado = EstadoContrato.Activo,
            FechaCreacion = DateTime.UtcNow
        };
    }

    #endregion

    #region Métodos de Dominio

    /// <summary>
    /// Inyectar capital adicional al contrato (cierra actual, crea relación).
    /// El capital actual se mantiene, y se registra la inyección.
    /// </summary>
    public void InyectarCapital(Capital capitalAInyectar, bool aprobacionGerencial = false)
    {
        ContratoRules.ValidarEstadoParaCierre(_estado);
        ContratoRules.ValidarCapitalInyectado(capitalAInyectar);
        // Validar ventana de pago (sin aprobación gerencial)
        VentanaPagoRules.ValidarOperacionEnVentana(DateTime.UtcNow, FechaInicio, _periodoPago, aprobacionGerencial);
        _capitalActual = _capitalActual.Sumar(capitalAInyectar);
    }

    /// <summary>
    /// Reinvertir ganancias sin cambiar estado (solo actualiza capital).
    /// </summary>
    public void Reinvertir(Capital ganancias, decimal? nuevoPorcentaje = null, bool aprobacionGerencial = false)
    {
        ContratoRules.ValidarEstadoParaCierre(_estado);
        // Validar ventana de pago (sin aprobación gerencial)
        VentanaPagoRules.ValidarOperacionEnVentana(DateTime.UtcNow, FechaInicio, _periodoPago, aprobacionGerencial);

        _capitalActual = _capitalActual.Sumar(ganancias);

        if (nuevoPorcentaje.HasValue)
        {
            ReinversionRules.ValidarNuevoPorcentajeMensual(nuevoPorcentaje.Value);
            _porcentajeMensual = nuevoPorcentaje.Value;
        }
    }

    /// <summary>
    /// Cambiar el porcentaje mensual (reinversión con cambio).
    /// </summary>
    public void CambiarPorcentajeMensual(decimal nuevoPorcentaje, bool aprobacionGerencial = false)
    {
        ContratoRules.ValidarEstadoParaCierre(_estado);
        // Validar ventana de pago (sin aprobación gerencial)
        VentanaPagoRules.ValidarOperacionEnVentana(DateTime.UtcNow, FechaInicio, _periodoPago, aprobacionGerencial);
        ReinversionRules.ValidarNuevoPorcentajeMensual(nuevoPorcentaje);
        _porcentajeMensual = nuevoPorcentaje;
    }

    /// <summary>
    /// Cerrar/Finalizar el contrato.
    /// </summary>
    public void Finalizar()
    {
        if (_estado != EstadoContrato.Activo)
            throw new OperacionNoPermitidaException("Finalización", _estado.ToString());
        
        _estado = EstadoContrato.Finalizado;
        FechaCierre = DateTime.UtcNow;
    }

    /// <summary>
    /// Marcar el contrato como unificado.
    /// </summary>
    public void MarcarComoUnificado(bool aprobacionGerencial = false)
    {
        ContratoRules.ValidarEstadoParaUnificacion(_estado);
        if (!PermiteUnificacion)
            throw ReglaNegocioException.UnificacionNoPermitida();


        // Validar ventana de pago (puede venir con aprobación gerencial)
        VentanaPagoRules.ValidarOperacionEnVentana(DateTime.UtcNow, FechaInicio, _periodoPago, aprobacionGerencial);

        _estado = EstadoContrato.Unificado;
        FechaCierre = DateTime.UtcNow;
    }

    /// <summary>
    /// Validar que el contrato puede recibir beneficiarios.
    /// </summary>
    public void ValidarPuedeTenerBeneficiarios()
    {
        if (_estado != EstadoContrato.Activo)
            throw ReglaNegocioException.EstadoContratoInvalido(_estado.ToString());
    }

    /// <summary>
    /// Validar que se puede hacer una operación sobre este contrato.
    /// </summary>
    public void ValidarEstaActivo()
    {
        if (_estado != EstadoContrato.Activo)
            throw ReglaNegocioException.ContratoNoActivo();
    }

    #endregion

    #region Helpers

    private static OperacionNoPermitidaException OperacionNoPermitida(string operacion, string estado)
        => new(operacion, estado);

    #endregion
}