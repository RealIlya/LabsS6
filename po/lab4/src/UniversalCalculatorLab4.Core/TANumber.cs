namespace UniversalCalculatorLab4.Core;

public abstract class TANumber
{
    public abstract bool IsZero();
    public abstract TANumber Copy();
    public abstract TANumber Add(TANumber other);
    public abstract TANumber Sub(TANumber other);
    public abstract TANumber Mul(TANumber other);
    public abstract TANumber Div(TANumber other);
    public abstract bool EqualsTo(TANumber other);
    public abstract TANumber Sqr();
    public abstract TANumber Rev();
    public abstract TANumber Negate();
}
