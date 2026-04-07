using UniversalCalculatorLab4.Core;

namespace UniversalCalculatorLab4.Tests;

public class NumberHierarchyTests
{
    [Fact]
    public void TPNumber_Add_ReturnsExpected()
    {
        var a = new TPNumber(5, 10, 4);
        var b = new TPNumber(3, 10, 4);

        var sum = a.Add(b);

        Assert.Equal("8", sum.ToString());
    }

    [Fact]
    public void TPNumber_ToString_UsesBaseAndPrecision()
    {
        var number = new TPNumber(10.5, 2, 4);

        Assert.Equal("1010.1", number.ToString());
    }

    [Fact]
    public void TPNumber_Parse_InvalidDigitForBase_Throws()
    {
        var ex = Assert.Throws<FormatException>(() => TPNumber.Parse("19", 8));

        Assert.Equal("Цифра '9' недопустима для основания 8.", ex.Message);
    }

    [Fact]
    public void TPNumber_Rev_Zero_Throws()
    {
        var number = new TPNumber(0, 10, 4);

        var ex = Assert.Throws<DivideByZeroException>(() => number.Rev());

        Assert.Equal("Деление на ноль.", ex.Message);
    }

    [Fact]
    public void TFrac_Mul_ReducesFraction()
    {
        var a = new TFrac(2, 3);
        var b = new TFrac(3, 4);

        var result = a.Mul(b);

        Assert.Equal("1/2", result.ToString());
    }

    [Fact]
    public void TFrac_WithNegativeDenominator_NormalizesSign()
    {
        var fraction = new TFrac(1, -2);

        Assert.Equal("-1/2", fraction.ToString());
    }

    [Fact]
    public void TFrac_StringConstructor_ZeroDenominator_Throws()
    {
        var ex = Assert.Throws<DivideByZeroException>(() => new TFrac("1/0"));

        Assert.Equal("Знаменатель не должен быть равен нулю.", ex.Message);
    }

    [Fact]
    public void TFrac_Rev_Zero_Throws()
    {
        var fraction = new TFrac(0, 5);

        var ex = Assert.Throws<DivideByZeroException>(() => fraction.Rev());

        Assert.Equal("Деление на ноль.", ex.Message);
    }

    [Fact]
    public void TComp_Sqr_ReturnsExpected()
    {
        var z = new TComp(2, 3);

        var sqr = z.Sqr();

        Assert.Equal("-5+i*12", sqr.ToString());
    }

    [Fact]
    public void TComp_StringConstructor_WithSemicolon_Parses()
    {
        var value = new TComp("1.5;2.25");

        Assert.Equal("1.5+i*2.25", value.ToString());
    }

    [Fact]
    public void TComp_StringConstructor_WithAlgebraicForm_Parses()
    {
        var value = new TComp("1.5-i*2.25");

        Assert.Equal("1.5-i*2.25", value.ToString());
    }

    [Fact]
    public void TComp_DivisionByZero_Throws()
    {
        var value = new TComp(1, 2);
        var zero = new TComp(0, 0);

        var ex = Assert.Throws<DivideByZeroException>(() => value.Div(zero));

        Assert.Equal("Деление на ноль.", ex.Message);
    }

    [Fact]
    public void TComp_StoresComponentsAsTPNumber()
    {
        var value = new TComp("A.5;-F", 16, 8);

        Assert.IsType<TPNumber>(value.Re);
        Assert.IsType<TPNumber>(value.Im);
        Assert.Equal("A.5", value.Re.ToString());
        Assert.Equal("-F", value.Im.ToString());
    }
}
