namespace ConverterLab1;

public struct Record
{
    public int p1;
    public int p2;
    public string number1;
    public string number2;

    public Record(int p1, int p2, string n1, string n2)
    {
        this.p1 = p1;
        this.p2 = p2;
        number1 = n1;
        number2 = n2;
    }

    public override string ToString()
    {
        return $"{number1} ({p1}) -> {number2} ({p2}){Environment.NewLine}";
    }
}

public class History
{
    private readonly List<Record> L;

    public History()
    {
        L = new List<Record>();
    }

    public Record this[int i]
    {
        get { return L[i]; }
    }

    public void AddRecord(int p1, int p2, string n1, string n2)
    {
        L.Add(new Record(p1, p2, n1, n2));
    }

    public void Clear()
    {
        L.Clear();
    }

    public int Count()
    {
        return L.Count;
    }

    // Методы-синонимы под названия из методички.
    public void ДобавитьЗапись(int p1, int p2, string n1, string n2)
    {
        AddRecord(p1, p2, n1, n2);
    }

    public int Записей()
    {
        return Count();
    }

    public void ОчиститьИсторию()
    {
        Clear();
    }
}
