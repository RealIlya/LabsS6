namespace CalculatorPart1Lab3.Core;

public sealed class TMemory<T> where T : ICalcNumber<T>
{
    public TMemory(T defaultValue)
    {
        if (defaultValue is null)
        {
            throw new ArgumentNullException(nameof(defaultValue), "Значение по умолчанию не должно быть null.");
        }

        number = defaultValue.Copy();
    }

    private T number;

    public bool IsOn { get; private set; }

    public void Store(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Сохраняемое значение не должно быть null.");
        }

        number = value.Copy();
        IsOn = true;
    }

    public void Add(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Добавляемое значение не должно быть null.");
        }

        number = number.Add(value);
        IsOn = !number.IsZero();
    }

    public T Read()
    {
        return number.Copy();
    }

    public void Clear(T zeroValue)
    {
        if (zeroValue is null)
        {
            throw new ArgumentNullException(nameof(zeroValue), "Нулевое значение не должно быть null.");
        }

        number = zeroValue.Copy();
        IsOn = false;
    }
}
