using System.Text;

namespace CalculatorPart1Lab3.Core;

public sealed class TPNumber : ICalcNumber<TPNumber>
{
    public TPNumber(double value = 0, int numberBase = 10, int precision = 10)
    {
        NumberBase = CheckBase(numberBase);
        Precision = CheckPrecision(precision);
        Value = value;
    }

    public TPNumber(string text, int numberBase = 10, int precision = 10)
    {
        NumberBase = CheckBase(numberBase);
        Precision = CheckPrecision(precision);
        Value = Parse(text, NumberBase);
    }

    public double Value { get; }
    public int NumberBase { get; }
    public int Precision { get; }

    public bool IsZero()
    {
        return Math.Abs(Value) < 1e-12;
    }

    public TPNumber Copy()
    {
        return new TPNumber(Value, NumberBase, Precision);
    }

    public TPNumber Add(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        return new TPNumber(Value + checkedOther.Value, NumberBase, Precision);
    }

    public TPNumber Sub(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        return new TPNumber(Value - checkedOther.Value, NumberBase, Precision);
    }

    public TPNumber Mul(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        return new TPNumber(Value * checkedOther.Value, NumberBase, Precision);
    }

    public TPNumber Div(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        if (checkedOther.IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TPNumber(Value / checkedOther.Value, NumberBase, Precision);
    }

    public bool EqualsTo(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        return Math.Abs(Value - checkedOther.Value) < 1e-12;
    }

    public TPNumber Sqr()
    {
        return new TPNumber(Value * Value, NumberBase, Precision);
    }

    public TPNumber Rev()
    {
        if (IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TPNumber(1.0 / Value, NumberBase, Precision);
    }

    public TPNumber Negate()
    {
        return new TPNumber(-Value, NumberBase, Precision);
    }

    public TPNumber WithBase(int numberBase)
    {
        return new TPNumber(Value, numberBase, Precision);
    }

    public override string ToString()
    {
        var sign = Value < 0 ? "-" : string.Empty;
        var abs = Math.Abs(Value);
        var intPart = (long)Math.Floor(abs);
        var fracPart = abs - intPart;

        var intText = IntToBase(intPart, NumberBase);
        var fracText = FracToBase(fracPart, NumberBase, Precision);

        if (fracText.Length == 0)
        {
            return sign + intText;
        }

        return sign + intText + "." + fracText;
    }

    public static double Parse(string text, int numberBase)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Входная строка пуста.", nameof(text));
        }

        CheckBase(numberBase);

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
            var digit = CharToDigit(ch);
            if (digit >= numberBase)
            {
                throw new FormatException($"Цифра '{ch}' недопустима для основания {numberBase}.");
            }

            intValue = intValue * numberBase + digit;
        }

        var fracValue = 0.0;
        var weight = 1.0 / numberBase;
        foreach (var ch in fracPart)
        {
            var digit = CharToDigit(ch);
            if (digit >= numberBase)
            {
                throw new FormatException($"Цифра '{ch}' недопустима для основания {numberBase}.");
            }

            fracValue += digit * weight;
            weight /= numberBase;
        }

        return sign * (intValue + fracValue);
    }

    private static int CheckBase(int numberBase)
    {
        if (numberBase < 2 || numberBase > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(numberBase), "Основание должно быть в диапазоне 2..16.");
        }

        return numberBase;
    }

    private static int CheckPrecision(int precision)
    {
        if (precision < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), "Точность должна быть неотрицательной.");
        }

        return precision;
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
        if (precision == 0 || frac == 0)
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

    private TPNumber EnsureCompatible(TPNumber other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other), "Сравниваемое число не должно быть null.");
        }

        if (NumberBase != other.NumberBase || Precision != other.Precision)
        {
            throw new InvalidOperationException("Числа имеют разные основание или точность.");
        }

        return other;
    }
}

