using System.Text.Json;

namespace PhoneBookLab2.WinForms;

internal static class UiErrorMapper
{
    public static string ToUserMessage(Exception ex)
    {
        return ex switch
        {
            ArgumentException { ParamName: "path" } => "Укажите корректный путь к файлу телефонной книги.",
            ArgumentException { ParamName: "newRecord" } => "Заполните имя и номер абонента.",

            ArgumentNullException { ParamName: "other" } => "Не удалось выполнить сравнение записи. Повторите действие.",
            ArgumentNullException { ParamName: "abonent" } => "Передана пустая запись абонента.",
            ArgumentNullException { ParamName: "records" } => "Список записей для загрузки пуст.",

            ArgumentOutOfRangeException { ParamName: "index" } => "Выбрана некорректная запись.",
            InvalidOperationException => "Сначала создайте новую книгу или откройте существующую.",
            JsonException => "Файл книги имеет некорректный формат.",
            UnauthorizedAccessException => "Недостаточно прав для доступа к файлу.",
            IOException => "Не удалось выполнить операцию с файлом. Проверьте путь и доступ.",
            _ => "Произошла непредвиденная ошибка. Повторите действие."
        };
    }
}
