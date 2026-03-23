using CalculatorPart1Lab3.Core;

namespace CalculatorPart1Lab3.Tests;

public class TPNumberEditorTests
{
    [Fact]
    public void AddDigit_FromZero_ReplacesZero()
    {
        var editor = new TPNumberEditor(16);
        editor.AddDigit(10);
        Assert.Equal("A", editor.Value);
    }

    [Fact]
    public void AddSeparator_AddsOnlyOnce()
    {
        var editor = new TPNumberEditor(10);
        editor.AddSeparator();
        editor.AddSeparator();
        Assert.Equal("0.", editor.Value);
    }

    [Fact]
    public void ToggleSign_Twice_ReturnsOriginal()
    {
        var editor = new TPNumberEditor(10);
        editor.ToggleSign();
        editor.ToggleSign();
        Assert.Equal("0", editor.Value);
    }
}
