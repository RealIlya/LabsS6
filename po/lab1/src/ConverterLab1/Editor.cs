namespace ConverterLab1;

public class Editor
{
    private string number = "0";
    private const string delim = ".";
    private const string zero = "0";

    public string Number
    {
        get { return number; }
    }

    public string AddDigit(int n)
    {
        if (n < 0 || n > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Цифра должна быть в диапазоне 0..15.");
        }

        var ch = Conver_10_P.int_to_Char(n).ToString();
        if (number == zero)
        {
            number = ch;
        }
        else
        {
            number += ch;
        }

        return number;
    }

    public int Acc()
    {
        var index = number.IndexOf(delim, StringComparison.Ordinal);
        if (index < 0)
        {
            return 0;
        }

        return number.Length - index - 1;
    }

    public string AddZero()
    {
        if (number != zero)
        {
            number += zero;
        }

        return number;
    }

    public string AddDelim()
    {
        if (number.Contains(delim, StringComparison.Ordinal))
        {
            return number;
        }

        number += delim;
        return number;
    }

    public string Bs()
    {
        if (number.Length <= 1)
        {
            number = zero;
            return number;
        }

        number = number[..^1];
        if (number == "-" || number.Length == 0)
        {
            number = zero;
        }

        return number;
    }

    public string Clear()
    {
        number = zero;
        return number;
    }

    public string DoEdit(int j)
    {
        if (j < 0)
        {
            return number;
        }

        if (j == 0)
        {
            return AddZero();
        }

        if (j >= 1 && j <= 15)
        {
            return AddDigit(j);
        }

        if (j == 16)
        {
            return AddDelim();
        }

        if (j == 17)
        {
            return Bs();
        }

        if (j == 18)
        {
            return Clear();
        }

        return number;
    }
}
