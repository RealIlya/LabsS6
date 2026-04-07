using UniversalCalculatorLab4.Core;

namespace UniversalCalculatorLab4.Tests;

public class EditorAndProcessorTests
{
    [Fact]
    public void PNumberEditor_AddSeparator_AddsDotOnce()
    {
        var editor = new PNumberEditor(16);

        editor.AddDigit(10);
        editor.AddSeparator();
        editor.AddDigit(1);
        editor.AddSeparator();

        Assert.Equal("A.1", editor.Value);
    }

    [Fact]
    public void PNumberEditor_DigitAboveBase_Throws()
    {
        var editor = new PNumberEditor(8);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => editor.AddDigit(8));

        Assert.Equal("digit", ex.ParamName);
    }

    [Fact]
    public void FractionEditor_AddSeparator_UsesSlash()
    {
        var editor = new FractionEditor();

        editor.AddDigit(5);
        editor.AddSeparator();
        editor.AddDigit(8);

        Assert.Equal("5/8", editor.Value);
    }

    [Fact]
    public void ComplexEditor_AddSeparator_UsesSemicolon()
    {
        var editor = new ComplexEditor();

        editor.AddDigit(1);
        editor.AddSeparator();
        editor.AddDigit(2);

        Assert.Equal("1;2", editor.Value);
    }

    [Fact]
    public void ComplexEditor_ToggleSign_NegatesBothParts()
    {
        var editor = new ComplexEditor();

        editor.AddDigit(1);
        editor.AddSeparator();
        editor.AddDigit(2);
        editor.ToggleSign();

        Assert.Equal("-1;-2", editor.Value);
    }

    [Fact]
    public void ComplexEditor_CanBuildFractionalNegativeImaginary()
    {
        var editor = new ComplexEditor();

        editor.AddDigit(1);
        editor.AddDecimalSeparator();
        editor.AddDigit(5);
        editor.AddSeparator();
        editor.ToggleImaginarySign();
        editor.AddDigit(2);
        editor.AddDecimalSeparator();
        editor.AddDigit(2);
        editor.AddDigit(5);

        Assert.Equal("1.5;-2.25", editor.Value);
    }

    [Fact]
    public void ComplexEditor_ToggleRealSign_ChangesOnlyRealPart()
    {
        var editor = new ComplexEditor();

        editor.AddDigit(1);
        editor.AddSeparator();
        editor.AddDigit(2);
        editor.ToggleRealSign();

        Assert.Equal("-1;2", editor.Value);
    }

    [Fact]
    public void ComplexEditor_WithHexBase_AllowsDigitsAboveNine()
    {
        var editor = new ComplexEditor(16);

        editor.AddDigit(10);
        editor.AddSeparator();
        editor.AddDigit(15);

        Assert.Equal("A;F", editor.Value);
    }

    [Fact]
    public void Editor_Backspace_FromSingleDigit_ReturnsZero()
    {
        var editor = new FractionEditor();

        editor.AddDigit(7);
        editor.Backspace();

        Assert.Equal("0", editor.Value);
    }

    [Fact]
    public void Editor_ToggleSign_FromZero_ProducesMinusZero()
    {
        var editor = new PNumberEditor(10);

        editor.ToggleSign();

        Assert.Equal("-0", editor.Value);
    }

    [Fact]
    public void Memory_AddingInverseValue_KeepsMemoryOnAndStoresZero()
    {
        var memory = new TMemory(new TPNumber(0, 10, 4));

        memory.Store(new TPNumber(3, 10, 4));
        memory.Add(new TPNumber(-3, 10, 4));

        Assert.True(memory.IsOn);
        Assert.Equal("0", memory.Read().ToString());
    }

    [Fact]
    public void Memory_NullDefaultValue_ThrowsRussianMessage()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new TMemory(null!));

        Assert.Equal("defaultNumber", ex.ParamName);
        Assert.Contains("не должно быть null", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Processor_DivisionByZero_SetsError()
    {
        var left = new TPNumber(8, 10, 4);
        var right = new TPNumber(0, 10, 4);
        var processor = new TProcessor(left, right);
        processor.SetOperation(BinaryOperation.Dvd);

        var ex = Assert.Throws<DivideByZeroException>(() => processor.RunOperation());

        Assert.Equal(ex.Message, processor.Error);
    }

    [Fact]
    public void Processor_RunFunction_StoresFunctionResultAsRightOperand()
    {
        var processor = new TProcessor(new TFrac(7, 3), new TFrac(1, 2));

        var result = processor.RunFunction(UnaryFunction.Rev);

        Assert.Equal("2/1", result.ToString());
        Assert.Equal("7/3", processor.LeftResult.ToString());
        Assert.Equal("2/1", processor.RightOperand.ToString());
    }
}
