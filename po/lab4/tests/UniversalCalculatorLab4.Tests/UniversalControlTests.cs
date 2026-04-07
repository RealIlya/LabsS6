using UniversalCalculatorLab4.Core;

namespace UniversalCalculatorLab4.Tests;

public class UniversalControlTests
{
    [Fact]
    public void PNumberMode_ChainedOperations_Work()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "2");
        control.ExecuteOperation(BinaryOperation.Add);
        EnterPNumber(control, "2");
        control.ExecuteOperation(BinaryOperation.Sub);
        EnterPNumber(control, "3");

        var result = control.ExecuteEqual();

        Assert.Equal("1", result);
    }

    [Fact]
    public void PNumberMode_FunctionsContinuePendingChain()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "2");
        control.ExecuteFunction(UnaryFunction.Sqr);
        control.ExecuteOperation(BinaryOperation.Add);
        EnterPNumber(control, "3");
        control.ExecuteFunction(UnaryFunction.Sqr);
        control.ExecuteOperation(BinaryOperation.Dvd);
        EnterPNumber(control, "2");

        var result = control.ExecuteEqual();

        Assert.Equal("6.5", result);
    }

    [Fact]
    public void PNumberMode_RepeatedEqual_RepeatsLastOperation()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "5");
        control.ExecuteOperation(BinaryOperation.Add);
        EnterPNumber(control, "4");

        Assert.Equal("9", control.ExecuteEqual());
        Assert.Equal("13", control.ExecuteEqual());
        Assert.Equal("17", control.ExecuteEqual());
    }

    [Fact]
    public void PNumberMode_NewOperationAfterEqual_DoesNotReusePreviousOperation()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "2");
        control.ExecuteOperation(BinaryOperation.Add);
        EnterPNumber(control, "3");
        Assert.Equal("5", control.ExecuteEqual());

        control.ExecuteOperation(BinaryOperation.Mul);
        EnterPNumber(control, "4");

        Assert.Equal("20", control.ExecuteEqual());
    }

    [Fact]
    public void PNumberMode_MemoryRecallCanBeUsedAsOperand()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "3");
        control.ExecuteMemoryCommand(0);
        control.Reset();
        EnterPNumber(control, "2");
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteMemoryCommand(1);

        var result = control.ExecuteEqual();

        Assert.Equal("5", result);
    }

    [Fact]
    public void PNumberMode_ClipboardPasteCanBeUsedAsOperand()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "2");
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteClipboardCommand(1, "3");

        var result = control.ExecuteEqual();

        Assert.Equal("5", result);
    }

    [Fact]
    public void PNumberMode_ChangingSettingsConvertsCurrentDisplayAndMemory()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "10.5");
        control.ExecuteMemoryCommand(0);
        control.SetPNumberSettings(2, 8);

        Assert.Equal("1010.1", control.Display);
        Assert.Equal("1010.1", control.ExecuteMemoryCommand(1));
    }

    [Fact]
    public void PNumberMode_ChangingSettingsKeepsPendingOperation()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "10");
        control.ExecuteOperation(BinaryOperation.Add);
        control.SetPNumberSettings(16, 8);
        EnterPNumber(control, "A");

        var result = control.ExecuteEqual();

        Assert.Equal("14", result);
    }

    [Fact]
    public void FractionMode_ChainedOperations_Work()
    {
        var control = CreateFractionControl();

        EnterFraction(control, "1/2");
        control.ExecuteOperation(BinaryOperation.Add);
        EnterFraction(control, "1/4");
        control.ExecuteOperation(BinaryOperation.Sub);
        EnterFraction(control, "1/8");

        var result = control.ExecuteEqual();

        Assert.Equal("5/8", result);
    }

    [Fact]
    public void FractionMode_FunctionsContinuePendingChain()
    {
        var control = CreateFractionControl();

        EnterFraction(control, "1/2");
        control.ExecuteFunction(UnaryFunction.Rev);
        control.ExecuteOperation(BinaryOperation.Add);
        EnterFraction(control, "1/2");

        var result = control.ExecuteEqual();

        Assert.Equal("5/2", result);
    }

    [Fact]
    public void FractionMode_MemoryRecallCanBeUsedAsOperand()
    {
        var control = CreateFractionControl();

        EnterFraction(control, "1/3");
        control.ExecuteMemoryCommand(0);
        control.Reset();
        EnterFraction(control, "1/6");
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteMemoryCommand(1);

        var result = control.ExecuteEqual();

        Assert.Equal("1/2", result);
    }

    [Fact]
    public void FractionMode_RepeatedEqual_RepeatsLastOperation()
    {
        var control = CreateFractionControl();

        EnterFraction(control, "1/2");
        control.ExecuteOperation(BinaryOperation.Add);
        EnterFraction(control, "1/4");

        Assert.Equal("3/4", control.ExecuteEqual());
        Assert.Equal("1/1", control.ExecuteEqual());
    }

    [Fact]
    public void ComplexMode_ChainedOperations_Work()
    {
        var control = CreateComplexControl();

        EnterComplex(control, "1;2");
        control.ExecuteOperation(BinaryOperation.Add);
        EnterComplex(control, "3;4");
        control.ExecuteOperation(BinaryOperation.Sub);
        EnterComplex(control, "1;1");

        var result = control.ExecuteEqual();

        Assert.Equal("3+i*5", result);
    }

    [Fact]
    public void ComplexMode_FunctionsContinuePendingChain()
    {
        var control = CreateComplexControl();

        EnterComplex(control, "1;2");
        control.ExecuteFunction(UnaryFunction.Sqr);
        control.ExecuteOperation(BinaryOperation.Add);
        EnterComplex(control, "1;0");

        var result = control.ExecuteEqual();

        Assert.Equal("-2+i*4", result);
    }

    [Fact]
    public void ComplexMode_MemoryRecallCanBeUsedAsOperand()
    {
        var control = CreateComplexControl();

        EnterComplex(control, "1;1");
        control.ExecuteMemoryCommand(0);
        control.Reset();
        EnterComplex(control, "2;0");
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteMemoryCommand(1);

        var result = control.ExecuteEqual();

        Assert.Equal("3+i*1", result);
    }

    [Fact]
    public void ComplexMode_EditorCommands_CanEnterFractionalNegativeImaginary()
    {
        var control = CreateComplexControl();

        control.ExecuteCalculatorCommand(CalculatorCommand.Digit1);
        control.ExecuteCalculatorCommand(CalculatorCommand.DecimalSeparator);
        control.ExecuteCalculatorCommand(CalculatorCommand.Digit5);
        control.ExecuteCalculatorCommand(CalculatorCommand.Separator);
        control.ExecuteCalculatorCommand(CalculatorCommand.ToggleImaginarySign);
        control.ExecuteCalculatorCommand(CalculatorCommand.Digit2);
        control.ExecuteCalculatorCommand(CalculatorCommand.DecimalSeparator);
        control.ExecuteCalculatorCommand(CalculatorCommand.Digit2);
        control.ExecuteCalculatorCommand(CalculatorCommand.Digit5);

        Assert.Equal("1.5;-2.25", control.Display);
        Assert.Equal("1.5-i*2.25", control.Number.ToString());
    }

    [Fact]
    public void ComplexMode_ToggleSign_NegatesWholeNumber()
    {
        var control = CreateComplexControl();

        EnterComplex(control, "1;2");
        control.ExecuteCalculatorCommand(CalculatorCommand.ToggleSign);

        Assert.Equal("-1;-2", control.Display);
        Assert.Equal("-1-i*2", control.Number.ToString());
    }

    [Fact]
    public void ComplexMode_ToggleRealSign_NegatesOnlyRealPart()
    {
        var control = CreateComplexControl();

        EnterComplex(control, "1;2");
        control.ExecuteCalculatorCommand(CalculatorCommand.ToggleRealSign);

        Assert.Equal("-1;2", control.Display);
        Assert.Equal("-1+i*2", control.Number.ToString());
    }

    [Fact]
    public void ComplexMode_ChangingSettingsConvertsDisplayAndMemory()
    {
        var control = CreateComplexControl(16, 8);

        EnterComplex(control, "A;F");
        control.ExecuteMemoryCommand(0);
        control.SetPNumberSettings(10, 8);

        Assert.Equal("10+i*15", control.Display);
        Assert.Equal("10+i*15", control.ExecuteMemoryCommand(1));
    }

    [Fact]
    public void ModeSwitch_ResetsDisplayAndMemory()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "9");
        control.ExecuteMemoryCommand(0);
        control.SetMode(NumberMode.Fraction);

        Assert.Equal("0", control.Display);
        Assert.False(control.MemoryState);
        Assert.Equal(UniversalCalculatorControl.TCtrlState.cStart, control.State);
    }

    [Fact]
    public void ExecuteOperation_WhenOperationAlreadySelected_ReplacesOperationWithoutEvaluation()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "2");
        control.ExecuteOperation(BinaryOperation.Add);
        var displayAfterReplace = control.ExecuteOperation(BinaryOperation.Mul);
        EnterPNumber(control, "3");

        var result = control.ExecuteEqual();

        Assert.Equal("2", displayAfterReplace);
        Assert.Equal("6", result);
    }

    [Fact]
    public void Clipboard_CopyPaste_WorksInCurrentMode()
    {
        var control = CreateFractionControl();

        EnterFraction(control, "3/4");

        var copied = control.ExecuteClipboardCommand(0);
        var pasted = control.ExecuteClipboardCommand(1, copied);

        Assert.Equal("3/4", copied);
        Assert.Equal("3/4", pasted);
    }

    [Fact]
    public void ExecuteCalculatorCommand_DispatchesEditorAndEqual()
    {
        var control = CreatePNumberControl();

        control.ExecuteCalculatorCommand(CalculatorCommand.Digit2);
        control.ExecuteCalculatorCommand(CalculatorCommand.Add);
        control.ExecuteCalculatorCommand(CalculatorCommand.Digit3);

        var result = control.ExecuteCalculatorCommand(CalculatorCommand.Equal);

        Assert.Equal("5", result);
    }

    [Fact]
    public void Number_ReturnsCurrentTypedValue()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "10.5");

        Assert.Equal("10.5", control.Number.ToString());
    }

    [Fact]
    public void ExecuteMemoryCommand_WithStateString_UpdatesMemoryState()
    {
        var control = CreatePNumberControl();
        var memoryState = "OFF";

        EnterPNumber(control, "7");
        control.ExecuteMemoryCommand(0, ref memoryState);
        Assert.Equal("ON", memoryState);

        control.ExecuteMemoryCommand(3, ref memoryState);
        Assert.Equal("OFF", memoryState);
    }

    [Fact]
    public void ExecuteClipboardCommand_CopyUpdatesClipboardValue()
    {
        var control = CreateFractionControl();
        var clipboardValue = string.Empty;

        EnterFraction(control, "3/4");

        var result = control.ExecuteClipboardCommand(0, ref clipboardValue);

        Assert.Equal("3/4", result);
        Assert.Equal("3/4", clipboardValue);
    }

    [Fact]
    public void ExecuteCalculatorCommand_WithStateRefs_UpdatesClipboardAndMemoryState()
    {
        var control = CreatePNumberControl();
        var clipboardValue = string.Empty;
        var memoryState = "OFF";

        control.ExecuteCalculatorCommand(CalculatorCommand.Digit2, ref clipboardValue, ref memoryState);
        control.ExecuteCalculatorCommand(CalculatorCommand.MemoryStore, ref clipboardValue, ref memoryState);
        Assert.Equal("ON", memoryState);

        control.ExecuteCalculatorCommand(CalculatorCommand.ClipboardCopy, ref clipboardValue, ref memoryState);

        Assert.Equal("2", clipboardValue);
        Assert.Equal("ON", memoryState);
    }

    [Fact]
    public void States_AreSetAccordingToLastAction()
    {
        var control = CreatePNumberControl();

        EnterPNumber(control, "2");
        Assert.Equal(UniversalCalculatorControl.TCtrlState.cEditing, control.State);

        control.ExecuteOperation(BinaryOperation.Add);
        Assert.Equal(UniversalCalculatorControl.TCtrlState.cOpChange, control.State);

        EnterPNumber(control, "3");
        control.ExecuteEqual();
        Assert.Equal(UniversalCalculatorControl.TCtrlState.cExpDone, control.State);

        control.ExecuteFunction(UnaryFunction.Sqr);
        Assert.Equal(UniversalCalculatorControl.TCtrlState.FunDone, control.State);

        control.ExecuteMemoryCommand(1);
        Assert.Equal(UniversalCalculatorControl.TCtrlState.cValDone, control.State);
    }

    [Fact]
    public void InvalidCommand_SetsErrorState()
    {
        var control = CreatePNumberControl();

        Assert.Throws<ArgumentOutOfRangeException>(() => control.ExecuteCalculatorCommand((CalculatorCommand)999));

        Assert.Equal(UniversalCalculatorControl.TCtrlState.cError, control.State);
    }

    private static UniversalCalculatorControl CreatePNumberControl(int numberBase = 10, int precision = 10)
    {
        var control = new UniversalCalculatorControl();
        control.SetMode(NumberMode.PNumber);
        control.SetPNumberSettings(numberBase, precision);
        return control;
    }

    private static UniversalCalculatorControl CreateFractionControl()
    {
        var control = new UniversalCalculatorControl();
        control.SetMode(NumberMode.Fraction);
        return control;
    }

    private static UniversalCalculatorControl CreateComplexControl(int numberBase = 10, int precision = 10)
    {
        var control = new UniversalCalculatorControl();
        control.SetMode(NumberMode.Complex);
        control.SetPNumberSettings(numberBase, precision);
        return control;
    }

    private static void EnterPNumber(UniversalCalculatorControl control, string text)
    {
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '-':
                    control.ExecuteEditorCommand(20);
                    break;
                case '.':
                    control.ExecuteEditorCommand(16);
                    break;
                default:
                    control.ExecuteEditorCommand(ParseHexDigit(ch));
                    break;
            }
        }
    }

    private static void EnterFraction(UniversalCalculatorControl control, string text)
    {
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '-':
                    control.ExecuteEditorCommand(20);
                    break;
                case '/':
                    control.ExecuteEditorCommand(16);
                    break;
                default:
                    control.ExecuteEditorCommand(ch - '0');
                    break;
            }
        }
    }

    private static void EnterComplex(UniversalCalculatorControl control, string text)
    {
        foreach (var ch in text)
        {
            switch (ch)
            {
                case '-':
                    if (control.Display.Contains(';', StringComparison.Ordinal))
                    {
                        control.ExecuteEditorCommand((int)CalculatorCommand.ToggleImaginarySign);
                    }
                    else
                    {
                        control.ExecuteEditorCommand(20);
                    }
                    break;
                case '.':
                    control.ExecuteEditorCommand((int)CalculatorCommand.DecimalSeparator);
                    break;
                case ';':
                    control.ExecuteEditorCommand(16);
                    break;
                default:
                    control.ExecuteEditorCommand(ParseHexDigit(ch));
                    break;
            }
        }
    }

    private static int ParseHexDigit(char ch)
    {
        if (ch >= '0' && ch <= '9')
        {
            return ch - '0';
        }

        if (ch >= 'A' && ch <= 'F')
        {
            return ch - 'A' + 10;
        }

        throw new ArgumentOutOfRangeException(nameof(ch), "Поддерживаются только цифры 0..9 и A..F.");
    }
}
