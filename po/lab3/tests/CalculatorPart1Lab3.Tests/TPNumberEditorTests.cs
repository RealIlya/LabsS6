using CalculatorPart1Lab3.Core;

namespace CalculatorPart1Lab3.Tests;

public class TPNumberEditorTests
{
    [Fact]
    public void Constructor_InvalidBase_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new TPNumberEditor(1));

        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void SetBase_ClearsValueAndUpdatesBase()
    {
        var editor = new TPNumberEditor(16);
        editor.AddDigit(10);

        editor.SetBase(2);

        Assert.Equal(2, editor.NumberBase);
        Assert.Equal("0", editor.Value);
    }

    [Fact]
    public void IsZero_NegativeZero_ReturnsTrue()
    {
        var editor = new TPNumberEditor(10);
        editor.ToggleSign();

        Assert.True(editor.IsZero());
    }

    [Fact]
    public void ToggleSign_Twice_ReturnsOriginal()
    {
        var editor = new TPNumberEditor(10);

        editor.ToggleSign();
        editor.ToggleSign();

        Assert.Equal("0", editor.Value);
    }

    [Fact]
    public void AddDigit_FromZero_ReplacesZero()
    {
        var editor = new TPNumberEditor(16);

        editor.AddDigit(10);

        Assert.Equal("A", editor.Value);
    }

    [Fact]
    public void AddZero_NonZero_AppendsZero()
    {
        var editor = new TPNumberEditor(10);
        editor.AddDigit(1);

        editor.AddZero();

        Assert.Equal("10", editor.Value);
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
    public void Backspace_RemovesLastCharacter()
    {
        var editor = new TPNumberEditor(16);
        editor.AddDigit(10);
        editor.AddDigit(1);

        editor.Backspace();

        Assert.Equal("A", editor.Value);
    }

    [Fact]
    public void Clear_ResetsValueToZero()
    {
        var editor = new TPNumberEditor(10);
        editor.AddDigit(5);

        editor.Clear();

        Assert.Equal("0", editor.Value);
    }

    [Fact]
    public void Edit_BackspaceCommand_DelegatesToEditorOperation()
    {
        var editor = new TPNumberEditor(10);
        editor.AddDigit(7);
        editor.AddDigit(2);

        editor.Edit(17);

        Assert.Equal("7", editor.Value);
    }
}
