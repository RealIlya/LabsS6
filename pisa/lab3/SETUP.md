# Инструкция по запуску проекта на другом устройстве

## Требования

- Windows 10/11
- Visual Studio 2019/2022 (с поддержкой ASP.NET и WebForms)
- .NET Framework 4.x
- SQL Server (Express или полная версия)

## Шаг 1 — Клонирование/копирование

Скопируйте папку `BookCatalog1` на целевое устройство. Структура:

```
BookCatalog1/
  BookCatalog/          ← основной проект
    BookCatalog.csproj
    Web.config
    *.aspx, *.aspx.cs
    ...
```

## Шаг 2 — Открытие в Visual Studio

1. Откройте Visual Studio
2. Файл → Открыть → Проект/Решение
3. Выберите `BookCatalog.csproj` (или `.sln` файл)
4. Дождитесь восстановления NuGet-пакетов (если потребуется)

## Шаг 3 — Настройка базы данных

### Вариант A — SQL Server Express (локальный)

1. Установите SQL Server Express, если не установлен
2. В Visual Studio: Обозреватель серверов → Подключиться к базе данных
3. В `Web.config` проверьте строку подключения:

```xml
<connectionStrings>
    <add name="BookCatalogDB"
         connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=BookCatalog;Integrated Security=True"
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

4. Если база данных `BookCatalog` не существует — создайте её через SQL Server Management Studio или Visual Studio
5. Таблицы создаются автоматически при первом запуске (если настроен автоматический миграции) или вручную через SQL-скрипты в папке проекта

### Вариант B — Подключение к существующей БД

1. Откройте `Web.config`
2. Измените `connectionString` на нужный сервер:

```xml
connectionString="Data Source=ВАШ_СЕРВЕР;Initial Catalog=BookCatalog;Integrated Security=True"
```

Или с логином/паролем:

```xml
connectionString="Data Source=ВАШ_СЕРВЕР;Initial Catalog=BookCatalog;User ID=sa;Password=ВАШ_ПАРОЛЬ"
```

## Шаг 4 — Запуск

1. В Visual Studio нажмите **F5** (с отладкой) или **Ctrl+F5** (без отладки)
2. Откроется браузер с адресом вроде `http://localhost:PORT/`
3. Главная страница — `Default.aspx`

## Тестовые данные

- **Админ:** `admin@test.com` / `admin123`
- **Регистрация:** через форму на `Register.aspx`

## Troubleshooting

| Проблема | Решение |
|----------|---------|
| Ошибка подключения к БД | Проверьте строку подключения в `Web.config`, убедитесь что SQL Server запущен |
| FriendlyUrls не работают | Проверьте что в `App_Start/RouteConfig.cs` включён `EnableFriendlyUrls` |
| PageMethods возвращают 404 | FriendlyUrls ломают PageMethods — используйте `.ashx` обработчики (см. `Admin/CheckISBNHandler.ashx`) |
| jQuery не загружается | Проверьте что `Scripts/jquery-3.7.0.min.js` существует |
| Стили не применяются | Очистите кеш браузера (Ctrl+Shift+R) |
| Не видно кнопку [ЗАБРОНИРОВАТЬ] | Обновите CSS: `nier-theme.css?v=N` — инкрементируйте версию |

## Структура AJAX-компонентов

| Компонент | Файл | Назначение |
|-----------|------|-----------|
| UpdatePanel | Default.aspx, AddBook.aspx, Register.aspx, Login.aspx, Booking.aspx, WriteOff.aspx | Асинхронное обновление без перезагрузки |
| jQuery AJAX | Admin/CheckISBNHandler.ashx | Проверка уникальности ISBN |
| PageMethods | Register.aspx.cs | Проверка доступности email |
| Vanilla JS | Default.aspx (inline) | Слот-машина, сохранение скролла |
