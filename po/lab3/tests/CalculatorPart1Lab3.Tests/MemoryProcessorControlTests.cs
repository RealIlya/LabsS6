using CalculatorPart1Lab3.Core;

namespace CalculatorPart1Lab3.Tests;

public class MemoryProcessorControlTests
{
    [Fact]
    public void Memory_StoreAddRead_Works()
    {
        var zero = new TPNumber(0, 10, 4);
        var memory = new TMemory<TPNumber>(zero);

        memory.Store(new TPNumber(5, 10, 4));
        memory.Add(new TPNumber(2, 10, 4));

        Assert.True(memory.IsOn);
        Assert.Equal("7", memory.Read().ToString());
    }

    [Fact]
    public void Processor_RunOperation_AddsOperands()
    {
        var proc = new TProcessor<TPNumber>(new TPNumber(0, 10, 4), new TPNumber(0, 10, 4));
        proc.SetLeft(new TPNumber(4, 10, 4));
        proc.SetRight(new TPNumber(3, 10, 4));
        proc.SetOperation(BinaryOperation.Add);

        var result = proc.RunOperation();
        Assert.Equal("7", result.ToString());
    }

    [Fact]
    public void Control_ExecuteOperationAndEqual_ReturnsResult()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(2);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteEditorCommand(3);
        var result = control.ExecuteEqual();

        Assert.Equal("5", result);
    }

    [Fact]
    public void Control_MemoryStoreRestore_Works()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(9);
        control.ExecuteMemoryCommand(0); // MS
        control.Reset();
        var result = control.ExecuteMemoryCommand(1); // MR

        Assert.Equal("9", result);
    }

    [Fact]
    public void Control_ClipboardCopyPaste_Works()
    {
        var control = new CalculatorControl(16, 4);
        control.ExecuteEditorCommand(10);
        control.ExecuteEditorCommand(11);

        var copied = control.ExecuteClipboardCommand(0);
        var pasted = control.ExecuteClipboardCommand(1, copied);

        Assert.Equal("AB", copied);
        Assert.Equal("AB", pasted);
    }

    [Fact]
    public void Control_SetBase_ConvertsDisplayedValue()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(1);
        control.ExecuteEditorCommand(0);
        control.ExecuteEditorCommand(16);
        control.ExecuteEditorCommand(5);

        control.SetBase(2);

        Assert.Equal("1010.1", control.Display);
    }

    [Fact]
    public void Control_OperationChain_WorksWithoutEqualBetweenOperators()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(2);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteEditorCommand(2);
        control.ExecuteOperation(BinaryOperation.Sub);
        control.ExecuteEditorCommand(3);

        var result = control.ExecuteEqual();

        Assert.Equal("1", result);
    }

    [Fact]
    public void Control_Sqr_ContinuesPendingOperationChain()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(2);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteEditorCommand(3);
        control.ExecuteFunction(UnaryFunction.Sqr);

        var result = control.ExecuteEqual();

        Assert.Equal("11", result);
    }

    [Fact]
    public void Control_Rev_ContinuesPendingOperationChain()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(2);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteEditorCommand(4);
        control.ExecuteFunction(UnaryFunction.Rev);

        var result = control.ExecuteEqual();

        Assert.Equal("2.25", result);
    }

    [Fact]
    public void Control_MemoryRecall_ContinuesPendingOperationChain()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(3);
        control.ExecuteMemoryCommand(0);
        control.Reset();

        control.ExecuteEditorCommand(2);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteMemoryCommand(1);

        var result = control.ExecuteEqual();

        Assert.Equal("5", result);
    }

    [Fact]
    public void Control_SqrAndOperationChain_ProducesExpectedResult()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(2);
        control.ExecuteFunction(UnaryFunction.Sqr);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteEditorCommand(3);
        control.ExecuteFunction(UnaryFunction.Sqr);
        control.ExecuteOperation(BinaryOperation.Dvd);
        control.ExecuteEditorCommand(2);

        var result = control.ExecuteEqual();

        Assert.Equal("6.5", result);
    }

    [Fact]
    public void Control_MemoryAdd_WhenMemoryIsOff_StoresCurrentValue()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(7);

        control.ExecuteMemoryCommand(2);
        control.Reset();

        var result = control.ExecuteMemoryCommand(1);

        Assert.Equal("7", result);
        Assert.True(control.MemoryState);
    }

    [Fact]
    public void Control_MemoryClear_TurnsMemoryOffAndReturnsZero()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(8);
        control.ExecuteMemoryCommand(0);

        control.ExecuteMemoryCommand(3);
        var result = control.ExecuteMemoryCommand(1);

        Assert.Equal("0", result);
        Assert.False(control.MemoryState);
    }

    [Fact]
    public void Control_SetBase_ConvertsMemoryValue()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(1);
        control.ExecuteEditorCommand(0);
        control.ExecuteEditorCommand(16);
        control.ExecuteEditorCommand(5);
        control.ExecuteMemoryCommand(0);

        control.SetBase(2);
        control.Reset();
        var result = control.ExecuteMemoryCommand(1);

        Assert.Equal("1010.1", result);
        Assert.True(control.MemoryState);
    }

    [Fact]
    public void Control_MemoryRecall_UsedInAdditionExpression()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(4);
        control.ExecuteMemoryCommand(0);
        control.Reset();

        control.ExecuteEditorCommand(2);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteMemoryCommand(1);
        var result = control.ExecuteEqual();

        Assert.Equal("6", result);
    }

    [Fact]
    public void Control_MemoryRecall_UsedInDivisionExpression()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(2);
        control.ExecuteMemoryCommand(0);
        control.Reset();

        control.ExecuteEditorCommand(8);
        control.ExecuteOperation(BinaryOperation.Dvd);
        control.ExecuteMemoryCommand(1);
        var result = control.ExecuteEqual();

        Assert.Equal("4", result);
    }

    [Fact]
    public void Control_MemoryAdd_ChangesNextCalculationResult()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(3);
        control.ExecuteMemoryCommand(0);
        control.Reset();
        control.ExecuteEditorCommand(2);
        control.ExecuteMemoryCommand(2);
        control.Reset();

        control.ExecuteEditorCommand(1);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteMemoryCommand(1);
        var result = control.ExecuteEqual();

        Assert.Equal("6", result);
    }

    [Fact]
    public void Control_MemoryRecall_WorksInsideOperationChain()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(5);
        control.ExecuteMemoryCommand(0);
        control.Reset();

        control.ExecuteEditorCommand(2);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteMemoryCommand(1);
        control.ExecuteOperation(BinaryOperation.Mul);
        control.ExecuteEditorCommand(3);
        var result = control.ExecuteEqual();

        Assert.Equal("21", result);
    }

    [Fact]
    public void Control_MemoryRecall_WorksAfterUnaryFunctionInExpression()
    {
        var control = new CalculatorControl(10, 4);
        control.ExecuteEditorCommand(4);
        control.ExecuteMemoryCommand(0);
        control.Reset();

        control.ExecuteEditorCommand(2);
        control.ExecuteFunction(UnaryFunction.Sqr);
        control.ExecuteOperation(BinaryOperation.Add);
        control.ExecuteMemoryCommand(1);
        var result = control.ExecuteEqual();

        Assert.Equal("8", result);
    }
}
