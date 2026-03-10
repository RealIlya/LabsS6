using PhoneBookLab2.Core;

namespace PhoneBookLab2.Tests;

public class AbonentListTests
{
    [Fact]
    public void AddRecord_KeepsSortedByName()
    {
        var list = new AbonentList();

        list.AddRecord(new Abonent(new AbonentRecord("Петров", "200")));
        list.AddRecord(new Abonent(new AbonentRecord("Иванов", "100")));
        list.AddRecord(new Abonent(new AbonentRecord("Сидоров", "300")));

        Assert.Equal(3, list.RecordsCount);
        Assert.Equal("Иванов", list.ReadRecord(0).Name);
        Assert.Equal("Петров", list.ReadRecord(1).Name);
        Assert.Equal("Сидоров", list.ReadRecord(2).Name);
    }

    [Fact]
    public void FindRecord_Existing_ReturnsIndex()
    {
        var list = new AbonentList();
        var target = new Abonent(new AbonentRecord("Иванов", "100"));
        list.AddRecord(target);

        var index = list.FindRecord(new Abonent(new AbonentRecord("Иванов", "100")));
        Assert.Equal(0, index);
    }

    [Fact]
    public void RemoveRecord_Existing_RemovesAndReturnsTrue()
    {
        var list = new AbonentList();
        list.AddRecord(new Abonent(new AbonentRecord("Иванов", "100")));
        list.AddRecord(new Abonent(new AbonentRecord("Петров", "200")));

        var removed = list.RemoveRecord(new Abonent(new AbonentRecord("Иванов", "100")));
        Assert.True(removed);
        Assert.Equal(1, list.RecordsCount);
        Assert.Equal("Петров", list.ReadRecord(0).Name);
    }

    [Fact]
    public void ClearList_RemovesAllItems()
    {
        var list = new AbonentList();
        list.AddRecord(new Abonent(new AbonentRecord("Иванов", "100")));
        list.ClearList();

        Assert.Equal(0, list.RecordsCount);
    }
}
