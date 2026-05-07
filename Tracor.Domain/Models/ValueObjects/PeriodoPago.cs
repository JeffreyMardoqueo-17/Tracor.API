using System;

namespace Tradecorp.Domain.Models.ValueObjects
{
    /// <summary>
    /// Value Object que define el ciclo de pago de un contrato.
    /// </summary>
    public sealed class PeriodoPago
    {
        public int DuracionMeses { get; }
        public int PeriodoMeses { get; }
        public int VentanaDiasHabiles { get; }

        public PeriodoPago(int duracionMeses = 24, int periodoMeses = 4, int ventanaDiasHabiles = 10)
        {
            if (duracionMeses <= 0) throw new ArgumentException("DuracionMeses debe ser mayor que 0", nameof(duracionMeses));
            if (periodoMeses <= 0) throw new ArgumentException("PeriodoMeses debe ser mayor que 0", nameof(periodoMeses));
            if (ventanaDiasHabiles <= 0) throw new ArgumentException("VentanaDiasHabiles debe ser mayor que 0", nameof(ventanaDiasHabiles));
            if (duracionMeses < periodoMeses) throw new ArgumentException("DuracionMeses debe ser >= PeriodoMeses");

            DuracionMeses = duracionMeses;
            PeriodoMeses = periodoMeses;
            VentanaDiasHabiles = ventanaDiasHabiles;
        }
    }
}
