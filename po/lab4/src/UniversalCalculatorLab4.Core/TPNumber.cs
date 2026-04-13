using System.Text;

namespace UniversalCalculatorLab4.Core;

public sealed class TPNumber : TANumber
{
    public TPNumber(double value = 0, int numberBase = 10, int precision = 10)
    {
        Value = value;
        NumberBase = ValidateBase(numberBase);
        Precision = ValidatePrecision(precision);
    }

    public TPNumber(string text, int numberBase = 10, int precision = 10)
    {
        NumberBase = ValidateBase(numberBase);
        Precision = ValidatePrecision(precision);
        Value = Parse(text, NumberBase);
    }

    public double Value { get; }
    public int NumberBase { get; }
    public int Precision { get; }

    public override bool IsZero() => Math.Abs(Value) < 1e-12;
    public override TANumber Copy() => new TPNumber(Value, NumberBase, Precision);
    public override TANumber Negate() => new TPNumber(-Value, NumberBase, Precision);
    public override TANumber Sqr() => new TPNumber(Value * Value, NumberBase, Precision);

    public override TANumber Rev()
    {
        if (IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TPNumber(1.0 / Value, NumberBase, Precision);
    }

    public override TANumber Add(TANumber other)
    {
        var b = CheckType(other);
        return new TPNumber(Value + b.Value, NumberBase, Precision);
    }

    public override TANumber Sub(TANumber other)
    {
        var b = CheckType(other);
        return new TPNumber(Value - b.Value, NumberBase, Precision);
    }

    public override TANumber Mul(TANumber other)
    {
        var b = CheckType(other);
        return new TPNumber(Value * b.Value, NumberBase, Precision);
    }

    public override TANumber Div(TANumber other)
    {
        var b = CheckType(other);
        if (b.IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TPNumber(Value / b.Value, NumberBase, Precision);
    }

    public override bool EqualsTo(TANumber other)
    {
        var b = CheckType(other);
        return Math.Abs(Value - b.Value) < 1e-12;
    }

    public override string ToString()
    {
        var sign = Value < 0 ? "-" : string.Empty;
        var abs = Math.Abs(Value);
        var intPart = (long)Math.Floor(abs);
        var fracPart = abs - intPart;
        var intText = IntToBase(intPart, NumberBase);
        var fracText = FracToBase(fracPart, NumberBase, Precision);
        return fracText.Length == 0 ? sign + intText : sign + intText + "." + fracText;
    }

    public static double Parse(string text, int numberBase)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Входная строка пуста.", nameof(text));
        }

        ValidateBase(numberBase);

        var source = text.Trim();
        var sign = 1.0;
        if (source.StartsWith("-", StringComparison.Ordinal))
        {
            sign = -1.0;
            source = source[1..];
        }

        var parts = source.Split('.');
        if (parts.Length > 2)
        {
            throw new FormatException("Некорректный формат числа.");
        }

        var intPart = parts[0];
        var fracPart = parts.Length == 2 ? parts[1] : string.Empty;

        var intValue = 0.0;
        foreach (var ch in intPart)
        {
            var d = CharToDigit(ch);
            if (d >= numberBase)
            {
                throw new FormatException($"Цифра '{ch}' недопустима для основания {numberBase}.");
            }

            intValue = intValue * numberBase + d;
        }

        var fracValue = 0.0;
        var w = 1.0 / numberBase;
        foreach (var ch in fracPart)
        {
            var d = CharToDigit(ch);
            if (d >= numberBase)
            {
                throw new FormatException($"Цифра '{ch}' недопустима для основания {numberBase}.");
            }

            fracValue += d * w;
            w /= numberBase;
        }

        return sign * (intValue + fracValue);
    }

    private static string IntToBase(long value, int numberBase)
    {
        if (value == 0)
        {
            return "0";
        }

        var builder = new StringBuilder();
        var current = value;
        while (current > 0)
        {
            var digit = (int)(current % numberBase);
            builder.Insert(0, DigitToChar(digit));
            current /= numberBase;
        }

        return builder.ToString();
    }

    private static string FracToBase(double frac, int numberBase, int precision)
    {
        if (frac == 0 || precision == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        var current = frac;
        for (var i = 0; i < precision; i++)
        {
            current *= numberBase;
            var digit = (int)Math.Floor(current);
            builder.Append(DigitToChar(digit));
            current -= digit;
            if (current == 0)
            {
                break;
            }
        }

        return builder.ToString();
    }

    private TPNumber CheckType(TANumber other)
    {
        if (other is not TPNumber n)
        {
            throw new InvalidOperationException("Разные типы чисел.");
        }

        if (n.NumberBase != NumberBase || n.Precision != Precision)
        {
            throw new InvalidOperationException("Разные параметры p-чисел.");
        }

        return n;
    }

    private static int ValidateBase(int numberBase)
    {
        if (numberBase < 2 || numberBase > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(numberBase), "Основание должно быть в диапазоне 2..16.");
        }

        return numberBase;
    }

    private static int ValidatePrecision(int precision)
    {
        if (precision < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), "Точность должна быть неотрицательной.");
        }

        return precision;
    }

    private static int CharToDigit(char ch)
    {
        if (ch >= '0' && ch <= '9')
        {
            return ch - '0';
        }

        if (ch >= 'A' && ch <= 'F')
        {
            return ch - 'A' + 10;
        }

        if (ch >= 'a' && ch <= 'f')
        {
            return ch - 'a' + 10;
        }

        throw new FormatException($"Недопустимая цифра '{ch}'.");
    }

    private static char DigitToChar(int digit)
    {
        return digit < 10 ? (char)('0' + digit) : (char)('A' + (digit - 10));
    }
}

