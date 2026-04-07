namespace UniversalCalculatorLab4.Core;

public sealed class PNumberEditor : AEditor
{
    private int numberBase;

    public PNumberEditor(int numberBase = 10)
    {
        NumberBase = numberBase;
    }

    public int NumberBase
    {
        get => numberBase;
        private set
        {
            if (value is < 2 or > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Основание должно быть в диапазоне 2..16.");
            }

            numberBase = value;
        }
    }

    public void SetBase(int numberBase)
    {
        NumberBase = numberBase;
        Clear();
    }

    public override string AddDigit(int digit)
    {
        if (digit < 0 || digit >= NumberBase)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), $"Цифра должна быть в диапазоне 0..{NumberBase - 1}.");
        }

        var ch = digit < 10
            ? ((char)('0' + digit)).ToString()
            : ((char)('A' + (digit - 10))).ToString();

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
        if (!value.Contains('.', StringComparison.Ordinal))
        {
            value += ".";
        }

        return value;
    }
}

