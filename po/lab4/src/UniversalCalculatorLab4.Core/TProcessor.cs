namespace UniversalCalculatorLab4.Core;

public sealed class TProcessor
{
    private TANumber left;
    private TANumber right;

    public TProcessor(TANumber leftDefault, TANumber rightDefault)
    {
        if (leftDefault is null)
        {
            throw new ArgumentNullException(nameof(leftDefault), "Левый операнд по умолчанию не должен быть null.");
        }

        if (rightDefault is null)
        {
            throw new ArgumentNullException(nameof(rightDefault), "Правый операнд по умолчанию не должен быть null.");
        }

        left = leftDefault.Copy();
        right = rightDefault.Copy();
    }

    public BinaryOperation Operation { get; private set; } = BinaryOperation.None;
    public string Error { get; private set; } = string.Empty;
    public TANumber LeftResult => left.Copy();
    public TANumber RightOperand => right.Copy();

    public void Reset(TANumber leftDefault, TANumber rightDefault)
    {
        left = leftDefault.Copy();
        right = rightDefault.Copy();
        Operation = BinaryOperation.None;
        Error = string.Empty;
    }

    public void SetLeft(TANumber value) => left = value.Copy();
    public void SetRight(TANumber value) => right = value.Copy();
    public void SetOperation(BinaryOperation operation) => Operation = operation;
    public void ClearOperation() => Operation = BinaryOperation.None;

    public TANumber RunOperation()
    {
        try
        {
            left = Operation switch
            {
                BinaryOperation.None => left,
                BinaryOperation.Add => left.Add(right),
                BinaryOperation.Sub => left.Sub(right),
                BinaryOperation.Mul => left.Mul(right),
                BinaryOperation.Dvd => left.Div(right),
                _ => throw new InvalidOperationException("Неподдерживаемая операция.")
            };
            Error = string.Empty;
            return left.Copy();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            throw;
        }
    }

    public TANumber RunFunction(UnaryFunction function)
    {
        try
        {
            right = function switch
            {
                UnaryFunction.Rev => right.Rev(),
                UnaryFunction.Sqr => right.Sqr(),
                _ => throw new InvalidOperationException("Неподдерживаемая функция.")
            };
            Error = string.Empty;
            return right.Copy();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            throw;
        }
    }
}

