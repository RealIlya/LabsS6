namespace CalculatorPart1Lab3.Core;

public sealed class TProcessor<T> where T : ICalcNumber<T>
{
    public TProcessor(T leftDefault, T rightDefault)
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

    private T left;
    private T right;

    public BinaryOperation Operation { get; private set; } = BinaryOperation.None;

    public T LeftResult => left.Copy();
    public T RightOperand => right.Copy();

    public string Error { get; private set; } = string.Empty;

    public void Reset(T leftDefault, T rightDefault)
    {
        left = leftDefault.Copy();
        right = rightDefault.Copy();
        Operation = BinaryOperation.None;
        Error = string.Empty;
    }

    public void SetLeft(T operand)
    {
        left = operand.Copy();
    }

    public void SetRight(T operand)
    {
        right = operand.Copy();
    }

    public void SetOperation(BinaryOperation operation)
    {
        Operation = operation;
    }

    public void ClearOperation()
    {
        Operation = BinaryOperation.None;
    }

    public T RunOperation()
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

    public T RunFunction(UnaryFunction function)
    {
        try
        {
            left = function switch
            {
                UnaryFunction.Rev => left.Rev(),
                UnaryFunction.Sqr => left.Sqr(),
                _ => throw new InvalidOperationException("Неподдерживаемая функция.")
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
}

