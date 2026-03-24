namespace CalculatorPart1Lab3.Core;

public sealed class CalculatorControl
{
    public enum CalcState
    {
        Start,
        Editing,
        OperationSet,
        Result
    }

    private readonly TPNumberEditor editor;
    private readonly TProcessor<TPNumber> processor;
    private readonly TMemory<TPNumber> memory;

    public CalculatorControl(int numberBase = 10, int precision = 10)
    {
        NumberBase = numberBase;
        Precision = precision;
        editor = new TPNumberEditor(numberBase);
        var zero = new TPNumber(0, numberBase, precision);
        processor = new TProcessor<TPNumber>(zero, zero);
        memory = new TMemory<TPNumber>(zero);
    }

    public int NumberBase { get; private set; }
    public int Precision { get; private set; }
    public CalcState State { get; private set; } = CalcState.Start;

    public string Display => editor.Value;
    public bool MemoryState => memory.IsOn;

    public void SetBase(int numberBase)
    {
        if (numberBase == NumberBase)
        {
            return;
        }

        var previousState = State;
        var previousOperation = processor.Operation;
        var currentValue = ReadCurrentNumber().WithBase(numberBase);
        var leftValue = processor.LeftResult.WithBase(numberBase);
        var rightValue = processor.RightOperand.WithBase(numberBase);
        var memoryValue = memory.IsOn ? memory.Read().WithBase(numberBase) : null;

        NumberBase = numberBase;
        editor.SetBase(numberBase);
        WriteToEditor(currentValue);

        processor.Reset(leftValue, rightValue);
        processor.SetOperation(previousOperation);

        var zero = new TPNumber(0, numberBase, Precision);
        memory.Clear(zero);
        if (memoryValue is not null)
        {
            memory.Store(memoryValue);
        }

        State = previousState;
    }

    public string ExecuteEditorCommand(int command)
    {
        if (State is CalcState.Result or CalcState.OperationSet)
        {
            editor.Clear();
            State = CalcState.Editing;
        }

        editor.Edit(command);
        if (State == CalcState.Start)
        {
            State = CalcState.Editing;
        }

        return Display;
    }

    public string ExecuteOperation(BinaryOperation operation)
    {
        var current = ReadCurrentNumber();

        if (State == CalcState.OperationSet)
        {
            processor.SetRight(current);
            processor.RunOperation();
        }
        else
        {
            processor.SetLeft(current);
        }

        processor.SetOperation(operation);
        State = CalcState.OperationSet;
        editor.Clear();
        return processor.LeftResult.ToString();
    }

    public string ExecuteFunction(UnaryFunction function)
    {
        var current = ReadCurrentNumber();
        processor.SetLeft(current);
        var result = processor.RunFunction(function);
        State = CalcState.Result;
        WriteToEditor(result);
        return Display;
    }

    public string ExecuteEqual()
    {
        if (State != CalcState.Result)
        {
            processor.SetRight(ReadCurrentNumber());
        }

        var result = processor.RunOperation();
        State = CalcState.Result;
        WriteToEditor(result);
        return Display;
    }

    public string ExecuteMemoryCommand(int command)
    {
        var current = ReadCurrentNumber();
        switch (command)
        {
            case 0: // MS
                memory.Store(current);
                return Display;
            case 1: // MR
                WriteToEditor(memory.Read());
                State = CalcState.Result;
                return Display;
            case 2: // M+
                memory.Add(current);
                return Display;
            case 3: // MC
                memory.Clear(new TPNumber(0, NumberBase, Precision));
                return Display;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), "Неподдерживаемая команда памяти.");
        }
    }

    public string ExecuteClipboardCommand(int command, string? clipboardValue = null)
    {
        switch (command)
        {
            case 0:
                return Display;
            case 1:
                if (string.IsNullOrWhiteSpace(clipboardValue))
                {
                    return Display;
                }

                var parsed = new TPNumber(clipboardValue.Trim(), NumberBase, Precision);
                WriteToEditor(parsed);
                State = CalcState.Result;
                return Display;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), "Неподдерживаемая команда буфера обмена.");
        }
    }

    public string Reset()
    {
        editor.Clear();
        var zero = new TPNumber(0, NumberBase, Precision);
        processor.Reset(zero, zero);
        State = CalcState.Start;
        return Display;
    }

    private TPNumber ReadCurrentNumber()
    {
        return new TPNumber(editor.Value, NumberBase, Precision);
    }

    private void WriteToEditor(TPNumber value)
    {
        editor.Clear();
        var text = value.ToString();
        foreach (var ch in text)
        {
            if (ch == '-')
            {
                editor.ToggleSign();
                continue;
            }

            if (ch == '.')
            {
                editor.AddSeparator();
                continue;
            }

            if (ch >= '0' && ch <= '9')
            {
                editor.AddDigit(ch - '0');
                continue;
            }

            if (ch >= 'A' && ch <= 'F')
            {
                editor.AddDigit(ch - 'A' + 10);
            }
        }
    }
}

