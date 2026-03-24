# Отчёт по лабораторной работе №3

## 1. Задание

Разработать приложение «Калькулятор» для выполнения вычислений над p-ичными числами с использованием объектно-ориентированной модели и графического интерфейса.

Приложение должно обеспечивать:

- ввод чисел и команд мышью и с клавиатуры;
- операции `+`, `-`, `*`, `/`, функции `Rev` и `Sqr`;
- выбор основания системы счисления в диапазоне `2..16`;
- работу с памятью `MC`, `MR`, `MS`, `M+`;
- операции копирования и вставки через меню `Правка`;
- вывод результата по правому краю и отображение состояния памяти;
- всплывающие подсказки компонентов и окно справки.

## 2. Ход выполнения

В ходе выполнения лабораторной работы разработан калькулятор p-ичных чисел с разделением на ядро вычислений и графический интерфейс. В ядре реализованы тип `TPNumber` для представления действительных чисел в системе счисления с основанием от `2` до `16`, редактор `TPNumberEditor`, процессор `TProcessor<TPNumber>`, память `TMemory<TPNumber>` и класс управления `CalculatorControl`.

Класс `TPNumber` выполняет хранение значения, арифметические операции, перевод в строковое p-ичное представление и разбор строкового ввода. Класс `TPNumberEditor` обеспечивает пошаговое редактирование вводимого числа: добавление цифр, знака, разделителя, удаление последнего символа и очистку.

Класс `TProcessor<TPNumber>` реализует выполнение бинарных операций и функций, а `TMemory<TPNumber>` хранит одно значение памяти и поддерживает команды сохранения, чтения, добавления и очистки. Класс `CalculatorControl` связывает редактор, процессор и память, хранит состояние калькулятора и предоставляет интерфейс команд для UI.

Графический интерфейс реализован в `Form1` на WinForms. Форма содержит поле вывода результата, настройку основания, индикатор памяти, меню `Правка`, `Настройка`, `Справка`, а также набор командных кнопок в стиле стандартного калькулятора Windows. Справка загружается из отдельного файла `help.txt`, а для пользовательских сообщений об ошибках используется отдельный маппер `UiErrorMapper`.

Для проверки корректности реализации подготовлены автоматические тесты для типов `TPNumber`, `TPNumberEditor`, `TMemory<TPNumber>`, `TProcessor<TPNumber>` и `CalculatorControl`. Тесты покрывают преобразование представлений, арифметику, редактирование, память, операции копирования/вставки и перевод отображаемого числа при смене основания системы счисления.

## 3. Диаграмма классов

На рис. 1 приведена упрощённая диаграмма классов разработанного приложения.

![Диаграмма классов приложения](C:/Users/Admin/Desktop/LabsS6/po/lab3/report/assets/class-diagram.png)

Рис. 1. Диаграмма классов приложения «Калькулятор p-ичных чисел»

## 4. Текст программы

### ICalcNumber.cs

```csharp
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
```

### BinaryOperation.cs

```csharp
namespace CalculatorPart1Lab3.Core;

public enum BinaryOperation
{
    None = 0,
    Add = 1,
    Sub = 2,
    Mul = 3,
    Dvd = 4
}
```

### UnaryFunction.cs

```csharp
namespace CalculatorPart1Lab3.Core;

public enum UnaryFunction
{
    Rev = 0,
    Sqr = 1
}
```

### TPNumber.cs

```csharp
using System.Text;

namespace CalculatorPart1Lab3.Core;

public sealed class TPNumber : ICalcNumber<TPNumber>
{
    public TPNumber(double value = 0, int numberBase = 10, int precision = 10)
    {
        NumberBase = CheckBase(numberBase);
        Precision = CheckPrecision(precision);
        Value = value;
    }

    public TPNumber(string text, int numberBase = 10, int precision = 10)
    {
        NumberBase = CheckBase(numberBase);
        Precision = CheckPrecision(precision);
        Value = Parse(text, NumberBase);
    }

    public double Value { get; }
    public int NumberBase { get; }
    public int Precision { get; }

    public bool IsZero()
    {
        return Math.Abs(Value) < 1e-12;
    }

    public TPNumber Copy()
    {
        return new TPNumber(Value, NumberBase, Precision);
    }

    public TPNumber Add(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        return new TPNumber(Value + checkedOther.Value, NumberBase, Precision);
    }

    public TPNumber Sub(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        return new TPNumber(Value - checkedOther.Value, NumberBase, Precision);
    }

    public TPNumber Mul(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        return new TPNumber(Value * checkedOther.Value, NumberBase, Precision);
    }

    public TPNumber Div(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        if (checkedOther.IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TPNumber(Value / checkedOther.Value, NumberBase, Precision);
    }

    public bool EqualsTo(TPNumber other)
    {
        var checkedOther = EnsureCompatible(other);
        return Math.Abs(Value - checkedOther.Value) < 1e-12;
    }

    public TPNumber Sqr()
    {
        return new TPNumber(Value * Value, NumberBase, Precision);
    }

    public TPNumber Rev()
    {
        if (IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TPNumber(1.0 / Value, NumberBase, Precision);
    }

    public TPNumber Negate()
    {
        return new TPNumber(-Value, NumberBase, Precision);
    }

    public TPNumber WithBase(int numberBase)
    {
        return new TPNumber(Value, numberBase, Precision);
    }

    public override string ToString()
    {
        var sign = Value < 0 ? "-" : string.Empty;
        var abs = Math.Abs(Value);
        var intPart = (long)Math.Floor(abs);
        var fracPart = abs - intPart;

        var intText = IntToBase(intPart, NumberBase);
        var fracText = FracToBase(fracPart, NumberBase, Precision);

        if (fracText.Length == 0)
        {
            return sign + intText;
        }

        return sign + intText + "." + fracText;
    }

    public static double Parse(string text, int numberBase)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Входная строка пуста.", nameof(text));
        }

        CheckBase(numberBase);

        var source = text.Trim();
        var sign = 1.0;
        if (source.StartsWith("-", StringComparison.Ordinal))
        {
            sign = -1.0;
            source = source[1..];
        }

        var parts = source.Split('.');
        if (parts.Length > 2)
        {
            throw new FormatException("Некорректный формат числа.");
        }

        var intPart = parts[0];
        var fracPart = parts.Length == 2 ? parts[1] : string.Empty;

        var intValue = 0.0;
        foreach (var ch in intPart)
        {
            var digit = CharToDigit(ch);
            if (digit >= numberBase)
            {
                throw new FormatException($"Цифра '{ch}' недопустима для основания {numberBase}.");
            }

            intValue = intValue * numberBase + digit;
        }

        var fracValue = 0.0;
        var weight = 1.0 / numberBase;
        foreach (var ch in fracPart)
        {
            var digit = CharToDigit(ch);
            if (digit >= numberBase)
            {
                throw new FormatException($"Цифра '{ch}' недопустима для основания {numberBase}.");
            }

            fracValue += digit * weight;
            weight /= numberBase;
        }

        return sign * (intValue + fracValue);
    }

    private static int CheckBase(int numberBase)
    {
        if (numberBase < 2 || numberBase > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(numberBase), "Основание должно быть в диапазоне 2..16.");
        }

        return numberBase;
    }

    private static int CheckPrecision(int precision)
    {
        if (precision < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), "Точность должна быть неотрицательной.");
        }

        return precision;
    }

    private static string IntToBase(long value, int numberBase)
    {
        if (value == 0)
        {
            return "0";
        }

        var builder = new StringBuilder();
        var current = value;
        while (current > 0)
        {
            var digit = (int)(current % numberBase);
            builder.Insert(0, DigitToChar(digit));
            current /= numberBase;
        }

        return builder.ToString();
    }

    private static string FracToBase(double frac, int numberBase, int precision)
    {
        if (precision == 0 || frac == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        var current = frac;
        for (var i = 0; i < precision; i++)
        {
            current *= numberBase;
            var digit = (int)Math.Floor(current);
            builder.Append(DigitToChar(digit));
            current -= digit;

            if (current == 0)
            {
                break;
            }
        }

        return builder.ToString();
    }

    private static int CharToDigit(char ch)
    {
        if (ch >= '0' && ch <= '9')
        {
            return ch - '0';
        }

        if (ch >= 'A' && ch <= 'F')
        {
            return ch - 'A' + 10;
        }

        if (ch >= 'a' && ch <= 'f')
        {
            return ch - 'a' + 10;
        }

        throw new FormatException($"Недопустимая цифра '{ch}'.");
    }

    private static char DigitToChar(int digit)
    {
        return digit < 10 ? (char)('0' + digit) : (char)('A' + (digit - 10));
    }

    private TPNumber EnsureCompatible(TPNumber other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other), "Сравниваемое число не должно быть null.");
        }

        if (NumberBase != other.NumberBase || Precision != other.Precision)
        {
            throw new InvalidOperationException("Числа имеют разные основание или точность.");
        }

        return other;
    }
}
```

### TPNumberEditor.cs

```csharp
namespace CalculatorPart1Lab3.Core;

public sealed class TPNumberEditor
{
    private string value = "0";
    private int numberBase;

    public TPNumberEditor(int numberBase = 10)
    {
        NumberBase = numberBase;
    }

    public int NumberBase
    {
        get => numberBase;
        private set
        {
            if (value is < 2 or > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Основание должно быть в диапазоне 2..16.");
            }

            numberBase = value;
        }
    }

    public string Value => value;

    public void SetBase(int numberBase)
    {
        NumberBase = numberBase;
        Clear();
    }

    public bool IsZero()
    {
        return value is "0" or "-0";
    }

    public string ToggleSign()
    {
        if (value.StartsWith("-", StringComparison.Ordinal))
        {
            value = value[1..];
            if (value.Length == 0)
            {
                value = "0";
            }

            return value;
        }

        value = "-" + value;
        return value;
    }

    public string AddDigit(int digit)
    {
        if (digit < 0 || digit >= NumberBase)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), $"Цифра должна быть в диапазоне 0..{NumberBase - 1}.");
        }

        var ch = digit < 10
            ? ((char)('0' + digit)).ToString()
            : ((char)('A' + (digit - 10))).ToString();

        if (value == "0")
        {
            value = ch;
        }
        else if (value == "-0")
        {
            value = "-" + ch;
        }
        else
        {
            value += ch;
        }

        return value;
    }

    public string AddZero()
    {
        if (!IsZero())
        {
            value += "0";
        }

        return value;
    }

    public string AddSeparator()
    {
        if (!value.Contains('.', StringComparison.Ordinal))
        {
            value += ".";
        }

        return value;
    }

    public string Backspace()
    {
        if (value.Length <= 1)
        {
            value = "0";
            return value;
        }

        value = value[..^1];
        if (value is "-" or "")
        {
            value = "0";
        }

        return value;
    }

    public string Clear()
    {
        value = "0";
        return value;
    }

    public string Edit(int command)
    {
        return command switch
        {
            0 => AddZero(),
            >= 1 and <= 15 => AddDigit(command),
            16 => AddSeparator(),
            17 => Backspace(),
            18 => Clear(),
            20 => ToggleSign(),
            _ => value
        };
    }
}
```

### TProcessor.cs

```csharp
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
```

### TMemory.cs

```csharp
namespace CalculatorPart1Lab3.Core;

public sealed class TMemory<T> where T : ICalcNumber<T>
{
    public TMemory(T defaultValue)
    {
        if (defaultValue is null)
        {
            throw new ArgumentNullException(nameof(defaultValue), "Значение по умолчанию не должно быть null.");
        }

        number = defaultValue.Copy();
    }

    private T number;

    public bool IsOn { get; private set; }

    public void Store(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Сохраняемое значение не должно быть null.");
        }

        number = value.Copy();
        IsOn = true;
    }

    public void Add(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Добавляемое значение не должно быть null.");
        }

        number = number.Add(value);
        IsOn = !number.IsZero();
    }

    public T Read()
    {
        return number.Copy();
    }

    public void Clear(T zeroValue)
    {
        if (zeroValue is null)
        {
            throw new ArgumentNullException(nameof(zeroValue), "Нулевое значение не должно быть null.");
        }

        number = zeroValue.Copy();
        IsOn = false;
    }
}
```

### CalculatorControl.cs

```csharp
﻿namespace CalculatorPart1Lab3.Core;

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
```

### UiErrorMapper.cs

```csharp
namespace CalculatorPart1Lab3.WinForms;

internal static class UiErrorMapper
{
    public static string ToUserMessage(Exception ex)
    {
        return ex switch
        {
            DivideByZeroException => "Деление на ноль недопустимо.",
            FormatException => "Некорректный формат числа для текущего основания.",
            ArgumentOutOfRangeException { ParamName: "digit" } => "Введена цифра вне диапазона текущего основания.",
            ArgumentOutOfRangeException { ParamName: "numberBase" } => "Основание должно быть в диапазоне 2..16.",
            ArgumentOutOfRangeException { ParamName: "precision" } => "Точность должна быть неотрицательной.",
            ArgumentOutOfRangeException { ParamName: "command" } => "Команда калькулятора не поддерживается.",
            InvalidOperationException => "Операция недоступна в текущем состоянии калькулятора.",
            ArgumentException => "Проверьте корректность введённых данных.",
            _ => "Произошла непредвиденная ошибка. Повторите действие."
        };
    }
}
```

### Form1.cs

```csharp
﻿using CalculatorPart1Lab3.Core;
using System.Drawing;
using System.Windows.Forms;

namespace CalculatorPart1Lab3.WinForms;

public partial class Form1 : Form
{
    private const string HelpFileName = "help.txt";

    private readonly CalculatorControl control = new();
    private readonly TextBox display = new();
    private readonly NumericUpDown baseSelector = new();
    private readonly List<Button> digitButtons = new();
    private readonly Label memoryIndicator = new();
    private readonly ToolTip hints = new();
    private readonly Dictionary<int, ToolStripMenuItem> baseMenuItems = new();

    public Form1()
    {
        InitializeComponent();
        BuildUi();
        RefreshUi();
    }

    private void BuildUi()
    {
        Text = "Калькулятор p-ичных чисел";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(485, 460);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        KeyPreview = true;
        KeyDown += OnFormKeyDown;

        var menu = BuildMenu();
        Controls.Add(menu);

        display.SetBounds(15, 40, 455, 36);
        display.ReadOnly = true;
        display.Font = new Font("Consolas", 16f);
        display.TextAlign = HorizontalAlignment.Right;

        var baseLabel = new Label { Text = "Основание", Left = 15, Top = 88, AutoSize = true };
        baseSelector.SetBounds(90, 85, 70, 26);
        baseSelector.Minimum = 2;
        baseSelector.Maximum = 16;
        baseSelector.Value = 10;
        baseSelector.ValueChanged += (_, _) => ApplyBase((int)baseSelector.Value);

        memoryIndicator.SetBounds(190, 88, 80, 22);
        memoryIndicator.Text = "M: OFF";

        hints.SetToolTip(display, "Строка результата текущего выражения");
        hints.SetToolTip(baseSelector, "Основание системы счисления (2..16)");
        hints.SetToolTip(memoryIndicator, "Состояние памяти калькулятора");

        Controls.Add(display);
        Controls.Add(baseLabel);
        Controls.Add(baseSelector);
        Controls.Add(memoryIndicator);

        BuildButtons();
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();

        var editMenu = new ToolStripMenuItem("Правка");
        var copyItem = new ToolStripMenuItem("Копировать", null, (_, _) => CopyToClipboard())
        {
            ShortcutKeys = Keys.Control | Keys.C
        };
        var pasteItem = new ToolStripMenuItem("Вставить", null, (_, _) => PasteFromClipboard())
        {
            ShortcutKeys = Keys.Control | Keys.V
        };
        editMenu.DropDownItems.Add(copyItem);
        editMenu.DropDownItems.Add(pasteItem);

        var settingsMenu = new ToolStripMenuItem("Настройка");
        AddBaseMenuItem(settingsMenu, 2);
        AddBaseMenuItem(settingsMenu, 8);
        AddBaseMenuItem(settingsMenu, 10);
        AddBaseMenuItem(settingsMenu, 16);

        var helpMenu = new ToolStripMenuItem("Справка");
        var aboutItem = new ToolStripMenuItem("О программе", null, (_, _) => ShowHelp());
        helpMenu.DropDownItems.Add(aboutItem);

        menu.Items.Add(editMenu);
        menu.Items.Add(settingsMenu);
        menu.Items.Add(helpMenu);
        MainMenuStrip = menu;

        return menu;
    }

    private void AddBaseMenuItem(ToolStripMenuItem settingsMenu, int numberBase)
    {
        var item = new ToolStripMenuItem($"Основание {numberBase}")
        {
            CheckOnClick = true
        };
        item.Click += (_, _) => baseSelector.Value = numberBase;
        settingsMenu.DropDownItems.Add(item);
        baseMenuItems[numberBase] = item;
    }

    private void BuildButtons()
    {
        var panel = new TableLayoutPanel
        {
            Left = 15,
            Top = 120,
            Width = 455,
            Height = 320,
            ColumnCount = 6,
            RowCount = 7
        };

        for (var i = 0; i < panel.ColumnCount; i++)
        {
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / panel.ColumnCount));
        }

        for (var i = 0; i < panel.RowCount; i++)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / panel.RowCount));
        }

        AddCommandButton(panel, "MC", 0, 0, (_, _) => RunMemory(3), "Очистить память");
        AddCommandButton(panel, "MR", 1, 0, (_, _) => RunMemory(1), "Прочитать из памяти");
        AddCommandButton(panel, "MS", 2, 0, (_, _) => RunMemory(0), "Записать в память");
        AddCommandButton(panel, "M+", 3, 0, (_, _) => RunMemory(2), "Прибавить к памяти");
        AddCommandButton(panel, "CL", 4, 1, (_, _) => ResetAll(), "Сбросить состояние");
        AddCommandButton(panel, "BS", 5, 1, (_, _) => RunEditor(17), "Удалить последний символ");

        AddCommandButton(panel, "A", 0, 1, (_, _) => RunEditor(10), "Цифра A", true);
        AddCommandButton(panel, "B", 0, 2, (_, _) => RunEditor(11), "Цифра B", true);
        AddCommandButton(panel, "C", 0, 3, (_, _) => RunEditor(12), "Цифра C", true);
        AddCommandButton(panel, "D", 0, 4, (_, _) => RunEditor(13), "Цифра D", true);
        AddCommandButton(panel, "E", 0, 5, (_, _) => RunEditor(14), "Цифра E", true);
        AddCommandButton(panel, "F", 0, 6, (_, _) => RunEditor(15), "Цифра F", true);

        AddCommandButton(panel, "/", 4, 2, (_, _) => RunOperation(BinaryOperation.Dvd), "Операция деления");
        AddCommandButton(panel, "Rev", 5, 2, (_, _) => RunFunction(UnaryFunction.Rev), "Обратное значение (1/x)");

        AddCommandButton(panel, "7", 1, 3, (_, _) => RunEditor(7), "Цифра 7", true);
        AddCommandButton(panel, "8", 2, 3, (_, _) => RunEditor(8), "Цифра 8", true);
        AddCommandButton(panel, "9", 3, 3, (_, _) => RunEditor(9), "Цифра 9", true);

        AddCommandButton(panel, "4", 1, 4, (_, _) => RunEditor(4), "Цифра 4", true);
        AddCommandButton(panel, "5", 2, 4, (_, _) => RunEditor(5), "Цифра 5", true);
        AddCommandButton(panel, "6", 3, 4, (_, _) => RunEditor(6), "Цифра 6", true);
        AddCommandButton(panel, "*", 4, 3, (_, _) => RunOperation(BinaryOperation.Mul), "Операция умножения");
        AddCommandButton(panel, "Sqr", 5, 3, (_, _) => RunFunction(UnaryFunction.Sqr), "Квадрат текущего числа");

        AddCommandButton(panel, "1", 1, 5, (_, _) => RunEditor(1), "Цифра 1", true);
        AddCommandButton(panel, "2", 2, 5, (_, _) => RunEditor(2), "Цифра 2", true);
        AddCommandButton(panel, "3", 3, 5, (_, _) => RunEditor(3), "Цифра 3", true);
        AddCommandButton(panel, "-", 4, 4, (_, _) => RunOperation(BinaryOperation.Sub), "Операция вычитания");

        AddCommandButton(panel, "+/-", 1, 6, (_, _) => RunEditor(20), "Сменить знак числа");
        AddCommandButton(panel, "0", 2, 6, (_, _) => RunEditor(0), "Цифра 0", true);
        AddCommandButton(panel, ".", 3, 6, (_, _) => RunEditor(16), "Разделитель целой и дробной части");
        AddCommandButton(panel, "+", 4, 5, (_, _) => RunOperation(BinaryOperation.Add), "Операция сложения");
        var equalButton = AddCommandButton(panel, "=", 4, 6, (_, _) => RunEqual(), "Вычислить выражение");
        panel.SetColumnSpan(equalButton, 2);
        ApplyAccentStyle(equalButton);

        Controls.Add(panel);
        UpdateDigitButtons();
    }

    private Button AddCommandButton(
        TableLayoutPanel panel,
        string text,
        int col,
        int row,
        EventHandler onClick,
        string hint,
        bool isDigit = false)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            Font = new Font("Segoe UI", 10f)
        };
        button.Click += onClick;
        hints.SetToolTip(button, hint);
        panel.Controls.Add(button, col, row);

        if (isDigit)
        {
            digitButtons.Add(button);
        }

        return button;
    }

    private static void ApplyAccentStyle(Button button)
    {
        button.UseVisualStyleBackColor = false;
        button.BackColor = Color.FromArgb(62, 163, 255);
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(28, 120, 204);
    }

    private void ApplyBase(int numberBase)
    {
        control.SetBase(numberBase);
        RefreshUi();
    }

    private void RunEditor(int command)
    {
        ExecuteSafely(() => control.ExecuteEditorCommand(command));
    }

    private void RunOperation(BinaryOperation operation)
    {
        ExecuteSafely(() => control.ExecuteOperation(operation));
    }

    private void RunFunction(UnaryFunction function)
    {
        ExecuteSafely(() => control.ExecuteFunction(function));
    }

    private void RunEqual()
    {
        ExecuteSafely(control.ExecuteEqual);
    }

    private void RunMemory(int command)
    {
        ExecuteSafely(() => control.ExecuteMemoryCommand(command));
    }

    private void ResetAll()
    {
        ExecuteSafely(control.Reset);
    }

    private void CopyToClipboard()
    {
        ExecuteSafely(() =>
        {
            var value = control.ExecuteClipboardCommand(0);
            Clipboard.SetText(value);
            return value;
        });
    }

    private void PasteFromClipboard()
    {
        ExecuteSafely(() =>
        {
            var value = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
            return control.ExecuteClipboardCommand(1, value);
        });
    }

    private void ExecuteSafely(Func<string> action)
    {
        try
        {
            action();
            RefreshUi();
        }
        catch (Exception ex)
        {
            MessageBox.Show(UiErrorMapper.ToUserMessage(ex), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RefreshUi()
    {
        display.Text = control.Display;
        memoryIndicator.Text = control.MemoryState ? "M: ON" : "M: OFF";
        UpdateBaseMenuChecks();
        UpdateDigitButtons();
    }

    private void UpdateBaseMenuChecks()
    {
        foreach (var pair in baseMenuItems)
        {
            pair.Value.Checked = pair.Key == (int)baseSelector.Value;
        }
    }

    private void UpdateDigitButtons()
    {
        foreach (var button in digitButtons)
        {
            var text = button.Text;
            int digit;
            if (text.Length == 1 && char.IsDigit(text[0]))
            {
                digit = text[0] - '0';
            }
            else
            {
                digit = text[0] - 'A' + 10;
            }

            button.Enabled = digit < (int)baseSelector.Value;
        }
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.C)
        {
            CopyToClipboard();
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control && e.KeyCode == Keys.V)
        {
            PasteFromClipboard();
            e.SuppressKeyPress = true;
            return;
        }

        if (TryHandleDigitKey(e.KeyCode, out var digit))
        {
            RunEditor(digit);
            e.SuppressKeyPress = true;
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Decimal:
            case Keys.OemPeriod:
                RunEditor(16);
                e.SuppressKeyPress = true;
                break;
            case Keys.Back:
                RunEditor(17);
                e.SuppressKeyPress = true;
                break;
            case Keys.Delete:
            case Keys.Escape:
                ResetAll();
                e.SuppressKeyPress = true;
                break;
            case Keys.Enter:
                RunEqual();
                e.SuppressKeyPress = true;
                break;
            case Keys.Add:
                RunOperation(BinaryOperation.Add);
                e.SuppressKeyPress = true;
                break;
            case Keys.Subtract:
            case Keys.OemMinus:
                RunOperation(BinaryOperation.Sub);
                e.SuppressKeyPress = true;
                break;
            case Keys.Multiply:
                RunOperation(BinaryOperation.Mul);
                e.SuppressKeyPress = true;
                break;
            case Keys.Divide:
            case Keys.OemQuestion:
                RunOperation(BinaryOperation.Dvd);
                e.SuppressKeyPress = true;
                break;
            case Keys.Oemplus when e.Shift:
                RunOperation(BinaryOperation.Add);
                e.SuppressKeyPress = true;
                break;
        }
    }

    private static bool TryHandleDigitKey(Keys key, out int digit)
    {
        digit = -1;

        if (key >= Keys.D0 && key <= Keys.D9)
        {
            digit = key - Keys.D0;
            return true;
        }

        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
        {
            digit = key - Keys.NumPad0;
            return true;
        }

        if (key >= Keys.A && key <= Keys.F)
        {
            digit = key - Keys.A + 10;
            return true;
        }

        return false;
    }

    private static void ShowHelp()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, HelpFileName);
            if (!File.Exists(path))
            {
                MessageBox.Show($"Файл справки не найден: {path}", "Справка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var text = File.ReadAllText(path);
            MessageBox.Show(text, "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(UiErrorMapper.ToUserMessage(ex), "Справка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
```

### Program.cs

```csharp
namespace CalculatorPart1Lab3.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}
```

### CalculatorPart1Lab3.WinForms.csproj

```xml
﻿<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\src\CalculatorPart1Lab3.Core\CalculatorPart1Lab3.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="help.txt">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
```

### help.txt

```text
Калькулятор p-ичных чисел

Основание 2..16, операции +, -, *, /, Sqr, Rev, память MC/MR/MS/M+ и буфер обмена.
История в этой версии не используется.

Бригада №2:
Весёлый Денис
Ворончук Илья
Лыкова Мария
```

## 5. Тестовые наборы данных и результаты тестирования

| № | Тестовый сценарий | Входные данные | Ожидаемый результат |
|---|---|---|---|
| 1 | Разбор p-ичного числа | `A.8`, основание `16` | Десятичное значение `10.5` |
| 2 | Формирование двоичной записи | `10.5`, основание `2`, точность `4` | Строка `1010.1` |
| 3 | Сложение чисел | `5 + 7` в основании `10` | Результат `12` |
| 4 | Проверка `Rev` | `0` | Генерируется ошибка деления на ноль |
| 5 | Редактирование числа | Добавление `A` в редакторе с основанием `16` | Значение редактора `A` |
| 6 | Повторное добавление разделителя | Ввод `0.` и повторная команда разделителя | Значение остаётся `0.` |
| 7 | Смена знака | Двойная команда `+/-` для `0` | Итоговое значение `0` |
| 8 | Память калькулятора | `MS(5)`, затем `M+(2)`, `MR` | Из памяти читается `7` |
| 9 | Выполнение операции | Ввод `2 + 3 =` | На дисплее `5` |
| 10 | Буфер обмена | Копирование `AB`, затем вставка | На дисплее `AB` |
| 11 | Смена основания системы счисления | Ввод `10.5` при основании `10`, затем переключение на основание `2` | На дисплее `1010.1` |

Результат автоматического тестирования:

```text
dotnet test lab3/tests/CalculatorPart1Lab3.Tests/CalculatorPart1Lab3.Tests.csproj
Пройден! : не пройдено 0, пройдено 13, пропущено 0, всего 13.
```
