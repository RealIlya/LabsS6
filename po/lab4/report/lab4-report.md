# Отчёт по лабораторной работе №4

## 1. Задание

Разработать приложение «Универсальный калькулятор» для выполнения вычислений над `p`-ичными числами, простыми дробями и комплексными числами с использованием объектно-ориентированной модели и графического интерфейса.

Приложение должно обеспечивать:

- ввод чисел и команд мышью и с клавиатуры;
- работу в режимах `p`-ичных чисел, простых дробей и комплексных чисел;
- операции `+`, `-`, `*`, `/`, функции `Rev` и `Sqr`;
- работу с памятью `MC`, `MR`, `MS`, `M+`;
- операции копирования и вставки через меню `Правка`;
- переключение вида чисел через меню `Вид`;
- отображение результата и состояния памяти;
- окно справки и всплывающие подсказки компонентов.

## 2. Ход выполнения

В ходе выполнения лабораторной работы разработан универсальный калькулятор, объединяющий три режима вычислений: `p`-ичные числа, простые дроби и комплексные числа. Архитектура приложения разделена на ядро вычислений и графический интерфейс.

В ядре реализована иерархия числовых типов с абстрактным базовым классом `TANumber` и конкретными реализациями `TPNumber`, `TFrac` и `TComp`. Для каждого вида чисел реализованы арифметические операции, сравнение, вычисление квадрата, обратного значения и формирование строкового представления.

Для ввода и редактирования чисел разработана иерархия редакторов: `AEditor`, `PNumberEditor`, `FractionEditor` и `ComplexEditor`. Комплексный редактор построен на основе двух `PNumberEditor`, что позволяет редактировать действительную и мнимую части по единым правилам и учитывать основание системы счисления в комплексном режиме.

Вычислительное ядро дополнено классами `TProcessor`, `TMemory` и `UniversalCalculatorControl`. Класс `UniversalCalculatorControl` связывает редактор, процессор, память и буфер обмена, хранит состояние калькулятора, распределяет команды между подсистемами и поддерживает цепочки операций, повторное выполнение по `=`, работу памяти и буфера обмена.

Графический интерфейс реализован в `Form1` на WinForms. Форма содержит строку результата, индикатор памяти, выбор вида числа, настройку основания, командные кнопки, меню `Правка`, `Вид`, `Справка`, а также отдельные элементы для ввода комплексных чисел. Справка загружается из отдельного файла `help.txt`, а пользовательские ошибки показываются через `UiErrorMapper`.

Для проверки корректности реализации подготовлены автоматические тесты для числовой иерархии, редакторов, памяти, процессора, управления калькулятором и формы. Тесты покрывают типовые и граничные сценарии для всех трёх режимов, включая память, буфер обмена, смену параметров `p`-ичных чисел и ввод комплексных чисел с дробными компонентами.

## 3. Диаграмма классов

На рис. 1 приведена упрощённая диаграмма классов разработанного приложения.

![Диаграмма классов приложения](C:/Users/Admin/Desktop/LabsS6/po/lab4/report/assets/class-diagram.png)

Рис. 1. Диаграмма классов приложения «Универсальный калькулятор»

## 4. Текст программы

### BinaryOperation.cs

```csharp
namespace UniversalCalculatorLab4.Core;

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
namespace UniversalCalculatorLab4.Core;

public enum UnaryFunction
{
    Rev = 0,
    Sqr = 1
}
```

### NumberMode.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public enum NumberMode
{
    PNumber = 0,
    Fraction = 1,
    Complex = 2
}
```

### CalculatorCommand.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public enum CalculatorCommand
{
    Digit0 = 0,
    Digit1 = 1,
    Digit2 = 2,
    Digit3 = 3,
    Digit4 = 4,
    Digit5 = 5,
    Digit6 = 6,
    Digit7 = 7,
    Digit8 = 8,
    Digit9 = 9,
    DigitA = 10,
    DigitB = 11,
    DigitC = 12,
    DigitD = 13,
    DigitE = 14,
    DigitF = 15,
    Separator = 16,
    Backspace = 17,
    EditorClear = 18,
    DecimalSeparator = 19,
    ToggleSign = 20,
    ToggleImaginarySign = 21,
    ToggleRealSign = 22,

    Add = 101,
    Sub = 102,
    Mul = 103,
    Divide = 104,

    Rev = 201,
    Sqr = 202,

    Equal = 301,
    Reset = 302,

    MemoryStore = 401,
    MemoryRecall = 402,
    MemoryAdd = 403,
    MemoryClear = 404,

    ClipboardCopy = 501,
    ClipboardPaste = 502
}
```

### TANumber.cs

```csharp
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
```

### TPNumber.cs

```csharp
using System.Text;

namespace UniversalCalculatorLab4.Core;

public sealed class TPNumber : TANumber
{
    public TPNumber(double value = 0, int numberBase = 10, int precision = 10)
    {
        Value = value;
        NumberBase = ValidateBase(numberBase);
        Precision = ValidatePrecision(precision);
    }

    public TPNumber(string text, int numberBase = 10, int precision = 10)
    {
        NumberBase = ValidateBase(numberBase);
        Precision = ValidatePrecision(precision);
        Value = Parse(text, NumberBase);
    }

    public double Value { get; }
    public int NumberBase { get; }
    public int Precision { get; }

    public override bool IsZero() => Math.Abs(Value) < 1e-12;
    public override TANumber Copy() => new TPNumber(Value, NumberBase, Precision);
    public override TANumber Negate() => new TPNumber(-Value, NumberBase, Precision);
    public override TANumber Sqr() => new TPNumber(Value * Value, NumberBase, Precision);

    public override TANumber Rev()
    {
        if (IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TPNumber(1.0 / Value, NumberBase, Precision);
    }

    public override TANumber Add(TANumber other)
    {
        var b = CheckType(other);
        return new TPNumber(Value + b.Value, NumberBase, Precision);
    }

    public override TANumber Sub(TANumber other)
    {
        var b = CheckType(other);
        return new TPNumber(Value - b.Value, NumberBase, Precision);
    }

    public override TANumber Mul(TANumber other)
    {
        var b = CheckType(other);
        return new TPNumber(Value * b.Value, NumberBase, Precision);
    }

    public override TANumber Div(TANumber other)
    {
        var b = CheckType(other);
        if (b.IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TPNumber(Value / b.Value, NumberBase, Precision);
    }

    public override bool EqualsTo(TANumber other)
    {
        var b = CheckType(other);
        return Math.Abs(Value - b.Value) < 1e-12;
    }

    public override string ToString()
    {
        var sign = Value < 0 ? "-" : string.Empty;
        var abs = Math.Abs(Value);
        var intPart = (long)Math.Floor(abs);
        var fracPart = abs - intPart;
        var intText = IntToBase(intPart, NumberBase);
        var fracText = FracToBase(fracPart, NumberBase, Precision);
        return fracText.Length == 0 ? sign + intText : sign + intText + "." + fracText;
    }

    public static double Parse(string text, int numberBase)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Входная строка пуста.", nameof(text));
        }

        ValidateBase(numberBase);

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
            var d = CharToDigit(ch);
            if (d >= numberBase)
            {
                throw new FormatException($"Цифра '{ch}' недопустима для основания {numberBase}.");
            }

            intValue = intValue * numberBase + d;
        }

        var fracValue = 0.0;
        var w = 1.0 / numberBase;
        foreach (var ch in fracPart)
        {
            var d = CharToDigit(ch);
            if (d >= numberBase)
            {
                throw new FormatException($"Цифра '{ch}' недопустима для основания {numberBase}.");
            }

            fracValue += d * w;
            w /= numberBase;
        }

        return sign * (intValue + fracValue);
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
        if (frac == 0 || precision == 0)
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

    private TPNumber CheckType(TANumber other)
    {
        if (other is not TPNumber n)
        {
            throw new InvalidOperationException("Разные типы чисел.");
        }

        if (n.NumberBase != NumberBase || n.Precision != Precision)
        {
            throw new InvalidOperationException("Разные параметры p-чисел.");
        }

        return n;
    }

    private static int ValidateBase(int numberBase)
    {
        if (numberBase < 2 || numberBase > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(numberBase), "Основание должно быть в диапазоне 2..16.");
        }

        return numberBase;
    }

    private static int ValidatePrecision(int precision)
    {
        if (precision < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), "Точность должна быть неотрицательной.");
        }

        return precision;
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
}
```

### TFrac.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public sealed class TFrac : TANumber
{
    public TFrac(long numerator = 0, long denominator = 1)
    {
        if (denominator == 0)
        {
            throw new DivideByZeroException("Знаменатель не должен быть равен нулю.");
        }

        var sign = denominator < 0 ? -1 : 1;
        var n = numerator * sign;
        var d = Math.Abs(denominator);
        var gcd = Gcd(Math.Abs(n), d);
        Numerator = n / gcd;
        Denominator = d / gcd;
    }

    public TFrac(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Входная строка пуста.", nameof(text));
        }

        var parts = text.Trim().Split('/');
        if (parts.Length != 2)
        {
            throw new FormatException("Дробь должна быть в формате a/b.");
        }

        var n = long.Parse(parts[0]);
        var d = long.Parse(parts[1]);
        if (d == 0)
        {
            throw new DivideByZeroException("Знаменатель не должен быть равен нулю.");
        }

        var normalized = new TFrac(n, d);
        Numerator = normalized.Numerator;
        Denominator = normalized.Denominator;
    }

    public long Numerator { get; }
    public long Denominator { get; }

    public override bool IsZero() => Numerator == 0;
    public override TANumber Copy() => new TFrac(Numerator, Denominator);
    public override TANumber Negate() => new TFrac(-Numerator, Denominator);
    public override TANumber Sqr() => new TFrac(Numerator * Numerator, Denominator * Denominator);

    public override TANumber Rev()
    {
        if (IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TFrac(Denominator, Numerator);
    }

    public override TANumber Add(TANumber other)
    {
        var b = CheckType(other);
        var n = Numerator * b.Denominator + b.Numerator * Denominator;
        var d = Denominator * b.Denominator;
        return new TFrac(n, d);
    }

    public override TANumber Sub(TANumber other)
    {
        var b = CheckType(other);
        var n = Numerator * b.Denominator - b.Numerator * Denominator;
        var d = Denominator * b.Denominator;
        return new TFrac(n, d);
    }

    public override TANumber Mul(TANumber other)
    {
        var b = CheckType(other);
        return new TFrac(Numerator * b.Numerator, Denominator * b.Denominator);
    }

    public override TANumber Div(TANumber other)
    {
        var b = CheckType(other);
        if (b.IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        return new TFrac(Numerator * b.Denominator, Denominator * b.Numerator);
    }

    public override bool EqualsTo(TANumber other)
    {
        var b = CheckType(other);
        return Numerator == b.Numerator && Denominator == b.Denominator;
    }

    public override string ToString()
    {
        return $"{Numerator}/{Denominator}";
    }

    private TFrac CheckType(TANumber other)
    {
        if (other is not TFrac value)
        {
            throw new InvalidOperationException("Разные типы чисел.");
        }

        return value;
    }

    private static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            var t = a % b;
            a = b;
            b = t;
        }

        return a == 0 ? 1 : a;
    }
}
```

### TComp.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public sealed class TComp : TANumber
{
    public TComp(TPNumber realPart, TPNumber imaginaryPart)
    {
        Re = (TPNumber)(realPart ?? throw new ArgumentNullException(nameof(realPart), "Действительная часть не должна быть null.")).Copy();
        Im = (TPNumber)(imaginaryPart ?? throw new ArgumentNullException(nameof(imaginaryPart), "Мнимая часть не должна быть null.")).Copy();
        ValidateParameters(Re, Im);
    }

    public TComp(double realPart = 0, double imaginaryPart = 0, int numberBase = 10, int precision = 10)
        : this(
            new TPNumber(realPart, numberBase, precision),
            new TPNumber(imaginaryPart, numberBase, precision))
    {
    }

    public TComp(string text, int numberBase = 10, int precision = 10)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Входная строка пуста.", nameof(text));
        }

        var source = text.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (source.Contains(';', StringComparison.Ordinal))
        {
            var parts = source.Split(';');
            if (parts.Length != 2)
            {
                throw new FormatException("Некорректный формат комплексного числа.");
            }

            Re = new TPNumber(parts[0], numberBase, precision);
            Im = new TPNumber(parts[1], numberBase, precision);
            return;
        }

        var markerIndex = FindImaginaryMarker(source);
        if (markerIndex < 0)
        {
            throw new FormatException("Комплексное число должно быть в формате a;b, a+i*b или a-i*b.");
        }

        var realText = source[..markerIndex];
        var sign = source[markerIndex];
        var imaginaryMagnitude = source[(markerIndex + 3)..];
        if (realText.Length == 0 || imaginaryMagnitude.Length == 0)
        {
            throw new FormatException("Комплексное число должно быть в формате a;b, a+i*b или a-i*b.");
        }

        Re = new TPNumber(realText, numberBase, precision);
        Im = new TPNumber(
            sign == '-'
                ? "-" + imaginaryMagnitude
                : imaginaryMagnitude,
            numberBase,
            precision);
    }

    public TPNumber Re { get; }
    public TPNumber Im { get; }

    public override bool IsZero() => Re.IsZero() && Im.IsZero();

    public override TANumber Copy()
    {
        return new TComp(Re, Im);
    }

    public override TANumber Negate()
    {
        return new TComp(
            (TPNumber)Re.Negate(),
            (TPNumber)Im.Negate());
    }

    public override TANumber Add(TANumber other)
    {
        var value = CheckType(other);
        return new TComp(
            (TPNumber)Re.Add(value.Re),
            (TPNumber)Im.Add(value.Im));
    }

    public override TANumber Sub(TANumber other)
    {
        var value = CheckType(other);
        return new TComp(
            (TPNumber)Re.Sub(value.Re),
            (TPNumber)Im.Sub(value.Im));
    }

    public override TANumber Mul(TANumber other)
    {
        var value = CheckType(other);
        var realPart = Re.Value * value.Re.Value - Im.Value * value.Im.Value;
        var imaginaryPart = Re.Value * value.Im.Value + Im.Value * value.Re.Value;
        return new TComp(realPart, imaginaryPart, Re.NumberBase, Re.Precision);
    }

    public override TANumber Div(TANumber other)
    {
        var value = CheckType(other);
        var denominator = value.Re.Value * value.Re.Value + value.Im.Value * value.Im.Value;
        if (Math.Abs(denominator) < 1e-12)
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        var realPart = (Re.Value * value.Re.Value + Im.Value * value.Im.Value) / denominator;
        var imaginaryPart = (Im.Value * value.Re.Value - Re.Value * value.Im.Value) / denominator;
        return new TComp(realPart, imaginaryPart, Re.NumberBase, Re.Precision);
    }

    public override bool EqualsTo(TANumber other)
    {
        var value = CheckType(other);
        return Re.EqualsTo(value.Re) && Im.EqualsTo(value.Im);
    }

    public override TANumber Sqr()
    {
        return Mul(this);
    }

    public override TANumber Rev()
    {
        if (IsZero())
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        var denominator = Re.Value * Re.Value + Im.Value * Im.Value;
        return new TComp(
            Re.Value / denominator,
            -Im.Value / denominator,
            Re.NumberBase,
            Re.Precision);
    }

    public override string ToString()
    {
        var sign = Im.Value < 0 ? "-" : "+";
        var imaginaryAbs = new TPNumber(Math.Abs(Im.Value), Im.NumberBase, Im.Precision);
        return $"{Re}{sign}i*{imaginaryAbs}";
    }

    private TComp CheckType(TANumber other)
    {
        if (other is not TComp value)
        {
            throw new InvalidOperationException("Разные типы чисел.");
        }

        ValidateParameters(Re, value.Re);
        ValidateParameters(Im, value.Im);
        return value;
    }

    private static int FindImaginaryMarker(string source)
    {
        for (var i = 1; i <= source.Length - 3; i++)
        {
            if ((source[i] == '+' || source[i] == '-')
                && source[i + 1] == 'i'
                && source[i + 2] == '*')
            {
                return i;
            }
        }

        return -1;
    }

    private static void ValidateParameters(TPNumber first, TPNumber second)
    {
        if (first.NumberBase != second.NumberBase || first.Precision != second.Precision)
        {
            throw new InvalidOperationException("Разные параметры p-чисел.");
        }
    }
}
```

### AEditor.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public abstract class AEditor
{
    protected string value = "0";

    public string Value => value;

    public virtual bool IsZero() => value is "0" or "-0";

    public virtual string Clear()
    {
        value = "0";
        return value;
    }

    public virtual string Backspace()
    {
        if (value.Length <= 1)
        {
            return Clear();
        }

        value = value[..^1];
        if (value is "-" or "")
        {
            value = "0";
        }

        return value;
    }

    public virtual string ToggleSign()
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

    public virtual void SetValue(string text)
    {
        value = string.IsNullOrWhiteSpace(text) ? "0" : text;
    }

    public abstract string AddDigit(int digit);
    public abstract string AddZero();
    public abstract string AddSeparator();

    public virtual string Edit(int command)
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

### PNumberEditor.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public sealed class PNumberEditor : AEditor
{
    private int numberBase;

    public PNumberEditor(int numberBase = 10)
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

    public void SetBase(int numberBase)
    {
        NumberBase = numberBase;
        Clear();
    }

    public override string AddDigit(int digit)
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

    public override string AddZero()
    {
        if (!IsZero())
        {
            value += "0";
        }

        return value;
    }

    public override string AddSeparator()
    {
        if (!value.Contains('.', StringComparison.Ordinal))
        {
            value += ".";
        }

        return value;
    }
}
```

### FractionEditor.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public sealed class FractionEditor : AEditor
{
    public override string AddDigit(int digit)
    {
        if (digit < 0 || digit > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), "Цифры дробного редактора должны быть в диапазоне 0..9.");
        }

        var ch = ((char)('0' + digit)).ToString();

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

    public override string AddZero()
    {
        if (!IsZero())
        {
            value += "0";
        }

        return value;
    }

    public override string AddSeparator()
    {
        if (!value.Contains('/', StringComparison.Ordinal))
        {
            value += "/";
        }

        return value;
    }
}
```

### ComplexEditor.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public sealed class ComplexEditor : AEditor
{
    private readonly PNumberEditor realEditor;
    private readonly PNumberEditor imaginaryEditor;
    private string realText = "0";
    private string imaginaryText = string.Empty;
    private bool hasImaginaryPart;
    private bool editingImaginary;

    public ComplexEditor(int numberBase = 10)
    {
        realEditor = new PNumberEditor(numberBase);
        imaginaryEditor = new PNumberEditor(numberBase);
        SyncValue();
    }

    public int NumberBase => realEditor.NumberBase;

    public void SetBase(int numberBase)
    {
        realEditor.SetBase(numberBase);
        imaginaryEditor.SetBase(numberBase);
        Clear();
    }

    public override bool IsZero()
    {
        return IsZeroComponent(realText) && (!hasImaginaryPart || IsZeroComponent(imaginaryText));
    }

    public override string AddDigit(int digit)
    {
        if (digit < 0 || digit >= NumberBase)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), $"Цифра должна быть в диапазоне 0..{NumberBase - 1}.");
        }

        ApplyToActiveComponent(editor => editor.AddDigit(digit));
        return value;
    }

    public override string AddZero()
    {
        ApplyToActiveComponent(editor => editor.AddZero());
        return value;
    }

    public override string AddSeparator()
    {
        if (!hasImaginaryPart)
        {
            hasImaginaryPart = true;
            imaginaryText = string.Empty;
            editingImaginary = true;
            SyncValue();
        }
        else
        {
            editingImaginary = true;
        }

        return value;
    }

    public override string ToggleSign()
    {
        realText = ToggleComponentSign(realText, realEditor, allowEmpty: false);
        if (hasImaginaryPart && imaginaryText.Length > 0)
        {
            imaginaryText = ToggleComponentSign(imaginaryText, imaginaryEditor, allowEmpty: true);
        }

        SyncValue();
        return value;
    }

    public override string Backspace()
    {
        if (hasImaginaryPart)
        {
            if (imaginaryText.Length > 0)
            {
                imaginaryText = BackspaceComponent(imaginaryText, allowEmpty: true);
            }
            else
            {
                hasImaginaryPart = false;
                editingImaginary = false;
            }

            SyncValue();
            return value;
        }

        realText = BackspaceComponent(realText, allowEmpty: false);
        SyncValue();
        return value;
    }

    public override string Clear()
    {
        realText = "0";
        imaginaryText = string.Empty;
        hasImaginaryPart = false;
        editingImaginary = false;
        SyncValue();
        return value;
    }

    public override void SetValue(string text)
    {
        var source = string.IsNullOrWhiteSpace(text) ? "0" : text.Trim();
        value = source;
        TrySeedEditorsFromText(source);
    }

    public override string Edit(int command)
    {
        return command switch
        {
            0 => AddZero(),
            >= 1 and <= 15 => AddDigit(command),
            16 => AddSeparator(),
            17 => Backspace(),
            18 => Clear(),
            19 => AddDecimalSeparator(),
            20 => ToggleSign(),
            21 => ToggleImaginarySign(),
            22 => ToggleRealSign(),
            _ => value
        };
    }

    public string AddDecimalSeparator()
    {
        ApplyToActiveComponent(editor => editor.AddSeparator());
        return value;
    }

    public string ToggleImaginarySign()
    {
        if (!hasImaginaryPart)
        {
            hasImaginaryPart = true;
        }

        editingImaginary = true;
        imaginaryText = ToggleComponentSign(imaginaryText, imaginaryEditor, allowEmpty: true);
        SyncValue();
        return value;
    }

    public string ToggleRealSign()
    {
        realText = ToggleComponentSign(realText, realEditor, allowEmpty: false);
        SyncValue();
        return value;
    }

    private void ApplyToActiveComponent(Func<PNumberEditor, string> action)
    {
        if (editingImaginary)
        {
            imaginaryText = ApplyToComponent(imaginaryEditor, imaginaryText, action);
        }
        else
        {
            realText = ApplyToComponent(realEditor, realText, action);
        }

        SyncValue();
    }

    private static string ApplyToComponent(PNumberEditor editor, string componentText, Func<PNumberEditor, string> action)
    {
        editor.SetValue(componentText.Length == 0 ? "0" : componentText);
        return action(editor);
    }

    private static string ToggleComponentSign(string componentText, PNumberEditor editor, bool allowEmpty)
    {
        editor.SetValue(componentText.Length == 0 ? "0" : componentText);
        var toggled = editor.ToggleSign();
        if (allowEmpty && componentText.Length == 0 && toggled == "0")
        {
            return string.Empty;
        }

        return toggled;
    }

    private void SyncValue()
    {
        value = hasImaginaryPart
            ? $"{realText};{imaginaryText}"
            : realText;
    }

    private void TrySeedEditorsFromText(string source)
    {
        try
        {
            if (source.Contains(';', StringComparison.Ordinal))
            {
                var parts = source.Split(';');
                if (parts.Length == 2)
                {
                    realText = parts[0];
                    imaginaryText = parts[1];
                    hasImaginaryPart = true;
                    editingImaginary = imaginaryText.Length > 0;
                    return;
                }
            }

            if (source.Contains("i*", StringComparison.Ordinal))
            {
                var complex = new TComp(source, NumberBase, 10);
                realText = complex.Re.ToString();
                imaginaryText = complex.Im.ToString();
                hasImaginaryPart = true;
                editingImaginary = false;
                return;
            }

            realText = source;
            imaginaryText = string.Empty;
            hasImaginaryPart = false;
            editingImaginary = false;
        }
        catch (Exception) when (source.Length > 0)
        {
            // Result strings or incomplete input should still remain displayable even if
            // they are not meant to be edited further in the current state.
            realText = source;
            imaginaryText = string.Empty;
            hasImaginaryPart = false;
            editingImaginary = false;
        }
    }

    private static string BackspaceComponent(string componentText, bool allowEmpty)
    {
        if (componentText.Length <= 1)
        {
            return allowEmpty ? string.Empty : "0";
        }

        var shortened = componentText[..^1];
        if (shortened == "-")
        {
            return allowEmpty ? string.Empty : "0";
        }

        return shortened;
    }

    private static bool IsZeroComponent(string componentText)
    {
        return componentText is "" or "-" or "0" or "-0" or "0." or "-0.";
    }
}
```

### TProcessor.cs

```csharp
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
```

### TMemory.cs

```csharp
namespace UniversalCalculatorLab4.Core;

public sealed class TMemory
{
    private TANumber number;

    public TMemory(TANumber defaultNumber)
    {
        if (defaultNumber is null)
        {
            throw new ArgumentNullException(nameof(defaultNumber), "Значение по умолчанию не должно быть null.");
        }

        number = defaultNumber.Copy();
    }

    public bool IsOn { get; private set; }

    public void Store(TANumber value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Сохраняемое значение не должно быть null.");
        }

        number = value.Copy();
        IsOn = true;
    }

    public TANumber Read()
    {
        return number.Copy();
    }

    public void Add(TANumber value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Добавляемое значение не должно быть null.");
        }

        number = number.Add(value);
        IsOn = true;
    }

    public void Clear(TANumber defaultNumber)
    {
        if (defaultNumber is null)
        {
            throw new ArgumentNullException(nameof(defaultNumber), "Нулевое значение не должно быть null.");
        }

        number = defaultNumber.Copy();
        IsOn = false;
    }
}
```

### UniversalCalculatorControl.cs

```csharp
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
```

### UiErrorMapper.cs

```csharp
namespace UniversalCalculatorLab4.WinForms;

internal static class UiErrorMapper
{
    public static string ToUserMessage(Exception ex)
    {
        return ex switch
        {
            DivideByZeroException => "Деление на ноль недопустимо.",
            FormatException => "Некорректный формат числа для выбранного режима.",
            ArgumentOutOfRangeException { ParamName: "digit" } => "Введена цифра вне допустимого диапазона.",
            ArgumentOutOfRangeException { ParamName: "numberBase" } => "Основание должно быть в диапазоне 2..16.",
            ArgumentOutOfRangeException { ParamName: "precision" } => "Точность должна быть неотрицательной.",
            ArgumentOutOfRangeException { ParamName: "command" } => "Команда калькулятора не поддерживается.",
            InvalidOperationException => "Операция недоступна для текущего режима.",
            ArgumentException => "Проверьте корректность введённых данных.",
            _ => "Произошла непредвиденная ошибка. Повторите действие."
        };
    }
}
```

### Form1.cs

```csharp
using System.Drawing;
using System.Windows.Forms;
using UniversalCalculatorLab4.Core;

namespace UniversalCalculatorLab4.WinForms;

public partial class Form1 : Form
{
    private const string HelpFileName = "help.txt";

    private readonly UniversalCalculatorControl control = new();
    private readonly TextBox display = new();
    private readonly ComboBox modeSelector = new();
    private readonly NumericUpDown baseSelector = new();
    private readonly List<Button> digitButtons = new();
    private readonly Label separatorHint = new();
    private readonly Label memoryIndicator = new();
    private readonly ToolTip hints = new();
    private readonly Dictionary<NumberMode, ToolStripMenuItem> modeMenuItems = new();
    private readonly Button complexDecimalButton = new();
    private readonly Button complexImaginarySignButton = new();
    private readonly Button complexRealSignButton = new();
    private string clipboardValue = string.Empty;
    private string memoryState = "OFF";

    public Form1()
    {
        InitializeComponent();
        BuildUi();
        RefreshUi();
    }

    private void BuildUi()
    {
        Text = "Универсальный калькулятор";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(620, 500);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        KeyPreview = true;
        KeyDown += OnFormKeyDown;
        KeyPress += OnFormKeyPress;
        Shown += (_, _) => ActiveControl = null;

        var menu = BuildMenu();
        Controls.Add(menu);

        display.SetBounds(15, 40, 590, 36);
        display.ReadOnly = true;
        display.TabStop = false;
        display.Font = new Font("Consolas", 15f);
        display.TextAlign = HorizontalAlignment.Right;

        var modeLabel = new Label { Text = "Вид", Left = 15, Top = 88, AutoSize = true };
        modeSelector.SetBounds(70, 85, 180, 26);
        modeSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        modeSelector.Items.AddRange(["Р-ичное", "Простая дробь", "Комплексное"]);
        modeSelector.SelectedIndex = 0;
        modeSelector.SelectedIndexChanged += (_, _) => ApplyMode((NumberMode)modeSelector.SelectedIndex);

        var baseLabel = new Label { Text = "Основание", Left = 270, Top = 88, AutoSize = true };
        baseSelector.SetBounds(350, 85, 70, 26);
        baseSelector.Minimum = 2;
        baseSelector.Maximum = 16;
        baseSelector.Value = 10;
        baseSelector.ValueChanged += (_, _) =>
        {
            control.SetPNumberSettings((int)baseSelector.Value, 10);
            RefreshUi();
        };

        separatorHint.SetBounds(440, 88, 90, 22);
        memoryIndicator.SetBounds(540, 88, 70, 22);

        hints.SetToolTip(display, "Строка результата текущего выражения");
        hints.SetToolTip(modeSelector, "Вид чисел для вычислений");
        hints.SetToolTip(baseSelector, "Основание p-ичной системы (2..16)");

        Controls.Add(display);
        Controls.Add(modeLabel);
        Controls.Add(modeSelector);
        Controls.Add(baseLabel);
        Controls.Add(baseSelector);
        Controls.Add(separatorHint);
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

        var viewMenu = new ToolStripMenuItem("Вид");
        AddModeMenuItem(viewMenu, NumberMode.PNumber, "Р-ичное");
        AddModeMenuItem(viewMenu, NumberMode.Fraction, "Простая дробь");
        AddModeMenuItem(viewMenu, NumberMode.Complex, "Комплексное");

        var helpMenu = new ToolStripMenuItem("Справка");
        var aboutItem = new ToolStripMenuItem("О программе", null, (_, _) => ShowHelp());
        helpMenu.DropDownItems.Add(aboutItem);

        menu.Items.Add(editMenu);
        menu.Items.Add(viewMenu);
        menu.Items.Add(helpMenu);
        MainMenuStrip = menu;

        return menu;
    }

    private void AddModeMenuItem(ToolStripMenuItem viewMenu, NumberMode mode, string text)
    {
        var item = new ToolStripMenuItem(text)
        {
            CheckOnClick = true
        };
        item.Click += (_, _) => modeSelector.SelectedIndex = (int)mode;
        viewMenu.DropDownItems.Add(item);
        modeMenuItems[mode] = item;
    }

    private void BuildButtons()
    {
        var panel = new TableLayoutPanel
        {
            Left = 15,
            Top = 120,
            Width = 590,
            Height = 360,
            ColumnCount = 8,
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

        AddButton(panel, "MC", 0, 0, CalculatorCommand.MemoryClear, "Очистить память");
        AddButton(panel, "MR", 1, 0, CalculatorCommand.MemoryRecall, "Прочитать из памяти");
        AddButton(panel, "MS", 2, 0, CalculatorCommand.MemoryStore, "Записать в память");
        AddButton(panel, "M+", 3, 0, CalculatorCommand.MemoryAdd, "Прибавить к памяти");
        AddButton(panel, "CL", 4, 0, CalculatorCommand.Reset, "Сбросить состояние");
        AddButton(panel, "BS", 5, 0, CalculatorCommand.Backspace, "Удалить последний символ");
        var equalButton = AddButton(panel, "=", 6, 0, CalculatorCommand.Equal, "Вычислить выражение");
        panel.SetColumnSpan(equalButton, 2);
        ApplyAccentStyle(equalButton);

        AddButton(panel, "A", 0, 1, CalculatorCommand.DigitA, "Цифра A", true);
        ConfigureOptionalButton(complexDecimalButton, panel, ".", 1, 1, CalculatorCommand.DecimalSeparator, "Десятичный разделитель компонента комплексного числа");
        ConfigureOptionalButton(complexImaginarySignButton, panel, "Im+/-", 2, 1, CalculatorCommand.ToggleImaginarySign, "Сменить знак мнимой части");
        ConfigureOptionalButton(complexRealSignButton, panel, "Re+/-", 3, 1, CalculatorCommand.ToggleRealSign, "Сменить знак действительной части");
        complexImaginarySignButton.Font = new Font("Segoe UI", 8.5f);
        complexRealSignButton.Font = new Font("Segoe UI", 8.5f);
        AddButton(panel, "B", 0, 2, CalculatorCommand.DigitB, "Цифра B", true);
        AddButton(panel, "C", 0, 3, CalculatorCommand.DigitC, "Цифра C", true);
        AddButton(panel, "D", 0, 4, CalculatorCommand.DigitD, "Цифра D", true);
        AddButton(panel, "E", 0, 5, CalculatorCommand.DigitE, "Цифра E", true);
        AddButton(panel, "F", 0, 6, CalculatorCommand.DigitF, "Цифра F", true);

        AddButton(panel, "7", 1, 2, CalculatorCommand.Digit7, "Цифра 7", true);
        AddButton(panel, "8", 2, 2, CalculatorCommand.Digit8, "Цифра 8", true);
        AddButton(panel, "9", 3, 2, CalculatorCommand.Digit9, "Цифра 9", true);
        AddButton(panel, "/", 4, 2, CalculatorCommand.Divide, "Операция деления");
        AddButton(panel, "Rev", 5, 2, CalculatorCommand.Rev, "Обратное значение (1/x)");

        AddButton(panel, "4", 1, 3, CalculatorCommand.Digit4, "Цифра 4", true);
        AddButton(panel, "5", 2, 3, CalculatorCommand.Digit5, "Цифра 5", true);
        AddButton(panel, "6", 3, 3, CalculatorCommand.Digit6, "Цифра 6", true);
        AddButton(panel, "*", 4, 3, CalculatorCommand.Mul, "Операция умножения");
        AddButton(panel, "Sqr", 5, 3, CalculatorCommand.Sqr, "Квадрат текущего числа");

        AddButton(panel, "1", 1, 4, CalculatorCommand.Digit1, "Цифра 1", true);
        AddButton(panel, "2", 2, 4, CalculatorCommand.Digit2, "Цифра 2", true);
        AddButton(panel, "3", 3, 4, CalculatorCommand.Digit3, "Цифра 3", true);
        AddButton(panel, "-", 4, 4, CalculatorCommand.Sub, "Операция вычитания");

        AddButton(panel, "+/-", 1, 5, CalculatorCommand.ToggleSign, "Сменить знак числа");
        AddButton(panel, "0", 2, 5, CalculatorCommand.Digit0, "Цифра 0", true);
        AddButton(panel, "SEP", 3, 5, CalculatorCommand.Separator, "Разделитель для текущего режима");
        AddButton(panel, "+", 4, 5, CalculatorCommand.Add, "Операция сложения");

        Controls.Add(panel);
    }

    private Button AddButton(
        TableLayoutPanel panel,
        string text,
        int col,
        int row,
        CalculatorCommand command,
        string hint,
        bool isDigit = false)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            Font = new Font("Segoe UI", 10f),
            TabStop = false
        };
        button.Click += (_, _) =>
        {
            RunCommand(command);
            ActiveControl = null;
        };
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

    private void ApplyMode(NumberMode mode)
    {
        control.SetMode(mode);
        RefreshUi();
    }

    private void ConfigureOptionalButton(
        Button button,
        TableLayoutPanel panel,
        string text,
        int col,
        int row,
        CalculatorCommand command,
        string hint)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(3);
        button.Font = new Font("Segoe UI", 10f);
        button.TabStop = false;
        button.Visible = false;
        button.Click += (_, _) =>
        {
            RunCommand(command);
            ActiveControl = null;
        };
        hints.SetToolTip(button, hint);
        panel.Controls.Add(button, col, row);
    }

    private void RunCommand(CalculatorCommand command)
    {
        ExecuteSafely(() => control.ExecuteCalculatorCommand(command, ref clipboardValue, ref memoryState));
    }

    private void RunPasteCommand(string? clipboardValue)
    {
        this.clipboardValue = clipboardValue ?? string.Empty;
        ExecuteSafely(() => control.ExecuteCalculatorCommand(CalculatorCommand.ClipboardPaste, ref this.clipboardValue, ref memoryState));
    }

    private void CopyToClipboard()
    {
        ExecuteSafely(() =>
        {
            var text = control.ExecuteCalculatorCommand(CalculatorCommand.ClipboardCopy, ref clipboardValue, ref memoryState);
            Clipboard.SetText(clipboardValue);
            return text;
        });
    }

    private void PasteFromClipboard()
    {
        var text = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
        RunPasteCommand(text);
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
        memoryState = control.MemoryStateText;
        memoryIndicator.Text = $"M: {memoryState}";
        baseSelector.Enabled = control.Mode is NumberMode.PNumber or NumberMode.Complex;
        separatorHint.Text = control.Mode switch
        {
            NumberMode.PNumber => "SEP = .",
            NumberMode.Fraction => "SEP = /",
            _ => "SEP = ;, . = p-дробь"
        };
        complexDecimalButton.Visible = control.Mode == NumberMode.Complex;
        complexImaginarySignButton.Visible = control.Mode == NumberMode.Complex;
        complexRealSignButton.Visible = control.Mode == NumberMode.Complex;

        foreach (var pair in modeMenuItems)
        {
            pair.Value.Checked = pair.Key == control.Mode;
        }

        UpdateDigitButtons();
    }

    private void UpdateDigitButtons()
    {
        var maxDigit = control.Mode is NumberMode.PNumber or NumberMode.Complex
            ? (int)baseSelector.Value - 1
            : 9;
        foreach (var button in digitButtons)
        {
            var digit = char.IsDigit(button.Text[0]) ? button.Text[0] - '0' : button.Text[0] - 'A' + 10;
            button.Enabled = digit <= maxDigit;
        }
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.C)
        {
            CopyToClipboard();
            ConsumeKey(e);
            return;
        }

        if (e.Control && e.KeyCode == Keys.V)
        {
            PasteFromClipboard();
            ConsumeKey(e);
            return;
        }

        if (TryHandleNumericPadDigitKey(e.KeyCode, out var digit))
        {
            RunCommand((CalculatorCommand)digit);
            ConsumeKey(e);
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Back:
                RunCommand(CalculatorCommand.Backspace);
                ConsumeKey(e);
                break;
            case Keys.Add:
                RunCommand(CalculatorCommand.Add);
                ConsumeKey(e);
                break;
            case Keys.Subtract:
                HandleMinusFromKeyboard();
                ConsumeKey(e);
                break;
            case Keys.Multiply:
                RunCommand(CalculatorCommand.Mul);
                ConsumeKey(e);
                break;
            case Keys.Divide:
                RunCommand(CalculatorCommand.Divide);
                ConsumeKey(e);
                break;
        }
    }

    private void OnFormKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (TryHandleDigitChar(e.KeyChar, out var digit))
        {
            RunCommand((CalculatorCommand)digit);
            e.Handled = true;
            return;
        }

        switch (e.KeyChar)
        {
            case '.':
            case ',':
                if (control.Mode == NumberMode.PNumber)
                {
                    RunCommand(CalculatorCommand.Separator);
                    e.Handled = true;
                }
                else if (control.Mode == NumberMode.Complex)
                {
                    RunCommand(CalculatorCommand.DecimalSeparator);
                    e.Handled = true;
                }
                break;
            case '/':
                if (control.Mode == NumberMode.Fraction)
                {
                    RunCommand(CalculatorCommand.Separator);
                }
                else
                {
                    RunCommand(CalculatorCommand.Divide);
                }
                e.Handled = true;
                break;
            case ';':
                if (control.Mode == NumberMode.Complex)
                {
                    RunCommand(CalculatorCommand.Separator);
                    e.Handled = true;
                }
                break;
            case '+':
                RunCommand(CalculatorCommand.Add);
                e.Handled = true;
                break;
            case '-':
                HandleMinusFromKeyboard();
                e.Handled = true;
                break;
            case '*':
                RunCommand(CalculatorCommand.Mul);
                e.Handled = true;
                break;
            case '=':
                RunCommand(CalculatorCommand.Equal);
                e.Handled = true;
                break;
        }
    }

    private bool TryHandleDigitChar(char ch, out int digit)
    {
        digit = -1;

        if (ch >= '0' && ch <= '9')
        {
            digit = ch - '0';
            return true;
        }

        if (control.Mode is not (NumberMode.PNumber or NumberMode.Complex))
        {
            return false;
        }

        if (ch >= 'A' && ch <= 'F')
        {
            digit = ch - 'A' + 10;
            return true;
        }

        if (ch >= 'a' && ch <= 'f')
        {
            digit = ch - 'a' + 10;
            return true;
        }

        return false;
    }

    private static bool TryHandleNumericPadDigitKey(Keys key, out int digit)
    {
        digit = -1;

        if (key < Keys.NumPad0 || key > Keys.NumPad9)
        {
            return false;
        }

        digit = key - Keys.NumPad0;
        return true;
    }

    private static void ConsumeKey(KeyEventArgs e)
    {
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private void HandleMinusFromKeyboard()
    {
        if (ShouldToggleImaginarySign())
        {
            RunCommand(CalculatorCommand.ToggleImaginarySign);
            return;
        }

        if (control.State is UniversalCalculatorControl.TCtrlState.cStart
            or UniversalCalculatorControl.TCtrlState.cOpChange
            or UniversalCalculatorControl.TCtrlState.cError
            || control.Display is "0" or "-0")
        {
            RunCommand(CalculatorCommand.ToggleSign);
            return;
        }

        RunCommand(CalculatorCommand.Sub);
    }

    private bool ShouldToggleImaginarySign()
    {
        if (control.Mode != NumberMode.Complex)
        {
            return false;
        }

        if (control.State is UniversalCalculatorControl.TCtrlState.cOpChange
            or UniversalCalculatorControl.TCtrlState.cExpDone
            or UniversalCalculatorControl.TCtrlState.cValDone
            or UniversalCalculatorControl.TCtrlState.FunDone
            or UniversalCalculatorControl.TCtrlState.cError)
        {
            return false;
        }

        var separatorIndex = control.Display.IndexOf(';');
        if (separatorIndex < 0)
        {
            return false;
        }

        var imaginaryPart = control.Display[(separatorIndex + 1)..];
        return imaginaryPart.Length == 0
            || imaginaryPart is "0" or "-0" or "-"
            || imaginaryPart.EndsWith(".", StringComparison.Ordinal);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Enter)
        {
            RunCommand(CalculatorCommand.Equal);
            return true;
        }

        if (keyData == Keys.Escape)
        {
            RunCommand(CalculatorCommand.Reset);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
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
namespace UniversalCalculatorLab4.WinForms;

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

### UniversalCalculatorLab4.WinForms.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\src\UniversalCalculatorLab4.Core\UniversalCalculatorLab4.Core.csproj" />
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

  <ItemGroup>
    <Compile Remove="build\**\*.cs" />
    <EmbeddedResource Remove="build\**\*" />
    <None Remove="build\**\*" />
  </ItemGroup>

</Project>
```

### help.txt

```text
Универсальный калькулятор

Поддерживаются режимы: p-ичные числа, простые дроби и комплексные числа.
Операции: +, -, *, /, Sqr, Rev, память MC/MR/MS/M+, буфер обмена и смена режима.

Бригада №2:
Весёлый Денис
Ворончук Илья
Лыкова Мария
```

## 5. Тестовые наборы данных и результаты тестирования

Автоматические тесты покрывают классы `TPNumber`, `TFrac`, `TComp`, `PNumberEditor`, `FractionEditor`, `ComplexEditor`, `TProcessor`, `TMemory`, `UniversalCalculatorControl` и ключевые сценарии формы `Form1`.

| № | Тестовый сценарий | Входные данные | Ожидаемый результат |
|---|---|---|---|
| 1 | Цепочка операций в режиме `p`-ичных чисел | Ввод `2 + 2 - 3 =` | На дисплее `1` |
| 2 | Повторное выполнение последней операции | Ввод `5 + 4 = = =` | Последовательность результатов `9`, `13`, `17` |
| 3 | Унарные функции в цепочке | Ввод `2 Sqr + 3 Sqr / 2 =` | На дисплее `6.5` |
| 4 | Память в выражении (`p`-ичные числа) | `MS(3)`, затем `2 + MR =` | На дисплее `5` |
| 5 | Буфер обмена | Копирование `3/4`, затем вставка | На дисплее `3/4` |
| 6 | Смена параметров `p`-ичного числа | `10.5` при основании `10`, затем основание `2` | На дисплее `1010.1` |
| 7 | Цепочка операций в режиме дробей | Ввод `1/2 + 1/4 - 1/8 =` | На дисплее `5/8` |
| 8 | Обратное значение в цепочке дробей | Ввод `1/2 Rev + 1/2 =` | На дисплее `5/2` |
| 9 | Цепочка операций в режиме комплексных чисел | Ввод `1;2 + 3;4 - 1;1 =` | На дисплее `3+i*5` |
| 10 | Квадрат комплексного числа в выражении | Ввод `1;2 Sqr + 1;0 =` | На дисплее `-2+i*4` |
| 11 | Ввод комплексного числа с дробной и отрицательной мнимой частью | Ввод `A.5;-B.C` при основании `16` | На дисплее `A.5;-B.C` |
| 12 | Смена основания в комплексном режиме | Ввод `A;F` при основании `16`, затем основание `10` | На дисплее `10+i*15` |
| 13 | Переключение режима работы | Ввод `9`, `MS`, затем переход в режим дробей | Дисплей `0`, память очищена |
| 14 | Подтверждение выражения клавишей `Enter` | Ввод `2 + 3`, затем `Enter` | На дисплее `5` |
| 15 | Сброс клавишей `Esc` | Ввод `7`, затем `Esc` | На дисплее `0` |
| 16 | UI-проверка complex-режима | Переключение в complex-режим | Кнопки `.` / `Im+/-` / `Re+/-` доступны, основание активно |

Результат автоматического тестирования:

```text
dotnet test lab4/tests/UniversalCalculatorLab4.Tests/UniversalCalculatorLab4.Tests.csproj --no-restore -p:BaseOutputPath=build/dotnet-alt-tests/
Пройден! : не пройдено 0, пройдено 67, пропущено 0, всего 67.
```
