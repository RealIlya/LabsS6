namespace UniversalCalculatorLab4.Core;

public sealed class TMemory
{
    private TANumber number;

    public TMemory(TANumber defaultNumber)
    {
        if (defaultNumber is null)
        {
            throw new ArgumentNullException(nameof(defaultNumber), "Значение по умолчанию не должно быть null.");
        }

        number = defaultNumber.Copy();
    }

    public bool IsOn { get; private set; }

    public void Store(TANumber value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Сохраняемое значение не должно быть null.");
        }

        number = value.Copy();
        IsOn = true;
    }

    public TANumber Read()
    {
        return number.Copy();
    }

    public void Add(TANumber value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Добавляемое значение не должно быть null.");
        }

        number = number.Add(value);
        IsOn = true;
    }

    public void Clear(TANumber defaultNumber)
    {
        if (defaultNumber is null)
        {
            throw new ArgumentNullException(nameof(defaultNumber), "Нулевое значение не должно быть null.");
        }

        number = defaultNumber.Copy();
        IsOn = false;
    }
}
