namespace PhoneBookLab2.Core;

public sealed class Abonent
{
    private AbonentRecord record;

    public Abonent()
    {
        record = new AbonentRecord(string.Empty, string.Empty);
    }

    public Abonent(AbonentRecord record)
    {
        Write(record);
    }

    public string Name => record.Name;
    public string Number => record.Number;

    public AbonentRecord Read()
    {
        return record;
    }

    public void Write(AbonentRecord newRecord)
    {
        if (string.IsNullOrWhiteSpace(newRecord.Name))
        {
            throw new ArgumentException("Имя не должно быть пустым.", nameof(newRecord));
        }

        if (string.IsNullOrWhiteSpace(newRecord.Number))
        {
            throw new ArgumentException("Номер не должен быть пустым.", nameof(newRecord));
        }

        record = new AbonentRecord(newRecord.Name.Trim(), newRecord.Number.Trim());
    }

    public int Less(Abonent other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other), "Сравниваемый абонент не должен быть null.");
        }

        var byName = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (byName < 0)
        {
            return -1;
        }

        if (byName > 0)
        {
            return 1;
        }

        var byNumber = string.Compare(Number, other.Number, StringComparison.OrdinalIgnoreCase);
        if (byNumber < 0)
        {
            return -1;
        }

        if (byNumber > 0)
        {
            return 1;
        }

        return 0;
    }

    public bool EqualsRecord(Abonent other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other), "Сравниваемый абонент не должен быть null.");
        }

        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Number, other.Number, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return $"{Name} | {Number}";
    }
}

