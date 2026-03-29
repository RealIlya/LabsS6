namespace CalculatorPart1Lab3.Core;

public interface ICalcNumber<T>
{
    bool IsZero();
    T Copy();
    T Add(T other);
    T Sub(T other);
    T Mul(T other);
    T Div(T other);
    bool EqualsTo(T other);
    T Sqr();
    T Rev();
}
