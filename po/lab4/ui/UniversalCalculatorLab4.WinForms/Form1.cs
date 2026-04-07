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
