using CalculatorPart1Lab3.Core;

namespace CalculatorPart1Lab3.Tests;

public class TPNumberTests
{
    [Fact]
    public void Parse_Base16String_ReturnsExpectedDecimal()
    {
        var value = TPNumber.Parse("A.8", 16);
        Assert.Equal(10.5, value, 6);
    }

    [Fact]
    public void ToString_Base2_ReturnsBinaryRepresentation()
    {
        var n = new TPNumber(10.5, 2, 4);
        Assert.Equal("1010.1", n.ToString());
    }

    [Fact]
    public void Add_SameBase_ReturnsSum()
    {
        var a = new TPNumber(5, 10, 4);
        var b = new TPNumber(7, 10, 4);
        Assert.Equal("12", a.Add(b).ToString());
    }

    [Fact]
    public void Rev_Zero_Throws()
    {
        var zero = new TPNumber(0, 10, 4);
        Assert.Throws<DivideByZeroException>(() => zero.Rev());
    }
}
