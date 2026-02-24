using ConverterLab1;

namespace ConverterLab1.Tests;

public class HistoryControlTests
{
    [Fact]
    public void History_AddRecord_IncreasesCount()
    {
        var history = new History();
        history.AddRecord(10, 16, "17.5", "11.8");
        Assert.Equal(1, history.Count());
    }

    [Fact]
    public void History_Indexer_ReturnsAddedRecord()
    {
        var history = new History();
        history.AddRecord(10, 2, "5", "101");
        var rec = history[0];
        Assert.Equal(10, rec.p1);
        Assert.Equal(2, rec.p2);
        Assert.Equal("5", rec.number1);
        Assert.Equal("101", rec.number2);
    }

    [Fact]
    public void History_Clear_RemovesAllRecords()
    {
        var history = new History();
        history.AddRecord(10, 8, "10", "12");
        history.Clear();
        Assert.Equal(0, history.Count());
    }

    [Fact]
    public void Control_DoCmnd19_ConvertsAndWritesHistory()
    {
        var ctl = new Control_();
        ctl.Pin = 10;
        ctl.Pout = 16;

        ctl.ed.Clear();
        ctl.ed.AddDigit(2);
        ctl.ed.AddDigit(5);
        ctl.ed.AddDelim();
        ctl.ed.AddDigit(5);

        var result = ctl.DoCmnd(19);

        Assert.Equal("19.8", result);
        Assert.Equal(1, ctl.his.Count());
    }

    [Fact]
    public void Control_DoCmnd19_ConvertsNegativeValue()
    {
        var ctl = new Control_();
        ctl.Pin = 10;
        ctl.Pout = 16;

        ctl.ed.Clear();
        ctl.ed.DoEdit(20);
        ctl.ed.AddDigit(1);
        ctl.ed.AddDigit(0);
        ctl.ed.AddDelim();
        ctl.ed.AddDigit(5);

        var result = ctl.DoCmnd(19);

        Assert.Equal("-A.8", result);
        Assert.Equal(1, ctl.his.Count());
    }

    [Fact]
    public void Control_RecalculateWithoutHistory_RecalculatesButDoesNotWriteHistory()
    {
        var ctl = new Control_();
        ctl.Pin = 10;
        ctl.Pout = 16;

        ctl.ed.Clear();
        ctl.ed.AddDigit(2);
        ctl.ed.AddDigit(5);
        ctl.ed.AddDelim();
        ctl.ed.AddDigit(5);

        var initial = ctl.DoCmnd(19);
        Assert.Equal("19.8", initial);
        Assert.Equal(1, ctl.his.Count());

        ctl.Pout = 2;
        var recalculated = ctl.RecalculateWithoutHistory();

        Assert.Equal("11001.1", recalculated);
        Assert.Equal(1, ctl.his.Count());
    }
}
