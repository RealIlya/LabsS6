using PhoneBookLab2.Core;

namespace PhoneBookLab2.Tests;

public class AbonentTests
{
    [Fact]
    public void WriteRead_ValidRecord_ReturnsSameRecord()
    {
        var abonent = new Abonent();
        abonent.Write(new AbonentRecord("Иванов", "12345"));

        var record = abonent.Read();
        Assert.Equal("Иванов", record.Name);
        Assert.Equal("12345", record.Number);
    }

    [Fact]
    public void Less_ByName_ReturnsMinusOne()
    {
        var left = new Abonent(new AbonentRecord("Алексей", "100"));
        var right = new Abonent(new AbonentRecord("Борис", "100"));

        Assert.Equal(-1, left.Less(right));
    }

    [Fact]
    public void Less_SameNameDifferentNumber_ReturnsComparison()
    {
        var left = new Abonent(new AbonentRecord("Иванов", "100"));
        var right = new Abonent(new AbonentRecord("Иванов", "200"));

        Assert.Equal(-1, left.Less(right));
    }

    [Fact]
    public void EqualsRecord_SameValuesDifferentCase_ReturnsTrue()
    {
        var left = new Abonent(new AbonentRecord("Иванов", "12345"));
        var right = new Abonent(new AbonentRecord("иванов", "12345"));

        Assert.True(left.EqualsRecord(right));
    }
}
