using PhoneBookLab2.Core;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PhoneBookLab2.WinForms;

public partial class Form1 : Form
{
    private const string HelpFileName = "help.txt";
    private const string DefaultWindowTitle = "Телефонная книга";

    private readonly PhoneBookControl control = new();

    private readonly TextBox nameBox = new();
    private readonly TextBox numberBox = new();
    private readonly ListBox bookList = new();
    private readonly Button addButton = new();
    private readonly Button updateButton = new();
    private readonly Button findButton = new();
    private readonly Button removeSelectedButton = new();
    private readonly Button removeByValueButton = new();
    private readonly Button clearButton = new();
    private readonly Button saveButton = new();

    public Form1()
    {
        InitializeComponent();
        BuildUi();
        Load += (_, _) => UpdateList();
        FormClosed += (_, _) => control.Dispose();
    }

    private void BuildUi()
    {
        Text = DefaultWindowTitle;
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

        ConfigureButton(addButton, "Добавить", 575, 38, OnAddClick);

        ConfigureButton(updateButton, "Изменить", 15, 80, OnUpdateSelectedClick);
        ConfigureButton(findButton, "Найти", 155, 80, OnFindClick);
        ConfigureButton(removeSelectedButton, "Удалить выдел.", 295, 80, OnDeleteSelectedClick);
        ConfigureButton(removeByValueButton, "Удалить по знач.", 435, 80, OnDeleteByValueClick);
        ConfigureButton(clearButton, "Очистить", 575, 80, OnClearClick);

        var newFileButton = CreateButton("Новая книга", 15, 120, OnNewFileClick);
        var openButton = CreateButton("Открыть", 155, 120, OnOpenClick);
        ConfigureButton(saveButton, "Сохранить", 295, 120, OnSaveClick);

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

        UpdateCommandState();
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

    private static void ConfigureButton(Button button, string text, int x, int y, EventHandler onClick)
    {
        button.Text = text;
        button.Left = x;
        button.Top = y;
        button.Width = 130;
        button.Height = 32;
        button.Click += onClick;
    }

    private void OnAddClick(object? sender, EventArgs e)
    {
        if (!HasOpenedBook())
        {
            MessageBox.Show("Сначала создайте новую книгу или откройте существующую.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            control.AddRecord(nameBox.Text, numberBox.Text);
            bookList.ClearSelected();
            UpdateList();
            UpdateCommandState();
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

        if (!HasOpenedBook())
        {
            MessageBox.Show("Сначала создайте новую книгу или откройте существующую.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            control.UpdateSelectedRecord(bookList.SelectedIndex, nameBox.Text, numberBox.Text);
            UpdateList();
            SelectRecord(nameBox.Text, numberBox.Text);
            UpdateCommandState();
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnDeleteByValueClick(object? sender, EventArgs e)
    {
        if (!HasOpenedBook())
        {
            MessageBox.Show("Сначала создайте новую книгу или откройте существующую.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var removed = control.DeleteRecord(nameBox.Text, numberBox.Text);
            if (!removed)
            {
                MessageBox.Show("Запись не найдена.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            bookList.ClearSelected();
            UpdateList();
            UpdateCommandState();
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

        if (!HasOpenedBook())
        {
            MessageBox.Show("Сначала создайте новую книгу или откройте существующую.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            control.DeleteSelectedRecord(bookList.SelectedIndex);
            bookList.ClearSelected();
            UpdateList();
            UpdateCommandState();
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnFindClick(object? sender, EventArgs e)
    {
        if (!HasOpenedBook())
        {
            MessageBox.Show("Сначала создайте новую книгу или откройте существующую.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

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
        if (!HasOpenedBook())
        {
            MessageBox.Show("Сначала создайте новую книгу или откройте существующую.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        control.ClearBook();
        bookList.ClearSelected();
        UpdateList();
        UpdateCommandState();
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
        UpdateCommandState();
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
            UpdateCommandState();
        }
        catch (Exception ex)
        {
            ShowMappedError(ex);
        }
    }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        if (!HasOpenedBook())
        {
            MessageBox.Show("Сначала создайте новую книгу или откройте существующую.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
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
            UpdateCommandState();
            return;
        }

        nameBox.Text = control.ReadName(bookList.SelectedIndex);
        numberBox.Text = control.ReadNumber(bookList.SelectedIndex);
        UpdateCommandState();
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

    private bool HasOpenedBook()
    {
        return !string.IsNullOrWhiteSpace(control.CurrentFilePath());
    }

    private void UpdateCommandState()
    {
        var hasOpenedBook = HasOpenedBook();
        nameBox.Enabled = hasOpenedBook;
        numberBox.Enabled = hasOpenedBook;
        addButton.Enabled = hasOpenedBook;
        findButton.Enabled = hasOpenedBook;
        removeByValueButton.Enabled = hasOpenedBook;
        clearButton.Enabled = hasOpenedBook;
        saveButton.Enabled = hasOpenedBook;

        var hasSelection = hasOpenedBook && bookList.SelectedIndex >= 0;
        updateButton.Enabled = hasSelection;
        removeSelectedButton.Enabled = hasSelection;

        UpdateWindowTitle();
    }

    private void UpdateWindowTitle()
    {
        var currentFilePath = control.CurrentFilePath();
        if (string.IsNullOrWhiteSpace(currentFilePath))
        {
            Text = DefaultWindowTitle;
            return;
        }

        Text = $"{DefaultWindowTitle} - {Path.GetFileName(currentFilePath)}";
    }
}
