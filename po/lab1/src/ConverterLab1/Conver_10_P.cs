using System.Text;

namespace ConverterLab1;

public static class Conver_10_P
{
    public static char int_to_Char(int n)
    {
        if (n < 0 || n > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Цифра должна быть в диапазоне 0..15.");
        }

        return n < 10 ? (char)('0' + n) : (char)('A' + (n - 10));
    }

    public static string int_to_P(int n, int p)
    {
        CheckBase(p);

        if (n == 0)
        {
            return "0";
        }

        var value = Math.Abs((long)n);
        var sb = new StringBuilder();

        while (value > 0)
        {
            var digit = (int)(value % p);
            sb.Insert(0, int_to_Char(digit));
            value /= p;
        }

        if (n < 0)
        {
            sb.Insert(0, '-');
        }

        return sb.ToString();
    }

    public static string flt_to_P(double n, int p, int c)
    {
        CheckBase(p);

        if (c < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(c), "Точность должна быть неотрицательной.");
        }

        if (n < 0)
        {
            n = Math.Abs(n);
        }

        var sb = new StringBuilder();
        var frac = n;

        for (var i = 0; i < c; i++)
        {
            frac *= p;
            var digit = (int)Math.Floor(frac);
            sb.Append(int_to_Char(digit));
            frac -= digit;

            if (frac == 0)
            {
                break;
            }
        }

        return sb.ToString();
    }

    public static string Do(double n, int p, int c)
    {
        CheckBase(p);

        if (c < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(c), "Точность должна быть неотрицательной.");
        }

        var sign = n < 0 ? "-" : string.Empty;
        var abs = Math.Abs(n);

        var intPart = (int)Math.Floor(abs);
        var fracPart = abs - intPart;

        var intText = int_to_P(intPart, p);
        var fracText = c == 0 ? string.Empty : flt_to_P(fracPart, p, c);

        if (fracText.Length == 0 || fracPart == 0)
        {
            return sign + intText;
        }

        return sign + intText + "." + fracText;
    }

    private static void CheckBase(int p)
    {
        if (p < 2 || p > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(p), "Основание должно быть в диапазоне 2..16.");
        }
    }
}
