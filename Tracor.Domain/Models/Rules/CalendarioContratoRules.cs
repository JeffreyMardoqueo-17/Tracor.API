using System;
using System.Collections.Generic;
using Tradecorp.Domain.Models.ValueObjects;

namespace Tradecorp.Domain.Models.Rules
{
    /// <summary>
    /// Reglas puras para calcular cortes y ventanas de pago.
    /// </summary>
    public static class CalendarioContratoRules
    {
        public static IEnumerable<VentanaPago> ObtenerVentanasDesde(DateOnly fechaCreacion, PeriodoPago periodo)
        {
            var ventanas = new List<VentanaPago>();

            for (int meses = periodo.PeriodoMeses; meses <= periodo.DuracionMeses; meses += periodo.PeriodoMeses)
            {
                var fechaCorte = fechaCreacion.AddMonths(meses);
                var inicioVentana = SiguienteDiaHabil(fechaCorte);
                var finVentana = SumarDiasHabiles(inicioVentana, periodo.VentanaDiasHabiles - 1);
                ventanas.Add(new VentanaPago(inicioVentana, finVentana));
            }

            return ventanas;
        }

        public static DateOnly CalcularProximaFechaPago(DateOnly fechaCreacion, DateTime ahoraUtc, PeriodoPago periodo)
        {
            var fechaReferencia = DateOnly.FromDateTime(ahoraUtc.Date);

            for (int meses = periodo.PeriodoMeses; meses <= periodo.DuracionMeses; meses += periodo.PeriodoMeses)
            {
                var fechaCorte = fechaCreacion.AddMonths(meses);
                if (fechaCorte >= fechaReferencia)
                {
                    return fechaCorte;
                }
            }

            return fechaCreacion.AddMonths(periodo.DuracionMeses);
        }

        private static DateOnly SiguienteDiaHabil(DateOnly fecha)
        {
            var cursor = fecha;
            while (cursor.DayOfWeek == DayOfWeek.Saturday || cursor.DayOfWeek == DayOfWeek.Sunday)
            {
                cursor = cursor.AddDays(1);
            }

            return cursor;
        }

        private static DateOnly SumarDiasHabiles(DateOnly inicio, int diasHabiles)
        {
            var cursor = inicio;
            var acumulados = 0;

            while (acumulados < diasHabiles)
            {
                cursor = cursor.AddDays(1);
                if (cursor.DayOfWeek != DayOfWeek.Saturday && cursor.DayOfWeek != DayOfWeek.Sunday)
                {
                    acumulados++;
                }
            }

            return cursor;
        }
    }
}
