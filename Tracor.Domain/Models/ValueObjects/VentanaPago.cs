using System;

namespace Tradecorp.Domain.Models.ValueObjects
{
    /// <summary>
    /// Value Object inmutable que representa una ventana válida de pago.
    /// </summary>
    public sealed class VentanaPago
    {
        public DateOnly FechaInicio { get; }
        public DateOnly FechaFin { get; }

        public VentanaPago(DateOnly fechaInicio, DateOnly fechaFin)
        {
            if (fechaFin < fechaInicio) throw new ArgumentException("FechaFin debe ser mayor o igual a FechaInicio");
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
        }

        public bool Incluye(DateTime fechaUtc)
        {
            var fecha = DateOnly.FromDateTime(fechaUtc.Date);
            return fecha >= FechaInicio && fecha <= FechaFin;
        }

        public int DiasHabilesDuracion()
        {
            int dias = 0;
            var cursor = FechaInicio;

            while (cursor <= FechaFin)
            {
                if (cursor.DayOfWeek != DayOfWeek.Saturday && cursor.DayOfWeek != DayOfWeek.Sunday)
                {
                    dias++;
                }

                cursor = cursor.AddDays(1);
            }

            return dias;
        }
    }
}
