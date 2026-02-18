namespace ConverterLab1;

public static class Conver_P_10
{
    public static double char_To_num(char ch)
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

        throw new ArgumentException("Недопустимый символ цифры.", nameof(ch));
    }

    private static double convert(string pNum, int p, double weight)
    {
        var sum = 0.0;

        foreach (var ch in pNum)
        {
            var value = char_To_num(ch);
            if (value >= p)
            {
                throw new ArgumentException($"Цифра {ch} недопустима для основания {p}.");
            }

            sum += value * weight;
            weight /= p;
        }

        return sum;
    }

    public static double dval(string pNum, int p)
    {
        if (string.IsNullOrWhiteSpace(pNum))
        {
            throw new ArgumentException("Строка числа не должна быть пустой.", nameof(pNum));
        }

        if (p < 2 || p > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(p), "Основание должно быть в диапазоне 2..16.");
        }

        var text = pNum.Trim();
        var sign = 1.0;

        if (text.StartsWith("-", StringComparison.Ordinal))
        {
            sign = -1.0;
            text = text[1..];
        }

        if (text.Length == 0)
        {
            throw new ArgumentException("Некорректная запись числа.", nameof(pNum));
        }

        var parts = text.Split('.');
        if (parts.Length > 2)
        {
            throw new ArgumentException("Некорректный разделитель дробной части.", nameof(pNum));
        }

        var intPart = parts[0];
        var fracPart = parts.Length == 2 ? parts[1] : string.Empty;

        var intValue = 0.0;
        if (intPart.Length > 0)
        {
            intValue = convert(intPart, p, Math.Pow(p, intPart.Length - 1));
        }

        var fracValue = 0.0;
        if (fracPart.Length > 0)
        {
            fracValue = convert(fracPart, p, 1.0 / p);
        }

        return sign * (intValue + fracValue);
    }
}
