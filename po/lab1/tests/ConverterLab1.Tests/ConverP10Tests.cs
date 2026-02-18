using ConverterLab1;

namespace ConverterLab1.Tests;

public class ConverP10Tests
{
    [Fact]
    public void char_To_num_ForA_Returns10()
    {
        var result = Conver_P_10.char_To_num('A');
        Assert.Equal(10, result);
    }

    [Fact]
    public void dval_ForA5E_Base16_Returns265875()
    {
        var result = Conver_P_10.dval("A5.E", 16);
        Assert.Equal(165.875, result, 6);
    }

    [Fact]
    public void dval_ForMinusBinary_ReturnsMinus105()
    {
        var result = Conver_P_10.dval("-1010.1", 2);
        Assert.Equal(-10.5, result, 6);
    }
}
