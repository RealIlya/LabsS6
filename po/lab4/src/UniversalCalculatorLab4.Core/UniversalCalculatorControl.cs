namespace UniversalCalculatorLab4.Core;

public sealed class UniversalCalculatorControl
{
    public enum TCtrlState
    {
        cStart,
        cEditing,
        FunDone,
        cValDone,
        cExpDone,
        cOpChange,
        cError
    }

    private AEditor editor;
    private TProcessor processor;
    private TMemory memory;
    private TANumber number;
    private BinaryOperation repeatedEqualOperation = BinaryOperation.None;
    private TANumber? repeatedEqualOperand;

    public UniversalCalculatorControl()
    {
        PNumberBase = 10;
        PNumberPrecision = 10;

        editor = CreateEditor(Mode);
        var zero = CreateZero(Mode);
        processor = new TProcessor(zero, zero);
        memory = new TMemory(zero);
        number = zero.Copy();
    }

    public NumberMode Mode { get; private set; } = NumberMode.PNumber;
    public TCtrlState State { get; private set; } = TCtrlState.cStart;

    public int PNumberBase { get; private set; }
    public int PNumberPrecision { get; private set; }

    public TANumber Number => number.Copy();
    public string Display => editor.Value;
    public bool MemoryState => memory.IsOn;
    public string MemoryStateText => memory.IsOn ? "ON" : "OFF";

    public void SetMode(NumberMode mode)
    {
        ExecuteWithErrorState(() =>
        {
            Mode = mode;
            editor = CreateEditor(mode);
            var zero = CreateZero(mode);
            processor.Reset(zero, zero);
            memory.Clear(zero);
            ClearRepeatedEqualState();
            State = TCtrlState.cStart;
        });
    }

    public void SetPNumberSettings(int numberBase, int precision)
    {
        ExecuteWithErrorState(() =>
        {
            if (Mode == NumberMode.PNumber && editor is PNumberEditor pEditor)
            {
                var previousState = State;
                var previousOperation = processor.Operation;
                var currentValue = ConvertNumberForPBasedMode(ParseCurrent(), numberBase, precision);
                var leftValue = ConvertNumberForPBasedMode(processor.LeftResult, numberBase, precision);
                var rightValue = ConvertNumberForPBasedMode(processor.RightOperand, numberBase, precision);
                var memoryValue = memory.IsOn ? ConvertNumberForPBasedMode(memory.Read(), numberBase, precision) : null;
                var repeatedOperand = repeatedEqualOperand is null
                    ? null
                    : ConvertNumberForPBasedMode(repeatedEqualOperand, numberBase, precision);

                PNumberBase = numberBase;
                PNumberPrecision = precision;

                pEditor.SetBase(numberBase);
                WriteToEditor(currentValue);

                processor.Reset(leftValue, rightValue);
                processor.SetOperation(previousOperation);

                var zero = CreateZero(Mode);
                memory.Clear(zero);
                if (memoryValue is not null)
                {
                    memory.Store(memoryValue);
                }

                repeatedEqualOperand = repeatedOperand;
                State = previousState;
                return;
            }

            if (Mode == NumberMode.Complex && editor is ComplexEditor complexEditor)
            {
                var previousState = State;
                var previousOperation = processor.Operation;
                var currentValue = ConvertNumberForPBasedMode(ParseCurrent(), numberBase, precision);
                var leftValue = ConvertNumberForPBasedMode(processor.LeftResult, numberBase, precision);
                var rightValue = ConvertNumberForPBasedMode(processor.RightOperand, numberBase, precision);
                var memoryValue = memory.IsOn ? ConvertNumberForPBasedMode(memory.Read(), numberBase, precision) : null;
                var repeatedOperand = repeatedEqualOperand is null
                    ? null
                    : ConvertNumberForPBasedMode(repeatedEqualOperand, numberBase, precision);

                PNumberBase = numberBase;
                PNumberPrecision = precision;

                complexEditor.SetBase(numberBase);
                WriteToEditor(currentValue);

                processor.Reset(leftValue, rightValue);
                processor.SetOperation(previousOperation);

                var zero = CreateZero(Mode);
                memory.Clear(zero);
                if (memoryValue is not null)
                {
                    memory.Store(memoryValue);
                }

                repeatedEqualOperand = repeatedOperand;
                State = previousState;
                return;
            }

            PNumberBase = numberBase;
            PNumberPrecision = precision;
        });
    }

    public string ExecuteEditorCommand(int command)
    {
        return ExecuteWithErrorState(() =>
        {
            ClearRepeatedEqualState();

            if (State is TCtrlState.cExpDone or TCtrlState.FunDone or TCtrlState.cValDone or TCtrlState.cOpChange or TCtrlState.cError)
            {
                editor.Clear();
            }

            editor.Edit(command);
            State = TCtrlState.cEditing;
            return Display;
        });
    }

    public string ExecuteOperation(BinaryOperation operation)
    {
        return ExecuteWithErrorState(() =>
        {
            ClearRepeatedEqualState();
            var current = ParseCurrent();

            if (State == TCtrlState.cOpChange)
            {
                processor.SetOperation(operation);
                return processor.LeftResult.ToString() ?? string.Empty;
            }

            if (processor.Operation != BinaryOperation.None)
            {
                processor.SetRight(current);
                var result = processor.RunOperation();
                WriteToEditor(result);
            }
            else
            {
                processor.SetLeft(current);
                WriteToEditor(current);
            }

            processor.SetOperation(operation);
            State = TCtrlState.cOpChange;
            return processor.LeftResult.ToString() ?? string.Empty;
        });
    }

    public string ExecuteFunction(UnaryFunction function)
    {
        return ExecuteWithErrorState(() =>
        {
            ClearRepeatedEqualState();
            var current = ParseCurrent();
            processor.SetRight(current);
            var result = processor.RunFunction(function);
            WriteToEditor(result);
            State = TCtrlState.FunDone;
            return Display;
        });
    }

    public string ExecuteEqual()
    {
        return ExecuteWithErrorState(() =>
        {
            if (State == TCtrlState.cExpDone)
            {
                if (repeatedEqualOperation == BinaryOperation.None || repeatedEqualOperand is null)
                {
                    return Display;
                }

                processor.SetLeft(ParseCurrent());
                processor.SetRight(repeatedEqualOperand);
                processor.SetOperation(repeatedEqualOperation);
            }
            else if (processor.Operation == BinaryOperation.None)
            {
                var current = ParseCurrent();
                processor.SetLeft(current);
                WriteToEditor(current);
                ClearRepeatedEqualState();
                State = TCtrlState.cValDone;
                return Display;
            }
            else
            {
                var current = ParseCurrent();
                processor.SetRight(current);
                repeatedEqualOperation = processor.Operation;
                repeatedEqualOperand = current.Copy();
            }

            var result = processor.RunOperation();
            processor.ClearOperation();
            WriteToEditor(result);
            State = TCtrlState.cExpDone;
            return Display;
        });
    }

    public string ExecuteMemoryCommand(int command)
    {
        var memoryState = MemoryStateText;
        return ExecuteMemoryCommand(command, ref memoryState);
    }

    public string ExecuteMemoryCommand(int command, ref string memoryState)
    {
        try
        {
            ClearRepeatedEqualState();
            var current = ParseCurrent();
            switch (command)
            {
                case 0: // MS
                    memory.Store(current);
                    break;
                case 1: // MR
                    WriteToEditor(memory.Read());
                    State = TCtrlState.cValDone;
                    break;
                case 2: // M+
                    memory.Add(current);
                    break;
                case 3: // MC
                    memory.Clear(CreateZero(Mode));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), "Неподдерживаемая команда памяти.");
            }

            memoryState = MemoryStateText;
            return Display;
        }
        catch
        {
            State = TCtrlState.cError;
            throw;
        }
        finally
        {
            if (State != TCtrlState.cError)
            {
                UpdateNumberSnapshot();
            }
        }
    }

    public string ExecuteClipboardCommand(int command, string? clipboardValue = null)
    {
        var currentClipboard = clipboardValue ?? string.Empty;
        return ExecuteClipboardCommand(command, ref currentClipboard);
    }

    public string ExecuteClipboardCommand(int command, ref string clipboardValue)
    {
        try
        {
            ClearRepeatedEqualState();
            switch (command)
            {
                case 0: // Copy
                    clipboardValue = Display;
                    return Display;
                case 1: // Paste
                    if (string.IsNullOrWhiteSpace(clipboardValue))
                    {
                        return Display;
                    }

                    var parsed = ParseTextForMode(clipboardValue.Trim());
                    WriteToEditor(parsed);
                    State = TCtrlState.cValDone;
                    return Display;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), "Неподдерживаемая команда буфера обмена.");
            }
        }
        catch
        {
            State = TCtrlState.cError;
            throw;
        }
        finally
        {
            if (State != TCtrlState.cError)
            {
                UpdateNumberSnapshot();
            }
        }
    }

    public string Reset()
    {
        return ExecuteWithErrorState(() =>
        {
            ClearRepeatedEqualState();
            editor.Clear();
            var zero = CreateZero(Mode);
            processor.Reset(zero, zero);
            State = TCtrlState.cStart;
            return Display;
        });
    }

    public string ExecuteCalculatorCommand(CalculatorCommand command, string? clipboardValue = null)
    {
        var currentClipboard = clipboardValue ?? string.Empty;
        var memoryState = MemoryStateText;
        return ExecuteCalculatorCommand(command, ref currentClipboard, ref memoryState);
    }

    public string ExecuteCalculatorCommand(CalculatorCommand command, ref string clipboardValue, ref string memoryState)
    {
        return ExecuteCalculatorCommand((int)command, ref clipboardValue, ref memoryState);
    }

    public string ExecuteCalculatorCommand(int command, ref string clipboardValue, ref string memoryState)
    {
        try
        {
            var result = ((CalculatorCommand)command) switch
            {
                CalculatorCommand.Digit0 or
                CalculatorCommand.Digit1 or
                CalculatorCommand.Digit2 or
                CalculatorCommand.Digit3 or
                CalculatorCommand.Digit4 or
                CalculatorCommand.Digit5 or
                CalculatorCommand.Digit6 or
                CalculatorCommand.Digit7 or
                CalculatorCommand.Digit8 or
                CalculatorCommand.Digit9 or
                CalculatorCommand.DigitA or
                CalculatorCommand.DigitB or
                CalculatorCommand.DigitC or
                CalculatorCommand.DigitD or
                CalculatorCommand.DigitE or
                CalculatorCommand.DigitF or
                CalculatorCommand.Separator or
                CalculatorCommand.Backspace or
                CalculatorCommand.EditorClear or
                CalculatorCommand.DecimalSeparator or
                CalculatorCommand.ToggleSign or
                CalculatorCommand.ToggleImaginarySign or
                CalculatorCommand.ToggleRealSign
                    => ExecuteEditorCommand((int)command),
                CalculatorCommand.Add => ExecuteOperation(BinaryOperation.Add),
                CalculatorCommand.Sub => ExecuteOperation(BinaryOperation.Sub),
                CalculatorCommand.Mul => ExecuteOperation(BinaryOperation.Mul),
                CalculatorCommand.Divide => ExecuteOperation(BinaryOperation.Dvd),
                CalculatorCommand.Rev => ExecuteFunction(UnaryFunction.Rev),
                CalculatorCommand.Sqr => ExecuteFunction(UnaryFunction.Sqr),
                CalculatorCommand.Equal => ExecuteEqual(),
                CalculatorCommand.Reset => Reset(),
                CalculatorCommand.MemoryStore => ExecuteMemoryCommand(0, ref memoryState),
                CalculatorCommand.MemoryRecall => ExecuteMemoryCommand(1, ref memoryState),
                CalculatorCommand.MemoryAdd => ExecuteMemoryCommand(2, ref memoryState),
                CalculatorCommand.MemoryClear => ExecuteMemoryCommand(3, ref memoryState),
                CalculatorCommand.ClipboardCopy => ExecuteClipboardCommand(0, ref clipboardValue),
                CalculatorCommand.ClipboardPaste => ExecuteClipboardCommand(1, ref clipboardValue),
                _ => throw new ArgumentOutOfRangeException(nameof(command), "Неподдерживаемая команда калькулятора.")
            };

            memoryState = MemoryStateText;
            return result;
        }
        catch
        {
            State = TCtrlState.cError;
            throw;
        }
        finally
        {
            if (State != TCtrlState.cError)
            {
                UpdateNumberSnapshot();
            }
        }
    }

    private AEditor CreateEditor(NumberMode mode)
    {
        return mode switch
        {
            NumberMode.PNumber => new PNumberEditor(PNumberBase),
            NumberMode.Fraction => new FractionEditor(),
            NumberMode.Complex => new ComplexEditor(PNumberBase),
            _ => throw new InvalidOperationException("Неизвестный режим.")
        };
    }

    private TANumber CreateZero(NumberMode mode)
    {
        return mode switch
        {
            NumberMode.PNumber => new TPNumber(0, PNumberBase, PNumberPrecision),
            NumberMode.Fraction => new TFrac(0, 1),
            NumberMode.Complex => new TComp(0, 0, PNumberBase, PNumberPrecision),
            _ => throw new InvalidOperationException("Неизвестный режим.")
        };
    }

    private TANumber ParseCurrent()
    {
        return ParseTextForMode(editor.Value);
    }

    private TANumber ParseTextForMode(string text)
    {
        return Mode switch
        {
            NumberMode.PNumber => new TPNumber(text, PNumberBase, PNumberPrecision),
            NumberMode.Fraction => new TFrac(text.Contains('/') ? text : text + "/1"),
            NumberMode.Complex => new TComp(
                text.Contains(';', StringComparison.Ordinal) || text.Contains("i*", StringComparison.Ordinal)
                    ? text
                    : text + ";0",
                PNumberBase,
                PNumberPrecision),
            _ => throw new InvalidOperationException("Неизвестный режим.")
        };
    }

    private TANumber ConvertNumberForPBasedMode(TANumber value, int numberBase, int precision)
    {
        return value switch
        {
            TPNumber number => new TPNumber(number.Value, numberBase, precision),
            TComp complex => new TComp(complex.Re.Value, complex.Im.Value, numberBase, precision),
            _ => throw new InvalidOperationException("Ожидалось p-ичное число или комплексное число на его основе.")
        };
    }

    private void WriteToEditor(TANumber value)
    {
        editor.SetValue(value.ToString() ?? string.Empty);
    }

    private void ClearRepeatedEqualState()
    {
        repeatedEqualOperation = BinaryOperation.None;
        repeatedEqualOperand = null;
    }

    private void UpdateNumberSnapshot()
    {
        try
        {
            number = ParseCurrent();
        }
        catch (FormatException)
        {
            // During editing, Fraction/Complex editors can temporarily hold incomplete values like "1/" or "1;".
            // Keep the last valid snapshot and let strict parsing happen only when the user runs a calculation command.
        }
        catch (ArgumentException)
        {
            // Empty intermediate fragments should not break typing.
        }
    }

    private string ExecuteWithErrorState(Func<string> action)
    {
        try
        {
            var result = action();
            UpdateNumberSnapshot();
            return result;
        }
        catch
        {
            State = TCtrlState.cError;
            throw;
        }
    }

    private void ExecuteWithErrorState(Action action)
    {
        try
        {
            action();
            UpdateNumberSnapshot();
        }
        catch
        {
            State = TCtrlState.cError;
            throw;
        }
    }
}

