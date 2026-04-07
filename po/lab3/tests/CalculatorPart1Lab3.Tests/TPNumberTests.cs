using CalculatorPart1Lab3.Core;

namespace CalculatorPart1Lab3.Tests;

public class TPNumberTests
{
    [Fact]
    public void Constructor_InvalidBase_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new TPNumber(1, 1, 4));

        Assert.Equal("numberBase", ex.ParamName);
    }

    [Fact]
    public void Parse_Base16String_ReturnsExpectedDecimal()
    {
        var value = TPNumber.Parse("A.8", 16);

        Assert.Equal(10.5, value, 6);
    }

    [Fact]
    public void Parse_EmptyString_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => TPNumber.Parse(" ", 10));

        Assert.Equal("text", ex.ParamName);
    }

    [Fact]
    public void IsZero_ZeroValue_ReturnsTrue()
    {
        var value = new TPNumber(0, 10, 4);

        Assert.True(value.IsZero());
    }

    [Fact]
    public void Copy_ReturnsEquivalentIndependentObject()
    {
        var original = new TPNumber(10.5, 10, 4);

        var copy = original.Copy();

        Assert.NotSame(original, copy);
        Assert.True(original.EqualsTo(copy));
    }

    [Fact]
    public void ToString_Base2_ReturnsBinaryRepresentation()
    {
        var number = new TPNumber(10.5, 2, 4);

        Assert.Equal("1010.1", number.ToString());
    }

    [Fact]
    public void Add_SameBase_ReturnsSum()
    {
        var a = new TPNumber(5, 10, 4);
        var b = new TPNumber(7, 10, 4);

        Assert.Equal("12", a.Add(b).ToString());
    }

    [Fact]
    public void Sub_SameBase_ReturnsDifference()
    {
        var a = new TPNumber(9, 10, 4);
        var b = new TPNumber(4, 10, 4);

        Assert.Equal("5", a.Sub(b).ToString());
    }

    [Fact]
    public void Mul_SameBase_ReturnsProduct()
    {
        var a = new TPNumber(3, 10, 4);
        var b = new TPNumber(4, 10, 4);

        Assert.Equal("12", a.Mul(b).ToString());
    }

    [Fact]
    public void Div_SameBase_ReturnsQuotient()
    {
        var a = new TPNumber(8, 10, 4);
        var b = new TPNumber(2, 10, 4);

        Assert.Equal("4", a.Div(b).ToString());
    }

    [Fact]
    public void EqualsTo_SameValue_ReturnsTrue()
    {
        var a = new TPNumber(10.5, 10, 4);
        var b = new TPNumber("10.5", 10, 4);

        Assert.True(a.EqualsTo(b));
    }

    [Fact]
    public void Sqr_ReturnsSquaredValue()
    {
        var value = new TPNumber(3, 10, 4);

        Assert.Equal("9", value.Sqr().ToString());
    }

    [Fact]
    public void Rev_Zero_Throws()
    {
        var zero = new TPNumber(0, 10, 4);

        Assert.Throws<DivideByZeroException>(() => zero.Rev());
    }

    [Fact]
    public void Negate_ReturnsNumberWithOppositeSign()
    {
        var value = new TPNumber(3.5, 10, 4);

        Assert.Equal("-3.5", value.Negate().ToString());
    }

    [Fact]
    public void WithBase_ChangesOnlyBaseRepresentation()
    {
        var value = new TPNumber(10.5, 10, 4);

        var converted = value.WithBase(2);

        Assert.Equal(2, converted.NumberBase);
        Assert.Equal(10.5, converted.Value, 6);
        Assert.Equal("1010.1", converted.ToString());
    }
}
