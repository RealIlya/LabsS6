namespace PhoneBookLab2.Core;

public sealed class AbonentList
{
    private readonly List<Abonent> items = new();

    public int RecordsCount => items.Count;

    public Abonent ReadRecord(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Индекс записи выходит за пределы списка.");
        }

        return items[index];
    }

    public void AddRecord(Abonent abonent)
    {
        if (abonent is null)
        {
            throw new ArgumentNullException(nameof(abonent), "Абонент не должен быть null.");
        }

        var insertIndex = items.FindIndex(x => x.Less(abonent) > 0);
        if (insertIndex < 0)
        {
            items.Add(abonent);
            return;
        }

        items.Insert(insertIndex, abonent);
    }

    public int FindRecord(Abonent abonent)
    {
        if (abonent is null)
        {
            throw new ArgumentNullException(nameof(abonent), "Абонент не должен быть null.");
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].EqualsRecord(abonent))
            {
                return i;
            }
        }

        return -1;
    }

    public bool RemoveRecord(Abonent abonent)
    {
        var index = FindRecord(abonent);
        if (index < 0)
        {
            return false;
        }

        items.RemoveAt(index);
        return true;
    }

    public void RemoveSelectedRecord(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Индекс записи выходит за пределы списка.");
        }

        items.RemoveAt(index);
    }

    public void ClearList()
    {
        items.Clear();
    }

    public IReadOnlyList<AbonentRecord> Snapshot()
    {
        return items.Select(x => x.Read()).ToList();
    }

    public void ReplaceAll(IEnumerable<AbonentRecord> records)
    {
        if (records is null)
        {
            throw new ArgumentNullException(nameof(records), "Список записей не должен быть null.");
        }

        items.Clear();
        foreach (var record in records)
        {
            AddRecord(new Abonent(record));
        }
    }
}
