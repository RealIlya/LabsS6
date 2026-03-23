namespace CalculatorPart1Lab3.Core;

public sealed class TPNumberEditor
{
    private string value = "0";
    private int numberBase;

    public TPNumberEditor(int numberBase = 10)
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

    public string Value => value;

    public void SetBase(int numberBase)
    {
        NumberBase = numberBase;
        Clear();
    }

    public bool IsZero()
    {
        return value is "0" or "-0";
    }

    public string ToggleSign()
    {
        if (value.StartsWith("-", StringComparison.Ordinal))
        {
            value = value[1..];
            if (value.Length == 0)
            {
                value = "0";
            }

            return value;
        }

        value = "-" + value;
        return value;
    }

    public string AddDigit(int digit)
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

    public string AddZero()
    {
        if (!IsZero())
        {
            value += "0";
        }

        return value;
    }

    public string AddSeparator()
    {
        if (!value.Contains('.', StringComparison.Ordinal))
        {
            value += ".";
        }

        return value;
    }

    public string Backspace()
    {
        if (value.Length <= 1)
        {
            value = "0";
            return value;
        }

        value = value[..^1];
        if (value is "-" or "")
        {
            value = "0";
        }

        return value;
    }

    public string Clear()
    {
        value = "0";
        return value;
    }

    public string Edit(int command)
    {
        return command switch
        {
            0 => AddZero(),
            >= 1 and <= 15 => AddDigit(command),
            16 => AddSeparator(),
            17 => Backspace(),
            18 => Clear(),
            20 => ToggleSign(),
            _ => value
        };
    }
}

