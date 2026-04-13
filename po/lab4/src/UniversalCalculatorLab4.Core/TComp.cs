namespace UniversalCalculatorLab4.Core;

public sealed class TComp : TANumber
{
    public TComp(TPNumber realPart, TPNumber imaginaryPart)
    {
        Re = (TPNumber)(realPart ?? throw new ArgumentNullException(nameof(realPart), "Действительная часть не должна быть null.")).Copy();
        Im = (TPNumber)(imaginaryPart ?? throw new ArgumentNullException(nameof(imaginaryPart), "Мнимая часть не должна быть null.")).Copy();
        ValidateParameters(Re, Im);
    }

    public TComp(double realPart = 0, double imaginaryPart = 0, int numberBase = 10, int precision = 10)
        : this(
            new TPNumber(realPart, numberBase, precision),
            new TPNumber(imaginaryPart, numberBase, precision))
    {
    }

    public TComp(string text, int numberBase = 10, int precision = 10)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Входная строка пуста.", nameof(text));
        }

        var source = text.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (source.Contains(';', StringComparison.Ordinal))
        {
            var parts = source.Split(';');
            if (parts.Length != 2)
            {
                throw new FormatException("Некорректный формат комплексного числа.");
            }

            Re = new TPNumber(parts[0], numberBase, precision);
            Im = new TPNumber(parts[1], numberBase, precision);
            return;
        }

        var markerIndex = FindImaginaryMarker(source);
        if (markerIndex < 0)
        {
            throw new FormatException("Комплексное число должно быть в формате a;b, a+i*b или a-i*b.");
        }

        var realText = source[..markerIndex];
        var sign = source[markerIndex];
        var imaginaryMagnitude = source[(markerIndex + 3)..];
        if (realText.Length == 0 || imaginaryMagnitude.Length == 0)
        {
            throw new FormatException("Комплексное число должно быть в формате a;b, a+i*b или a-i*b.");
        }

        Re = new TPNumber(realText, numberBase, precision);
        Im = new TPNumber(
            sign == '-'
                ? "-" + imaginaryMagnitude
                : imaginaryMagnitude,
            numberBase,
            precision);
    }

    public TPNumber Re { get; }
    public TPNumber Im { get; }

    public override bool IsZero() => Re.IsZero() && Im.IsZero();

    public override TANumber Copy()
    {
        return new TComp(Re, Im);
    }

    public override TANumber Negate()
    {
        return new TComp(
            (TPNumber)Re.Negate(),
            (TPNumber)Im.Negate());
    }

    public override TANumber Add(TANumber other)
    {
        var value = CheckType(other);
        return new TComp(
            (TPNumber)Re.Add(value.Re),
            (TPNumber)Im.Add(value.Im));
    }

    public override TANumber Sub(TANumber other)
    {
        var value = CheckType(other);
        return new TComp(
            (TPNumber)Re.Sub(value.Re),
            (TPNumber)Im.Sub(value.Im));
    }

    public override TANumber Mul(TANumber other)
    {
        var value = CheckType(other);
        var realPart = Re.Value * value.Re.Value - Im.Value * value.Im.Value;
        var imaginaryPart = Re.Value * value.Im.Value + Im.Value * value.Re.Value;
        return new TComp(realPart, imaginaryPart, Re.NumberBase, Re.Precision);
    }

    public override TANumber Div(TANumber other)
    {
        var value = CheckType(other);
        var denominator = value.Re.Value * value.Re.Value + value.Im.Value * value.Im.Value;
        if (Math.Abs(denominator) < 1e-12)
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        var realPart = (Re.Value * value.Re.Value + Im.Value * value.Im.Value) / denominator;
        var imaginaryPart = (Im.Value * value.Re.Value - Re.Value * value.Im.Value) / denominator;
        return new TComp(realPart, imaginaryPart, Re.NumberBase, Re.Precision);
    }

    public override bool EqualsTo(TANumber other)
    {
        var value = CheckType(other);
        return Re.EqualsTo(value.Re) && Im.EqualsTo(value.Im);
    }

    public override TANumber Sqr()
    {
        return Mul(this);
    }

    public override TANumber Rev()
    {
        if (IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        var denominator = Re.Value * Re.Value + Im.Value * Im.Value;
        return new TComp(
            Re.Value / denominator,
            -Im.Value / denominator,
            Re.NumberBase,
            Re.Precision);
    }

    public override string ToString()
    {
        var sign = Im.Value < 0 ? "-" : "+";
        var imaginaryAbs = new TPNumber(Math.Abs(Im.Value), Im.NumberBase, Im.Precision);
        return $"{Re}{sign}i*{imaginaryAbs}";
    }

    private TComp CheckType(TANumber other)
    {
        if (other is not TComp value)
        {
            throw new InvalidOperationException("Разные типы чисел.");
        }

        ValidateParameters(Re, value.Re);
        ValidateParameters(Im, value.Im);
        return value;
    }

    private static int FindImaginaryMarker(string source)
    {
        for (var i = 1; i <= source.Length - 3; i++)
        {
            if ((source[i] == '+' || source[i] == '-')
                && source[i + 1] == 'i'
                && source[i + 2] == '*')
            {
                return i;
            }
        }

        return -1;
    }

    private static void ValidateParameters(TPNumber first, TPNumber second)
    {
        if (first.NumberBase != second.NumberBase || first.Precision != second.Precision)
        {
            throw new InvalidOperationException("Разные параметры p-чисел.");
        }
    }
}
