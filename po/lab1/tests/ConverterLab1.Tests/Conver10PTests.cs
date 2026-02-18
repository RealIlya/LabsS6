using ConverterLab1;

namespace ConverterLab1.Tests;

public class Conver10PTests
{
    [Fact]
    public void int_to_Char_For14_ReturnsE()
    {
        var result = Conver_10_P.int_to_Char(14);
        Assert.Equal('E', result);
    }

    [Fact]
    public void int_to_P_For161Base16_ReturnsA1()
    {
        var result = Conver_10_P.int_to_P(161, 16);
        Assert.Equal("A1", result);
    }

    [Fact]
    public void flt_to_P_For09375Base2Precision4_Returns1111()
    {
        var result = Conver_10_P.flt_to_P(0.9375, 2, 4);
        Assert.Equal("1111", result);
    }

    [Fact]
    public void Do_ForMinus17875Base16Precision3_ReturnsMinus11E()
    {
        var result = Conver_10_P.Do(-17.875, 16, 3);
        Assert.Equal("-11.E", result);
    }
}
