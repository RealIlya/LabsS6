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
