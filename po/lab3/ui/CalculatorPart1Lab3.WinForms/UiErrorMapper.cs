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
