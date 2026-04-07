namespace UniversalCalculatorLab4.Core;

public sealed class FractionEditor : AEditor
{
    public override string AddDigit(int digit)
    {
        if (digit < 0 || digit > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), "Цифры дробного редактора должны быть в диапазоне 0..9.");
        }

        var ch = ((char)('0' + digit)).ToString();

        if (value == "0")
        {
            value = ch;
        }
        else if (value == "-0")
        {
            value = "-" + ch;
        }
        else
        {
            value += ch;
        }

        return value;
    }

    public override string AddZero()
    {
        if (!IsZero())
        {
            value += "0";
        }

        return value;
    }

    public override string AddSeparator()
    {
        if (!value.Contains('/', StringComparison.Ordinal))
        {
            value += "/";
        }

        return value;
    }
}

