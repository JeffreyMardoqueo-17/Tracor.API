namespace Tradecorp.Domain.Models.ValueObjects;

/// <summary>
/// Value Object que representa un capital (monto de dinero) con validaciones.
/// Garantiza que los capitales siempre sean positivos o cero.
/// </summary>
public class Capital : IEquatable<Capital>
{
    private readonly decimal _valor;

    public decimal Valor => _valor;

    public Capital(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("El capital no puede ser negativo.", nameof(valor));

        _valor = valor;
    }

    public static Capital Cero => new Capital(0);

    public Capital Sumar(Capital otro)
    {
        return new Capital(Valor + otro.Valor);
    }

    public Capital Restar(Capital otro)
    {
        if (otro.Valor > Valor)
            throw new InvalidOperationException("No se puede restar un capital mayor al actual.");
        
        return new Capital(Valor - otro.Valor);
    }

    public Capital Multiplicar(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100)
            throw new ArgumentException("El porcentaje debe estar entre 0 y 100.", nameof(porcentaje));

        return new Capital(Valor * (porcentaje / 100m));
    }

    public override bool Equals(object? obj) => Equals(obj as Capital);

    public bool Equals(Capital? otro)
    {
        return otro is not null && Valor == otro.Valor;
    }

    public override int GetHashCode() => Valor.GetHashCode();

    public override string ToString() => $"${Valor:F2}";

    public static bool operator ==(Capital? izq, Capital? der)
    {
        if (izq is null) return der is null;
        return izq.Equals(der);
    }

    public static bool operator !=(Capital? izq, Capital? der) => !(izq == der);

    public static bool operator <(Capital? izq, Capital? der)
    {
        if (izq is null || der is null) throw new ArgumentNullException();
        return izq.Valor < der.Valor;
    }

    public static bool operator <=(Capital? izq, Capital? der)
    {
        if (izq is null || der is null) throw new ArgumentNullException();
        return izq.Valor <= der.Valor;
    }

    public static bool operator >(Capital? izq, Capital? der)
    {
        if (izq is null || der is null) throw new ArgumentNullException();
        return izq.Valor > der.Valor;
    }

    public static bool operator >=(Capital? izq, Capital? der)
    {
        if (izq is null || der is null) throw new ArgumentNullException();
        return izq.Valor >= der.Valor;
    }
}

/// <summary>
/// Value Object que representa un porcentaje (0-100).
/// </summary>
public class Porcentaje : IEquatable<Porcentaje>
{
    private readonly decimal _valor;

    public decimal Valor => _valor;

    public Porcentaje(decimal valor)
    {
        if (valor < 0 || valor > 100)
            throw new ArgumentException("El porcentaje debe estar entre 0 y 100.", nameof(valor));

        _valor = valor;
    }

    public static Porcentaje Cero => new Porcentaje(0);
    public static Porcentaje Completo => new Porcentaje(100);

    public Porcentaje Sumar(Porcentaje otro)
    {
        var suma = Valor + otro.Valor;
        if (suma > 100)
            throw new InvalidOperationException("La suma de porcentajes no puede exceder 100.");

        return new Porcentaje(suma);
    }

    public override bool Equals(object? obj) => Equals(obj as Porcentaje);

    public bool Equals(Porcentaje? otro)
    {
        return otro is not null && Valor == otro.Valor;
    }

    public override int GetHashCode() => Valor.GetHashCode();

    public override string ToString() => $"{Valor:F2}%";
}
