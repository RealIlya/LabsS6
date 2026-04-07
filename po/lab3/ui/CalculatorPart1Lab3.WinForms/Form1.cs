using CalculatorPart1Lab3.Core;
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
        KeyPress += OnFormKeyPress;

        var menu = BuildMenu();
        Controls.Add(menu);

        display.SetBounds(15, 40, 455, 36);
        display.ReadOnly = true;
        display.TabStop = false;
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
            Font = new Font("Segoe UI", 10f),
            TabStop = false
        };
        button.Click += (sender, args) =>
        {
            onClick(sender, args);
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
            RunEditor(digit);
            ConsumeKey(e);
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Back:
                RunEditor(17);
                ConsumeKey(e);
                break;
            case Keys.Delete:
            case Keys.Escape:
                ResetAll();
                ConsumeKey(e);
                break;
            case Keys.Enter:
                RunEqual();
                ConsumeKey(e);
                break;
            case Keys.Add:
                RunOperation(BinaryOperation.Add);
                ConsumeKey(e);
                break;
            case Keys.Subtract:
                HandleMinusFromKeyboard();
                ConsumeKey(e);
                break;
            case Keys.Multiply:
                RunOperation(BinaryOperation.Mul);
                ConsumeKey(e);
                break;
            case Keys.Divide:
                RunOperation(BinaryOperation.Dvd);
                ConsumeKey(e);
                break;
        }
    }

    private void OnFormKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (TryHandleDigitChar(e.KeyChar, out var digit))
        {
            RunEditor(digit);
            e.Handled = true;
            return;
        }

        switch (e.KeyChar)
        {
            case '.':
            case ',':
                RunEditor(16);
                e.Handled = true;
                break;
            case '+':
                RunOperation(BinaryOperation.Add);
                e.Handled = true;
                break;
            case '-':
                HandleMinusFromKeyboard();
                e.Handled = true;
                break;
            case '*':
                RunOperation(BinaryOperation.Mul);
                e.Handled = true;
                break;
            case '/':
                RunOperation(BinaryOperation.Dvd);
                e.Handled = true;
                break;
            case '=':
                RunEqual();
                e.Handled = true;
                break;
        }
    }

    private static bool TryHandleDigitChar(char ch, out int digit)
    {
        digit = -1;

        if (ch >= '0' && ch <= '9')
        {
            digit = ch - '0';
            return true;
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
        if (control.State is CalculatorControl.CalcState.Start or CalculatorControl.CalcState.OperationSet
            || control.Display is "0" or "-0")
        {
            RunEditor(20);
            return;
        }

        RunOperation(BinaryOperation.Sub);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Enter)
        {
            RunEqual();
            return true;
        }

        if (keyData == Keys.Escape)
        {
            ResetAll();
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
