using System;
using Tradecorp.Domain.Models.ValueObjects;

namespace Tradecorp.Domain.Models.Rules
{
    /// <summary>
    /// Reglas de negocio para validar si una operación ocurre dentro de una ventana de pago.
    /// </summary>
    public static class VentanaPagoRules
    {
        public static VentanaPago? ObtenerVentanaActual(DateTime ahoraUtc, DateOnly fechaCreacion, PeriodoPago periodo)
        {
            var ventanas = CalendarioContratoRules.ObtenerVentanasDesde(fechaCreacion, periodo);
            foreach (var ventana in ventanas)
            {
                if (ventana.Incluye(ahoraUtc))
                {
                    return ventana;
                }
            }

            return null;
        }

        public static void ValidarOperacionEnVentana(DateTime ahoraUtc, DateOnly fechaCreacion, PeriodoPago periodo, bool aprobacionGerencial)
        {
            if (aprobacionGerencial)
            {
                return;
            }

            var ventanaActual = ObtenerVentanaActual(ahoraUtc, fechaCreacion, periodo);
            if (ventanaActual is null)
            {
                throw new Tradecorp.Domain.Exceptions.ReglaNegocioException("Operación no permitida fuera de ventana de pago");
            }
        }
    }
}
