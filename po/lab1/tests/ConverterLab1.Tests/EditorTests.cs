using ConverterLab1;

namespace ConverterLab1.Tests;

public class EditorTests
{
    [Fact]
    public void AddDigit_FromZero_ReplacesZero()
    {
        var ed = new Editor();
        var result = ed.AddDigit(5);
        Assert.Equal("5", result);
    }

    [Fact]
    public void AddZero_WhenNumberIsNotZero_AppendsZero()
    {
        var ed = new Editor();
        ed.AddDigit(7);
        var result = ed.AddZero();
        Assert.Equal("70", result);
    }

    [Fact]
    public void AddDelim_WhenNoDelim_AppendsPoint()
    {
        var ed = new Editor();
        ed.AddDigit(2);
        var result = ed.AddDelim();
        Assert.Equal("2.", result);
    }

    [Fact]
    public void Bs_RemovesLastCharacter()
    {
        var ed = new Editor();
        ed.AddDigit(1);
        ed.AddDigit(2);
        var result = ed.Bs();
        Assert.Equal("1", result);
    }

    [Fact]
    public void Clear_SetsZero()
    {
        var ed = new Editor();
        ed.AddDigit(9);
        var result = ed.Clear();
        Assert.Equal("0", result);
    }

    [Fact]
    public void Acc_ForNumberWithFraction_ReturnsFractionLength()
    {
        var ed = new Editor();
        ed.AddDigit(1);
        ed.AddDelim();
        ed.AddDigit(10);
        ed.AddDigit(11);
        Assert.Equal(2, ed.Acc());
    }

    [Fact]
    public void DoEdit_Command16_AddsDelimiter()
    {
        var ed = new Editor();
        ed.AddDigit(3);
        var result = ed.DoEdit(16);
        Assert.Equal("3.", result);
    }

    [Fact]
    public void ToggleSign_FromZero_ReturnsMinusZero()
    {
        var ed = new Editor();
        var result = ed.ToggleSign();
        Assert.Equal("-0", result);
    }

    [Fact]
    public void AddDigit_AfterMinusZero_ReplacesZeroAndKeepsSign()
    {
        var ed = new Editor();
        ed.ToggleSign();
        var result = ed.AddDigit(5);
        Assert.Equal("-5", result);
    }

    [Fact]
    public void DoEdit_Command20_TogglesSign()
    {
        var ed = new Editor();
        ed.AddDigit(9);
        var negative = ed.DoEdit(20);
        var positive = ed.DoEdit(20);

        Assert.Equal("-9", negative);
        Assert.Equal("9", positive);
    }
}
