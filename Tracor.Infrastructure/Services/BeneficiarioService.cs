using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using Tradecorp.Application.DTOs;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Services;

/// <summary>
/// Servicio de negocios para Beneficiarios
/// Regla de negocio principal: Beneficiarios activos deben sumar exactamente 100%
/// Principio SOLID: Single Responsibility - solo maneja lógica de beneficiarios
/// </summary>
public class BeneficiarioService : IBeneficiarioService
{
    private readonly IClienteBeneficiarioRepository _beneficiarioRepository;
    private readonly IClienteRepository _clienteRepository;

    public BeneficiarioService(
        IClienteBeneficiarioRepository beneficiarioRepository,
        IClienteRepository clienteRepository)
    {
        _beneficiarioRepository = beneficiarioRepository ?? throw new ArgumentNullException(nameof(beneficiarioRepository));
        _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
    }

    public async Task<ClienteBeneficiarioResponse> CrearBeneficiarioAsync(int clienteId, CreateClienteBeneficiarioRequest request)
    {
        // Validar cliente existe
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null)
            throw new InvalidOperationException($"Cliente con ID {clienteId} no encontrado.");

        // Validar porcentaje
        if (request.Porcentaje <= 0 || request.Porcentaje > 100)
            throw new ArgumentException("El porcentaje debe estar entre 0 y 100.");

        // Validar que no se exceda el 100%
        var sumaActual = await _beneficiarioRepository.GetSumaPorcentajeActivosAsync(clienteId);
        if (sumaActual + request.Porcentaje > 100)
            throw new InvalidOperationException(
                $"No se puede agregar este beneficiario. Porcentaje disponible: {100 - sumaActual}%");

        // Crear beneficiario
        var beneficiario = new ClienteBeneficiario
        {
            ClienteId = clienteId,
            NombreCompleto = request.NombreCompleto.Trim(),
            DUI = request.DUI?.Trim(),
            Correo = request.Correo?.Trim(),
            Telefono = request.Telefono?.Trim(),
            Direccion = request.Direccion?.Trim(),
            Porcentaje = request.Porcentaje,
            TipoRelacion = Enum.Parse<ClienteBeneficiarioTipo>(request.TipoRelacion),
            Estado = ClienteBeneficiarioEstado.Activo,
            Notas = request.Notas?.Trim(),
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        var beneficiarioCreado = await _beneficiarioRepository.CreateAsync(beneficiario);
        return MapearBeneficiarioAResponse(beneficiarioCreado);
    }

    public async Task<ClienteBeneficiarioResponse?> ObtenerBeneficiarioAsync(int beneficiarioId)
    {
        var beneficiario = await _beneficiarioRepository.GetByIdAsync(beneficiarioId);
        if (beneficiario == null)
            return null;

        return MapearBeneficiarioAResponse(beneficiario);
    }

    public async Task<IEnumerable<ClienteBeneficiarioResponse>> ObtenerBeneficiariosClienteAsync(int clienteId)
    {
        var beneficiarios = await _beneficiarioRepository.GetByClienteIdAsync(clienteId);
        return beneficiarios.Select(MapearBeneficiarioAResponse).ToList();
    }

    public async Task<IEnumerable<ClienteBeneficiarioResponse>> ObtenerBeneficiariosActivosAsync(int clienteId)
    {
        var beneficiarios = await _beneficiarioRepository.GetActivosByClienteIdAsync(clienteId);
        return beneficiarios.Select(MapearBeneficiarioAResponse).ToList();
    }

    public async Task<ClienteBeneficiarioResponse> ActualizarBeneficiarioAsync(int beneficiarioId, UpdateClienteBeneficiarioRequest request)
    {
        var beneficiario = await _beneficiarioRepository.GetByIdAsync(beneficiarioId);
        if (beneficiario == null)
            throw new InvalidOperationException($"Beneficiario con ID {beneficiarioId} no encontrado.");

        var porcentajeAnterior = beneficiario.Porcentaje;

        // Actualizar campos
        if (!string.IsNullOrWhiteSpace(request.NombreCompleto))
            beneficiario.NombreCompleto = request.NombreCompleto.Trim();

        if (!string.IsNullOrEmpty(request.DUI))
            beneficiario.DUI = request.DUI.Trim();

        if (!string.IsNullOrEmpty(request.Correo))
            beneficiario.Correo = request.Correo.Trim();

        if (!string.IsNullOrEmpty(request.Telefono))
            beneficiario.Telefono = request.Telefono.Trim();

        if (!string.IsNullOrEmpty(request.Direccion))
            beneficiario.Direccion = request.Direccion.Trim();

        if (request.Porcentaje.HasValue)
        {
            // Validar nuevo porcentaje
            if (request.Porcentaje <= 0 || request.Porcentaje > 100)
                throw new ArgumentException("El porcentaje debe estar entre 0 y 100.");

            // Validar que no se exceda 100% si cambia el porcentaje
            if (request.Porcentaje != porcentajeAnterior)
            {
                var sumaOtros = await _beneficiarioRepository.GetSumaPorcentajeActivosAsync(beneficiario.ClienteId);
                sumaOtros -= porcentajeAnterior; // Restar el porcentaje anterior

                if (sumaOtros + request.Porcentaje > 100)
                    throw new InvalidOperationException(
                        $"No se puede asignar este porcentaje. Máximo disponible: {100 - sumaOtros}%");

                beneficiario.Porcentaje = request.Porcentaje.Value;
            }
        }

        if (!string.IsNullOrEmpty(request.TipoRelacion))
        {
            beneficiario.TipoRelacion = Enum.Parse<ClienteBeneficiarioTipo>(request.TipoRelacion);
        }

        if (!string.IsNullOrEmpty(request.Notas))
            beneficiario.Notas = request.Notas.Trim();

        beneficiario.FechaActualizacion = DateTime.UtcNow;

        var beneficiarioActualizado = await _beneficiarioRepository.UpdateAsync(beneficiario);
        return MapearBeneficiarioAResponse(beneficiarioActualizado);
    }

    public async Task<ClienteBeneficiarioValidacionResponse> ValidarBeneficiariosAsync(int clienteId)
    {
        var suma = await _beneficiarioRepository.GetSumaPorcentajeActivosAsync(clienteId);
        var cantidad = await _beneficiarioRepository.GetCantidadActivosAsync(clienteId);

        var respuesta = new ClienteBeneficiarioValidacionResponse
        {
            BeneficiariosCompletos = suma == 100m,
            SumaPorcentaje = suma,
            CantidadBeneficiariosActivos = cantidad
        };

        // Generar mensaje
        if (respuesta.BeneficiariosCompletos)
        {
            respuesta.Mensaje = "✅ Los beneficiarios están completos y suman exactamente 100%.";
        }
        else if (suma < 100m)
        {
            respuesta.Mensaje = $"⚠️ Asignación incompleta: {suma}% de 100%. Falta asignar {100 - suma}%.";
        }
        else
        {
            respuesta.Mensaje = $"❌ Error: Los beneficiarios suman {suma}% en lugar de 100%.";
        }

        return respuesta;
    }

    public async Task<bool> RegistrarFallecimientoBeneficiarioAsync(int beneficiarioId, RegistrarFallecimientoBeneficiarioRequest request)
    {
        if (request.FechaFallecimiento > DateTime.UtcNow)
            throw new ArgumentException("La fecha de fallecimiento no puede ser en el futuro.");

        return await _beneficiarioRepository.RegistrarFallecimientoAsync(
            beneficiarioId,
            request.Notas);
    }

    public async Task<bool> DesactivarBeneficiarioAsync(int beneficiarioId, string? razon)
    {
        return await _beneficiarioRepository.DeactivateAsync(beneficiarioId, razon);
    }

    public async Task<IEnumerable<ClienteBeneficiarioHistoricoResponse>> ObtenerHistoricoBeneficiarioAsync(int beneficiarioId)
    {
        var historico = await _beneficiarioRepository.GetHistoricoByBeneficiarioIdAsync(beneficiarioId);
        return historico.Select(h => new ClienteBeneficiarioHistoricoResponse
        {
            Id = h.Id,
            NombreCompleto = h.NombreCompleto,
            DUI = h.DUI,
            PorcentajeAsignado = h.PorcentajeAsignado,
            TipoRelacion = h.TipoRelacion.ToString(),
            Evento = h.Evento,
            FechaEvento = h.FechaEvento,
            Notas = h.Notas
        }).ToList();
    }

    public async Task<IEnumerable<ClienteBeneficiarioHistoricoResponse>> ObtenerHistoricoClienteAsync(int clienteId)
    {
        var historico = await _beneficiarioRepository.GetHistoricoByClienteIdAsync(clienteId);
        return historico.Select(h => new ClienteBeneficiarioHistoricoResponse
        {
            Id = h.Id,
            NombreCompleto = h.NombreCompleto,
            DUI = h.DUI,
            PorcentajeAsignado = h.PorcentajeAsignado,
            TipoRelacion = h.TipoRelacion.ToString(),
            Evento = h.Evento,
            FechaEvento = h.FechaEvento,
            Notas = h.Notas
        }).ToList();
    }

    public async Task<decimal> ObtenerPorcentajeDisponibleAsync(int clienteId)
    {
        var suma = await _beneficiarioRepository.GetSumaPorcentajeActivosAsync(clienteId);
        return 100 - suma;
    }

    // Métodos auxiliares privados

    private ClienteBeneficiarioResponse MapearBeneficiarioAResponse(ClienteBeneficiario beneficiario)
    {
        return new ClienteBeneficiarioResponse
        {
            Id = beneficiario.Id,
            NombreCompleto = beneficiario.NombreCompleto,
            DUI = beneficiario.DUI,
            Correo = beneficiario.Correo,
            Telefono = beneficiario.Telefono,
            Direccion = beneficiario.Direccion,
            Porcentaje = beneficiario.Porcentaje,
            TipoRelacion = beneficiario.TipoRelacion.ToString(),
            Estado = beneficiario.Estado.ToString(),
            Notas = beneficiario.Notas,
            FechaCreacion = beneficiario.FechaCreacion,
            FechaActualizacion = beneficiario.FechaActualizacion
        };
    }
}
