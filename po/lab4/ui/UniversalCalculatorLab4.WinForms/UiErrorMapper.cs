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
