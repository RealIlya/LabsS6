namespace UniversalCalculatorLab4.Core;

public abstract class AEditor
{
    protected string value = "0";

    public string Value => value;

    public virtual bool IsZero() => value is "0" or "-0";

    public virtual string Clear()
    {
        value = "0";
        return value;
    }

    public virtual string Backspace()
    {
        if (value.Length <= 1)
        {
            return Clear();
        }

        value = value[..^1];
        if (value is "-" or "")
        {
            value = "0";
        }

        return value;
    }

    public virtual string ToggleSign()
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

    public virtual void SetValue(string text)
    {
        value = string.IsNullOrWhiteSpace(text) ? "0" : text;
    }

    public abstract string AddDigit(int digit);
    public abstract string AddZero();
    public abstract string AddSeparator();

    public virtual string Edit(int command)
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
