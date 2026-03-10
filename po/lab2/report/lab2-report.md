# Отчёт по лабораторной работе №2

## 1. Задание

Разработать приложение «Телефонная книга», реализовав объектно-ориентированную модель и графический интерфейс.  
Приложение должно обеспечивать:

- создание новой книги;
- чтение книги из файла и сохранение в файл;
- добавление, редактирование, поиск и удаление записей;
- очистку списка записей;
- вызов окна справки из меню интерфейса.

## 2. Ход выполнения

В ходе выполнения лабораторной работы была разработана объектно-ориентированная модель телефонной книги. Для хранения одной записи введена структура `AbonentRecord`, класс `Abonent` реализует операции чтения, изменения и сравнения записи, а класс `AbonentList` отвечает за хранение и упорядоченное размещение списка абонентов.

Для управления приложением реализован класс `PhoneBookControl`, обеспечивающий добавление, редактирование, поиск, удаление, очистку списка, а также чтение и сохранение книги в файл формата `*.pbk`. Сохранение данных выполнено через сериализацию списка записей в JSON.

Графический интерфейс реализован в `Form1`. Форма предоставляет поля ввода имени и номера, список записей, команды добавления, редактирования, поиска, удаления, очистки, создания новой книги, открытия и сохранения файла. Окно справки вызывается из меню и загружает текст из отдельного файла `help.txt`.

Для отображения ошибок в интерфейсе реализован отдельный маппер `UiErrorMapper`, который преобразует исключения ядра в понятные пользовательские сообщения. Для проверки корректности реализации подготовлены автоматические тесты для классов `Abonent`, `AbonentList` и `PhoneBookControl`.

## 3. Диаграмма классов

![Диаграмма классов приложения](C:/Users/Admin/Desktop/LabsS6/po/lab2/report/assets/class-diagram.png)

Рис. 1. Диаграмма классов приложения «Телефонная книга»

## 4. Текст программы

### AbonentRecord.cs

```csharp
namespace PhoneBookLab2.Core;

public readonly record struct AbonentRecord(string Name, string Number);
```

### Abonent.cs

```csharp
namespace PhoneBookLab2.Core;

public sealed class Abonent
{
    private AbonentRecord record;

    public Abonent()
    {
        record = new AbonentRecord(string.Empty, string.Empty);
    }

    public Abonent(AbonentRecord record)
    {
        Write(record);
    }

    public string Name => record.Name;
    public string Number => record.Number;

    public AbonentRecord Read()
    {
        return record;
    }

    public void Write(AbonentRecord newRecord)
    {
        if (string.IsNullOrWhiteSpace(newRecord.Name))
        {
            throw new ArgumentException("Имя не должно быть пустым.", nameof(newRecord));
        }

        if (string.IsNullOrWhiteSpace(newRecord.Number))
        {
            throw new ArgumentException("Номер не должен быть пустым.", nameof(newRecord));
        }

        record = new AbonentRecord(newRecord.Name.Trim(), newRecord.Number.Trim());
    }

    public int Less(Abonent other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other), "Сравниваемый абонент не должен быть null.");
        }

        var byName = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (byName < 0)
        {
            return -1;
        }

        if (byName > 0)
        {
            return 1;
        }

        var byNumber = string.Compare(Number, other.Number, StringComparison.OrdinalIgnoreCase);
        if (byNumber < 0)
        {
            return -1;
        }

        if (byNumber > 0)
        {
            return 1;
        }

        return 0;
    }

    public bool EqualsRecord(Abonent other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other), "Сравниваемый абонент не должен быть null.");
        }

        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Number, other.Number, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return $"{Name} | {Number}";
    }
}
```

### AbonentList.cs

```csharp
namespace PhoneBookLab2.Core;

public sealed class AbonentList
{
    private readonly List<Abonent> items = new();

    public int RecordsCount => items.Count;

    public Abonent ReadRecord(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Индекс записи выходит за пределы списка.");
        }

        return items[index];
    }

    public void AddRecord(Abonent abonent)
    {
        if (abonent is null)
        {
            throw new ArgumentNullException(nameof(abonent), "Абонент не должен быть null.");
        }

        var insertIndex = items.FindIndex(x => x.Less(abonent) > 0);
        if (insertIndex < 0)
        {
            items.Add(abonent);
            return;
        }

        items.Insert(insertIndex, abonent);
    }

    public int FindRecord(Abonent abonent)
    {
        if (abonent is null)
        {
            throw new ArgumentNullException(nameof(abonent), "Абонент не должен быть null.");
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].EqualsRecord(abonent))
            {
                return i;
            }
        }

        return -1;
    }

    public bool RemoveRecord(Abonent abonent)
    {
        var index = FindRecord(abonent);
        if (index < 0)
        {
            return false;
        }

        items.RemoveAt(index);
        return true;
    }

    public void RemoveSelectedRecord(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Индекс записи выходит за пределы списка.");
        }

        items.RemoveAt(index);
    }

    public void ClearList()
    {
        items.Clear();
    }

    public IReadOnlyList<AbonentRecord> Snapshot()
    {
        return items.Select(x => x.Read()).ToList();
    }

    public void ReplaceAll(IEnumerable<AbonentRecord> records)
    {
        if (records is null)
        {
            throw new ArgumentNullException(nameof(records), "Список записей не должен быть null.");
        }

        items.Clear();
        foreach (var record in records)
        {
            AddRecord(new Abonent(record));
        }
    }
}
```

### PhoneBookControl.cs

```csharp
using System.Text.Json;

namespace PhoneBookLab2.Core;

public sealed class PhoneBookControl : IDisposable
{
    private readonly AbonentList list = new();
    private FileStream? fileStream;
    private string? filePath;

    public int RecordsInBook()
    {
        return list.RecordsCount;
    }

    public string ReadRecord(int index)
    {
        return list.ReadRecord(index).ToString();
    }

    public string ReadName(int index)
    {
        return list.ReadRecord(index).Name;
    }

    public string ReadNumber(int index)
    {
        return list.ReadRecord(index).Number;
    }

    public void AddRecord(string name, string number)
    {
        list.AddRecord(new Abonent(new AbonentRecord(name, number)));
    }

    public bool DeleteRecord(string name, string number)
    {
        return list.RemoveRecord(new Abonent(new AbonentRecord(name, number)));
    }

    public void DeleteSelectedRecord(int index)
    {
        list.RemoveSelectedRecord(index);
    }

    public void UpdateSelectedRecord(int index, string name, string number)
    {
        var updated = new Abonent(new AbonentRecord(name, number));
        list.RemoveSelectedRecord(index);
        list.AddRecord(updated);
    }

    public void ClearBook()
    {
        list.ClearList();
    }

    public int FindRecord(string name, string number)
    {
        return list.FindRecord(new Abonent(new AbonentRecord(name, number)));
    }

    public void CreateFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Путь к файлу не должен быть пустым.", nameof(path));
        }

        DisposeStream();

        filePath = path;
        fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
    }

    public void SaveBookToFile()
    {
        EnsureFileCreated();

        fileStream!.SetLength(0);
        fileStream.Position = 0;

        var records = list.Snapshot();
        JsonSerializer.Serialize(fileStream, records);
        fileStream.Flush();
        fileStream.Position = 0;
    }

    public void LoadBookFromFile()
    {
        EnsureFileCreated();

        fileStream!.Position = 0;
        if (fileStream.Length == 0)
        {
            list.ClearList();
            return;
        }

        var records = JsonSerializer.Deserialize<List<AbonentRecord>>(fileStream);
        list.ReplaceAll(records ?? []);
        fileStream.Position = 0;
    }

    public string? CurrentFilePath()
    {
        return filePath;
    }

    public void Dispose()
    {
        DisposeStream();
        GC.SuppressFinalize(this);
    }

    private void DisposeStream()
    {
        fileStream?.Dispose();
        fileStream = null;
    }

    private void EnsureFileCreated()
    {
        if (fileStream is null)
        {
            throw new InvalidOperationException("Файл не создан и не открыт.");
        }
    }
}
```

### UiErrorMapper.cs

```csharp
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
```

### Form1.cs

```csharp
using PhoneBookLab2.Core;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PhoneBookLab2.WinForms;

public partial class Form1 : Form
{
    private const string HelpFileName = "help.txt";

    private readonly PhoneBookControl control = new();

    private readonly TextBox nameBox = new();
    private readonly TextBox numberBox = new();
    private readonly ListBox bookList = new();

    public Form1()
    {
        InitializeComponent();
        BuildUi();
        Load += (_, _) => UpdateList();
        FormClosed += (_, _) => control.Dispose();
    }

    private void BuildUi()
    {
        Text = "Телефонная книга";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 520);

        var menuStrip = new MenuStrip();
        var helpMenu = new ToolStripMenuItem("Справка");
        var aboutItem = new ToolStripMenuItem("О программе");
        aboutItem.Click += (_, _) => ShowHelp();
        helpMenu.DropDownItems.Add(aboutItem);
        menuStrip.Items.Add(helpMenu);
        MainMenuStrip = menuStrip;
        Controls.Add(menuStrip);

        var nameLabel = new Label { Text = "Имя", AutoSize = true, Location = new Point(15, 45) };
        nameBox.SetBounds(60, 40, 220, 30);

        var numberLabel = new Label { Text = "Номер", AutoSize = true, Location = new Point(300, 45) };
        numberBox.SetBounds(360, 40, 180, 30);

        var addButton = CreateButton("Добавить", 575, 38, OnAddClick);

        var updateButton = CreateButton("Изменить", 15, 80, OnUpdateSelectedClick);
        var findButton = CreateButton("Найти", 155, 80, OnFindClick);
        var removeSelectedButton = CreateButton("Удалить выдел.", 295, 80, OnDeleteSelectedClick);
        var removeByValueButton = CreateButton("Удалить по знач.", 435, 80, OnDeleteByValueClick);
        var clearButton = CreateButton("Очистить", 575, 80, OnClearClick);

        var newFileButton = CreateButton("Новая книга", 15, 120, OnNewFileClick);
        var openButton = CreateButton("Открыть", 155, 120, OnOpenClick);
        var saveButton = CreateButton("Сохранить", 295, 120, OnSaveClick);

        bookList.SetBounds(15, 165, 690, 290);
        bookList.Font = new Font("Consolas", 11f);
        bookList.DoubleClick += (_, _) => OnDeleteSelectedClick(this, EventArgs.Empty);
        bookList.SelectedIndexChanged += (_, _) => SyncSelectionToEditors();

        Controls.Add(nameLabel);
        Controls.Add(nameBox);
        Controls.Add(numberLabel);
        Controls.Add(numberBox);
        Controls.Add(addButton);
        Controls.Add(updateButton);
        Controls.Add(removeByValueButton);
        Controls.Add(removeSelectedButton);
        Controls.Add(findButton);
        Controls.Add(clearButton);
        Controls.Add(newFileButton);
        Controls.Add(openButton);
        Controls.Add(saveButton);
        Controls.Add(bookList);
    }

    private Button CreateButton(string text, int x, int y, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Left = x,
            Top = y,
            Width = 130,
            Height = 32
        };
        button.Click += onClick;
        return button;
    }

    private void OnAddClick(object? sender, EventArgs e)
    {
        try
        {
            control.AddRecord(nameBox.Text, numberBox.Text);
            bookList.ClearSelected();
            UpdateList();
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnUpdateSelectedClick(object? sender, EventArgs e)
    {
        if (bookList.SelectedIndex < 0)
        {
            MessageBox.Show("Выберите запись для редактирования.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            control.UpdateSelectedRecord(bookList.SelectedIndex, nameBox.Text, numberBox.Text);
            UpdateList();
            SelectRecord(nameBox.Text, numberBox.Text);
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnDeleteByValueClick(object? sender, EventArgs e)
    {
        try
        {
            var removed = control.DeleteRecord(nameBox.Text, numberBox.Text);
            if (!removed)
            {
                MessageBox.Show("Запись не найдена.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            bookList.ClearSelected();
            UpdateList();
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnDeleteSelectedClick(object? sender, EventArgs e)
    {
        if (bookList.SelectedIndex < 0)
        {
            return;
        }

        try
        {
            control.DeleteSelectedRecord(bookList.SelectedIndex);
            bookList.ClearSelected();
            UpdateList();
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnFindClick(object? sender, EventArgs e)
    {
        try
        {
            var index = control.FindRecord(nameBox.Text, numberBox.Text);
            if (index < 0)
            {
                MessageBox.Show("Запись не найдена.", "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bookList.SelectedIndex = index;
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnClearClick(object? sender, EventArgs e)
    {
        control.ClearBook();
        bookList.ClearSelected();
        UpdateList();
    }

    private void OnNewFileClick(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog();
        dialog.Filter = "Phone book (*.pbk)|*.pbk|All files (*.*)|*.*";
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        control.CreateFile(dialog.FileName);
        control.ClearBook();
        control.SaveBookToFile();
        bookList.ClearSelected();
        UpdateList();
    }

    private void OnOpenClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog();
        dialog.Filter = "Phone book (*.pbk)|*.pbk|All files (*.*)|*.*";
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            control.CreateFile(dialog.FileName);
            control.LoadBookFromFile();
            bookList.ClearSelected();
            UpdateList();
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(control.CurrentFilePath()))
            {
                OnNewFileClick(sender, e);
                if (string.IsNullOrWhiteSpace(control.CurrentFilePath()))
                {
                    return;
                }
            }

            control.SaveBookToFile();
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void UpdateList()
    {
        bookList.Items.Clear();
        for (var i = 0; i < control.RecordsInBook(); i++)
        {
            bookList.Items.Add(control.ReadRecord(i));
        }
    }

    private void SelectRecord(string name, string number)
    {
        var index = control.FindRecord(name, number);
        if (index >= 0)
        {
            bookList.SelectedIndex = index;
        }
    }

    private void SyncSelectionToEditors()
    {
        if (bookList.SelectedIndex < 0)
        {
            return;
        }

        nameBox.Text = control.ReadName(bookList.SelectedIndex);
        numberBox.Text = control.ReadNumber(bookList.SelectedIndex);
    }

    private void ShowHelp()
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
            ShowMappedError(ex, "Справка");
        }
    }

    private static void ShowMappedError(Exception ex, string title = "Ошибка")
    {
        MessageBox.Show(UiErrorMapper.ToUserMessage(ex), title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
```

### Program.cs

```csharp
namespace PhoneBookLab2.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
```

### help.txt

```text
Телефонная книга

Ввод, поиск, редактирование и хранение записей абонентов.
Основные действия: Добавить, Изменить, Удалить, Сохранить/Открыть.

Бригада №2:
Весёлый Денис
Ворончук Илья
Лыкова Мария
```

## 5. Тестовые наборы данных и результаты тестирования

| № | Тестовый сценарий | Входные данные | Ожидаемый результат |
|---|---|---|---|
| 1 | Добавление и поиск записи | `Ivanov, 100`; `Petrov, 200` | Запись `Ivanov, 100` найдена, количество записей корректно |
| 2 | Удаление выделенной записи | Список из 2 записей, удаление по индексу `0` | В списке остаётся 1 запись |
| 3 | Редактирование записи | Обновление `Petrov, 200` в `Alekseev, 900` | Запись обновлена, список остаётся отсортированным |
| 4 | Сохранение и загрузка книги | Сохранение в `*.pbk`, затем чтение файла | Состав и порядок записей восстановлены |
| 5 | Очистка книги | Книга с записями, команда очистки | Количество записей равно `0` |
| 6 | Проверка класса `Abonent` | Чтение, запись, сравнение записей | Методы `Read`, `Write`, `Less`, `EqualsRecord` работают корректно |
| 7 | Проверка класса `AbonentList` | Добавление, поиск, удаление, замена списка | Операции списка соответствуют спецификации |

Результат автоматического тестирования:

```text
dotnet test lab2/tests/PhoneBookLab2.Tests/PhoneBookLab2.Tests.csproj
Пройден! : не пройдено 0, пройдено 13, пропущено 0, всего 13.
```
