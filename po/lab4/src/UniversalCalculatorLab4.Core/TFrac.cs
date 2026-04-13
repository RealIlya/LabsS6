namespace UniversalCalculatorLab4.Core;

public sealed class TFrac : TANumber
{
    public TFrac(long numerator = 0, long denominator = 1)
    {
        if (denominator == 0)
        {
            throw new DivideByZeroException("Знаменатель не должен быть равен нулю.");
        }

        var sign = denominator < 0 ? -1 : 1;
        var n = numerator * sign;
        var d = Math.Abs(denominator);
        var gcd = Gcd(Math.Abs(n), d);
        Numerator = n / gcd;
        Denominator = d / gcd;
    }

    public TFrac(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Входная строка пуста.", nameof(text));
        }

        var parts = text.Trim().Split('/');
        if (parts.Length != 2)
        {
            throw new FormatException("Дробь должна быть в формате a/b.");
        }

        var n = long.Parse(parts[0]);
        var d = long.Parse(parts[1]);
        if (d == 0)
        {
            throw new DivideByZeroException("Знаменатель не должен быть равен нулю.");
        }

        var normalized = new TFrac(n, d);
        Numerator = normalized.Numerator;
        Denominator = normalized.Denominator;
    }

    public long Numerator { get; }
    public long Denominator { get; }

    public override bool IsZero() => Numerator == 0;
    public override TANumber Copy() => new TFrac(Numerator, Denominator);
    public override TANumber Negate() => new TFrac(-Numerator, Denominator);
    public override TANumber Sqr() => new TFrac(Numerator * Numerator, Denominator * Denominator);

    public override TANumber Rev()
    {
        if (IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TFrac(Denominator, Numerator);
    }

    public override TANumber Add(TANumber other)
    {
        var b = CheckType(other);
        var n = Numerator * b.Denominator + b.Numerator * Denominator;
        var d = Denominator * b.Denominator;
        return new TFrac(n, d);
    }

    public override TANumber Sub(TANumber other)
    {
        var b = CheckType(other);
        var n = Numerator * b.Denominator - b.Numerator * Denominator;
        var d = Denominator * b.Denominator;
        return new TFrac(n, d);
    }

    public override TANumber Mul(TANumber other)
    {
        var b = CheckType(other);
        return new TFrac(Numerator * b.Numerator, Denominator * b.Denominator);
    }

    public override TANumber Div(TANumber other)
    {
        var b = CheckType(other);
        if (b.IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TFrac(Numerator * b.Denominator, Denominator * b.Numerator);
    }

    public override bool EqualsTo(TANumber other)
    {
        var b = CheckType(other);
        return Numerator == b.Numerator && Denominator == b.Denominator;
    }

    public override string ToString()
    {
        return $"{Numerator}/{Denominator}";
    }

    private TFrac CheckType(TANumber other)
    {
        if (other is not TFrac value)
        {
            throw new InvalidOperationException("Разные типы чисел.");
        }

        return value;
    }

    private static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            var t = a % b;
            a = b;
            b = t;
        }

        return a == 0 ? 1 : a;
    }
}

