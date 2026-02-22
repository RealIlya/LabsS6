# Отчёт по лабораторной работе №1

## 1. Задание

Разработать приложение «Конвертер p1_p2» для преобразования действительных чисел со знаком из системы счисления с основанием `p1` в систему счисления с основанием `p2` при `2..16`, реализовать интерфейс ввода/редактирования, историю преобразований и покрыть методы тестами.

## 2. Текст программы

Ниже приведён листинг исходного кода проекта.


### Conver_10_P.cs
```csharp
using System.Text;

namespace ConverterLab1;

public static class Conver_10_P
{
    public static char int_to_Char(int n)
    {
        if (n < 0 || n > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Цифра должна быть в диапазоне 0..15.");
        }

        return n < 10 ? (char)('0' + n) : (char)('A' + (n - 10));
    }

    public static string int_to_P(int n, int p)
    {
        CheckBase(p);

        if (n == 0)
        {
            return "0";
        }

        var value = Math.Abs((long)n);
        var sb = new StringBuilder();

        while (value > 0)
        {
            var digit = (int)(value % p);
            sb.Insert(0, int_to_Char(digit));
            value /= p;
        }

        if (n < 0)
        {
            sb.Insert(0, '-');
        }

        return sb.ToString();
    }

    public static string flt_to_P(double n, int p, int c)
    {
        CheckBase(p);

        if (c < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(c), "Точность должна быть неотрицательной.");
        }

        if (n < 0)
        {
            n = Math.Abs(n);
        }

        var sb = new StringBuilder();
        var frac = n;

        for (var i = 0; i < c; i++)
        {
            frac *= p;
            var digit = (int)Math.Floor(frac);
            sb.Append(int_to_Char(digit));
            frac -= digit;

            if (frac == 0)
            {
                break;
            }
        }

        return sb.ToString();
    }

    public static string Do(double n, int p, int c)
    {
        CheckBase(p);

        if (c < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(c), "Точность должна быть неотрицательной.");
        }

        var sign = n < 0 ? "-" : string.Empty;
        var abs = Math.Abs(n);

        var intPart = (int)Math.Floor(abs);
        var fracPart = abs - intPart;

        var intText = int_to_P(intPart, p);
        var fracText = c == 0 ? string.Empty : flt_to_P(fracPart, p, c);

        if (fracText.Length == 0 || fracPart == 0)
        {
            return sign + intText;
        }

        return sign + intText + "." + fracText;
    }

    private static void CheckBase(int p)
    {
        if (p < 2 || p > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(p), "Основание должно быть в диапазоне 2..16.");
        }
    }
}
```

### Conver_P_10.cs
```csharp
namespace ConverterLab1;

public static class Conver_P_10
{
    public static double char_To_num(char ch)
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

        throw new ArgumentException("Недопустимый символ цифры.", nameof(ch));
    }

    private static double convert(string pNum, int p, double weight)
    {
        var sum = 0.0;

        foreach (var ch in pNum)
        {
            var value = char_To_num(ch);
            if (value >= p)
            {
                throw new ArgumentException($"Цифра {ch} недопустима для основания {p}.");
            }

            sum += value * weight;
            weight /= p;
        }

        return sum;
    }

    public static double dval(string pNum, int p)
    {
        if (string.IsNullOrWhiteSpace(pNum))
        {
            throw new ArgumentException("Строка числа не должна быть пустой.", nameof(pNum));
        }

        if (p < 2 || p > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(p), "Основание должно быть в диапазоне 2..16.");
        }

        var text = pNum.Trim();
        var sign = 1.0;

        if (text.StartsWith("-", StringComparison.Ordinal))
        {
            sign = -1.0;
            text = text[1..];
        }

        if (text.Length == 0)
        {
            throw new ArgumentException("Некорректная запись числа.", nameof(pNum));
        }

        var parts = text.Split('.');
        if (parts.Length > 2)
        {
            throw new ArgumentException("Некорректный разделитель дробной части.", nameof(pNum));
        }

        var intPart = parts[0];
        var fracPart = parts.Length == 2 ? parts[1] : string.Empty;

        var intValue = 0.0;
        if (intPart.Length > 0)
        {
            intValue = convert(intPart, p, Math.Pow(p, intPart.Length - 1));
        }

        var fracValue = 0.0;
        if (fracPart.Length > 0)
        {
            fracValue = convert(fracPart, p, 1.0 / p);
        }

        return sign * (intValue + fracValue);
    }
}
```

### Editor.cs
```csharp
namespace ConverterLab1;

public class Editor
{
    private string number = "0";
    private const string delim = ".";
    private const string zero = "0";
    private const string minusZero = "-0";

    public string Number
    {
        get { return number; }
    }

    public string AddDigit(int n)
    {
        if (n < 0 || n > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Цифра должна быть в диапазоне 0..15.");
        }

        var ch = Conver_10_P.int_to_Char(n).ToString();
        if (number == zero)
        {
            number = ch;
        }
        else if (number == minusZero)
        {
            number = "-" + ch;
        }
        else
        {
            number += ch;
        }

        return number;
    }

    public int Acc()
    {
        var index = number.IndexOf(delim, StringComparison.Ordinal);
        if (index < 0)
        {
            return 0;
        }

        return number.Length - index - 1;
    }

    public string AddZero()
    {
        if (number != zero && number != minusZero)
        {
            number += zero;
        }

        return number;
    }

    public string AddDelim()
    {
        if (number.Contains(delim, StringComparison.Ordinal))
        {
            return number;
        }

        number += delim;
        return number;
    }

    public string Bs()
    {
        if (number.Length <= 1)
        {
            number = zero;
            return number;
        }

        number = number[..^1];
        if (number == "-" || number.Length == 0)
        {
            number = zero;
        }

        return number;
    }

    public string Clear()
    {
        number = zero;
        return number;
    }

    public string ToggleSign()
    {
        if (number.StartsWith("-", StringComparison.Ordinal))
        {
            number = number[1..];
            if (number.Length == 0)
            {
                number = zero;
            }

            return number;
        }

        number = "-" + number;
        return number;
    }

    public string DoEdit(int j)
    {
        if (j < 0)
        {
            return number;
        }

        if (j == 0)
        {
            return AddZero();
        }

        if (j >= 1 && j <= 15)
        {
            return AddDigit(j);
        }

        if (j == 16)
        {
            return AddDelim();
        }

        if (j == 17)
        {
            return Bs();
        }

        if (j == 18)
        {
            return Clear();
        }

        if (j == 20)
        {
            return ToggleSign();
        }

        return number;
    }
}
```

### History.cs
```csharp
namespace ConverterLab1;

public struct Record
{
    public int p1;
    public int p2;
    public string number1;
    public string number2;

    public Record(int p1, int p2, string n1, string n2)
    {
        this.p1 = p1;
        this.p2 = p2;
        number1 = n1;
        number2 = n2;
    }

    public override string ToString()
    {
        return $"{number1} ({p1}) -> {number2} ({p2}){Environment.NewLine}";
    }
}

public class History
{
    private readonly List<Record> L;

    public History()
    {
        L = new List<Record>();
    }

    public Record this[int i]
    {
        get { return L[i]; }
    }

    public void AddRecord(int p1, int p2, string n1, string n2)
    {
        L.Add(new Record(p1, p2, n1, n2));
    }

    public void Clear()
    {
        L.Clear();
    }

    public int Count()
    {
        return L.Count;
    }

    // Методы-синонимы под названия из методички.
    public void ДобавитьЗапись(int p1, int p2, string n1, string n2)
    {
        AddRecord(p1, p2, n1, n2);
    }

    public int Записей()
    {
        return Count();
    }

    public void ОчиститьИсторию()
    {
        Clear();
    }
}
```

### Control_.cs
```csharp
namespace ConverterLab1;

public class Control_
{
    private const int pin = 10;
    private const int pout = 16;

    public enum State
    {
        Редактирование,
        Преобразовано
    }

    public History his = new History();
    public Editor ed = new Editor();

    public State St { get; set; }
    public int Pin { get; set; }
    public int Pout { get; set; }

    public Control_()
    {
        St = State.Редактирование;
        Pin = pin;
        Pout = pout;
    }

    public string DoCmnd(int j)
    {
        if (j == 19)
        {
            var input = ed.Number;
            var decimalValue = Conver_P_10.dval(input, Pin);
            var result = Conver_10_P.Do(decimalValue, Pout, acc());
            St = State.Преобразовано;
            his.ДобавитьЗапись(Pin, Pout, input, result);
            return result;
        }

        St = State.Редактирование;
        return ed.DoEdit(j);
    }

    private int acc()
    {
        var value = (int)Math.Round(ed.Acc() * Math.Log(Pin) / Math.Log(Pout) + 0.5);
        return Math.Max(value, 1);
    }
}
```

### Program.cs
```csharp
using System;
using System.Windows.Forms;

namespace ConverterLab1.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
```

### MainForm.cs
```csharp
using ConverterLab1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ConverterLab1.WinForms;

public class MainForm : Form
{
    private readonly Control_ ctl = new();

    private readonly Label labelInput = new();
    private readonly Label labelOutput = new();
    private readonly Label labelP1 = new();
    private readonly Label labelP2 = new();
    private readonly TrackBar trackBar1 = new();
    private readonly TrackBar trackBar2 = new();
    private readonly NumericUpDown numericUpDown1 = new();
    private readonly NumericUpDown numericUpDown2 = new();
    private readonly List<Button> digitButtons = new();

    public MainForm()
    {
        InitializeUi();
        Load += MainForm_Load;
        KeyPress += MainForm_KeyPress;
        KeyDown += MainForm_KeyDown;
    }

    private void InitializeUi()
    {
        Text = "Конвертор";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(420, 560);
        KeyPreview = true;

        var menu = new MenuStrip();
        var exitItem = new ToolStripMenuItem("Выход");
        var historyItem = new ToolStripMenuItem("История");
        var helpItem = new ToolStripMenuItem("Справка");

        exitItem.Click += (_, _) => Close();
        historyItem.Click += (_, _) => ShowHistory();
        helpItem.Click += (_, _) => ShowAbout();

        menu.Items.Add(exitItem);
        menu.Items.Add(historyItem);
        menu.Items.Add(helpItem);
        MainMenuStrip = menu;
        Controls.Add(menu);

        labelInput.SetBounds(20, 40, 380, 32);
        labelInput.BorderStyle = BorderStyle.Fixed3D;
        labelInput.TextAlign = ContentAlignment.MiddleRight;
        labelInput.Font = new Font("Consolas", 12f, FontStyle.Regular);

        labelP1.SetBounds(20, 82, 300, 22);

        numericUpDown1.SetBounds(350, 80, 50, 24);
        numericUpDown1.Minimum = 2;
        numericUpDown1.Maximum = 16;
        numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;

        trackBar1.SetBounds(20, 105, 380, 45);
        trackBar1.Minimum = 2;
        trackBar1.Maximum = 16;
        trackBar1.TickStyle = TickStyle.BottomRight;
        trackBar1.Scroll += trackBar1_Scroll;

        labelOutput.SetBounds(20, 155, 380, 32);
        labelOutput.BorderStyle = BorderStyle.Fixed3D;
        labelOutput.TextAlign = ContentAlignment.MiddleRight;
        labelOutput.Font = new Font("Consolas", 12f, FontStyle.Regular);

        labelP2.SetBounds(20, 197, 300, 22);

        numericUpDown2.SetBounds(350, 195, 50, 24);
        numericUpDown2.Minimum = 2;
        numericUpDown2.Maximum = 16;
        numericUpDown2.ValueChanged += numericUpDown2_ValueChanged;

        trackBar2.SetBounds(20, 220, 380, 45);
        trackBar2.Minimum = 2;
        trackBar2.Maximum = 16;
        trackBar2.TickStyle = TickStyle.BottomRight;
        trackBar2.Scroll += trackBar2_Scroll;

        var panel = new TableLayoutPanel();
        panel.SetBounds(20, 275, 380, 250);
        panel.ColumnCount = 5;
        panel.RowCount = 6;
        panel.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        panel.Padding = new Padding(0);

        for (var i = 0; i < panel.ColumnCount; i++)
        {
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
        }

        for (var i = 0; i < panel.RowCount; i++)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / panel.RowCount));
        }

        var layout = new (string Text, int Tag, int Col, int Row)[]
        {
            ("A", 10, 0, 0),
            ("B", 11, 0, 1),
            ("C", 12, 0, 2),
            ("D", 13, 0, 3),
            ("E", 14, 0, 4),
            ("F", 15, 0, 5),
            ("CL", 18, 3, 0),
            ("BS", 17, 4, 0),
            ("7", 7, 1, 2),
            ("8", 8, 2, 2),
            ("9", 9, 3, 2),
            ("4", 4, 1, 3),
            ("5", 5, 2, 3),
            ("6", 6, 3, 3),
            ("1", 1, 1, 4),
            ("2", 2, 2, 4),
            ("3", 3, 3, 4),
            ("+/-", 20, 1, 5),
            ("0", 0, 2, 5),
            (".", 16, 3, 5),
            ("Execute", 19, 4, 5)
        };

        foreach (var key in layout)
        {
            var button = CreateButton(key.Text, key.Tag);
            panel.Controls.Add(button, key.Col, key.Row);

            if (key.Tag <= 15)
            {
                digitButtons.Add(button);
            }
        }

        Controls.Add(labelInput);
        Controls.Add(labelP1);
        Controls.Add(numericUpDown1);
        Controls.Add(trackBar1);
        Controls.Add(labelOutput);
        Controls.Add(labelP2);
        Controls.Add(numericUpDown2);
        Controls.Add(trackBar2);
        Controls.Add(panel);
    }

    private Button CreateButton(string text, int tag)
    {
        var button = new Button();
        button.Text = text;
        button.Tag = tag;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(3);
        button.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        button.Click += button_Click;
        return button;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        labelInput.Text = ctl.ed.Number;
        labelOutput.Text = "0";

        trackBar1.Value = ctl.Pin;
        numericUpDown1.Value = ctl.Pin;

        trackBar2.Value = ctl.Pout;
        numericUpDown2.Value = ctl.Pout;

        UpdateP1();
        UpdateP2();
    }

    private void DoCmnd(int j)
    {
        if (j < 0)
        {
            return;
        }

        try
        {
            if (j == 19)
            {
                labelOutput.Text = ctl.DoCmnd(j);
            }
            else
            {
                if (ctl.St == Control_.State.Преобразовано)
                {
                    labelInput.Text = ctl.DoCmnd(18);
                }

                labelInput.Text = ctl.DoCmnd(j);
                labelOutput.Text = "0";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateButtons()
    {
        foreach (var button in digitButtons)
        {
            var j = Convert.ToInt32(button.Tag);
            button.Enabled = j < trackBar1.Value;
        }
    }

    private void UpdateP1()
    {
        labelP1.Text = $"Основание с. сч. исходного числа {trackBar1.Value}";
        ctl.Pin = trackBar1.Value;
        UpdateButtons();
        labelInput.Text = ctl.DoCmnd(18);
        labelOutput.Text = "0";
    }

    private void UpdateP2()
    {
        labelP2.Text = $"Основание с. сч. результата {trackBar2.Value}";
        ctl.Pout = trackBar2.Value;

        try
        {
            labelOutput.Text = ctl.DoCmnd(19);
        }
        catch
        {
            labelOutput.Text = "0";
        }
    }

    private void button_Click(object? sender, EventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var j = Convert.ToInt32(button.Tag);
        DoCmnd(j);
    }

    private void trackBar1_Scroll(object? sender, EventArgs e)
    {
        numericUpDown1.Value = trackBar1.Value;
        UpdateP1();
    }

    private void numericUpDown1_ValueChanged(object? sender, EventArgs e)
    {
        trackBar1.Value = Convert.ToInt32(numericUpDown1.Value);
        UpdateP1();
    }

    private void trackBar2_Scroll(object? sender, EventArgs e)
    {
        numericUpDown2.Value = trackBar2.Value;
        UpdateP2();
    }

    private void numericUpDown2_ValueChanged(object? sender, EventArgs e)
    {
        trackBar2.Value = Convert.ToInt32(numericUpDown2.Value);
        UpdateP2();
    }

    private void ShowHistory()
    {
        using var historyForm = new HistoryForm(ctl.his);
        historyForm.ShowDialog(this);
    }

    private void ShowAbout()
    {
        using var aboutForm = new AboutForm();
        aboutForm.ShowDialog(this);
    }

    private void MainForm_KeyPress(object? sender, KeyPressEventArgs e)
    {
        var i = -1;

        if (e.KeyChar >= 'A' && e.KeyChar <= 'F')
        {
            i = e.KeyChar - 'A' + 10;
        }

        if (e.KeyChar >= 'a' && e.KeyChar <= 'f')
        {
            i = e.KeyChar - 'a' + 10;
        }

        if (e.KeyChar >= '0' && e.KeyChar <= '9')
        {
            i = e.KeyChar - '0';
        }

        if (e.KeyChar == '.' || e.KeyChar == ',')
        {
            i = 16;
        }

        if (e.KeyChar == '-')
        {
            i = 20;
        }

        if ((int)e.KeyChar == 8)
        {
            i = 17;
        }

        if ((int)e.KeyChar == 13)
        {
            i = 19;
        }

        if ((i < ctl.Pin) || (i >= 16))
        {
            DoCmnd(i);
        }
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            DoCmnd(18);
        }

        if (e.KeyCode == Keys.Enter)
        {
            DoCmnd(19);
        }

        if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod || e.KeyCode == Keys.Oemcomma)
        {
            DoCmnd(16);
        }

        if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
        {
            DoCmnd(20);
        }
    }
}
```

### HistoryForm.cs
```csharp
using ConverterLab1;
using System.Drawing;
using System.Windows.Forms;

namespace ConverterLab1.WinForms;

public class HistoryForm : Form
{
    private readonly History history;
    private readonly TextBox textBox = new();

    public HistoryForm(History history)
    {
        this.history = history;

        Text = "История";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.SizableToolWindow;
        ClientSize = new Size(540, 320);

        textBox.Multiline = true;
        textBox.Dock = DockStyle.Fill;
        textBox.ScrollBars = ScrollBars.Vertical;
        textBox.ReadOnly = true;
        textBox.Font = new Font("Consolas", 10f, FontStyle.Regular);

        Controls.Add(textBox);

        Load += HistoryForm_Load;
    }

    private void HistoryForm_Load(object? sender, System.EventArgs e)
    {
        textBox.Clear();

        if (history.Count() == 0)
        {
            textBox.AppendText("История пуста");
            return;
        }

        for (var i = 0; i < history.Count(); i++)
        {
            textBox.AppendText(history[i].ToString());
        }
    }
}
```

### AboutForm.cs
```csharp
using System.Drawing;
using System.Windows.Forms;

namespace ConverterLab1.WinForms;

public class AboutForm : Form
{
    public AboutForm()
    {
        Text = "Справка";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(420, 180);

        var title = new Label();
        title.Text = "Конвертор p1_p2";
        title.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        title.AutoSize = true;
        title.Location = new Point(20, 20);

        var desc = new Label();
        desc.Text = "Преобразование действительных чисел\nиз системы счисления p1 в систему p2 (2..16).";
        desc.AutoSize = true;
        desc.Location = new Point(20, 65);

        var authors = new Label();
        authors.Text = "Лабораторная работа №1";
        authors.AutoSize = true;
        authors.Location = new Point(20, 120);

        Controls.Add(title);
        Controls.Add(desc);
        Controls.Add(authors);
    }
}
```

## 3. Тестовые наборы данных и результаты

### 3.1 Тестовые наборы данных

1. Перевод из `p` в `10`: вход `A5.E` при `p=16`, ожидаемый результат `165.875`.
2. Перевод отрицательного числа из `p` в `10`: вход `-1010.1` при `p=2`, ожидаемый результат `-10.5`.
3. Перевод из `10` в `p` (целая часть): вход `161` при `p=16`, ожидаемый результат `A1`.
4. Перевод дробной части из `10` в `p`: вход `0.9375`, `p=2`, `c=4`, ожидаемый результат `1111`.
5. Полное преобразование отрицательного числа: вход `-17.875`, `p=16`, `c=3`, ожидаемый результат `-11.E`.
6. Управление, отрицательное число: ввод `-10.5`, `p1=10`, `p2=16`, ожидаемый результат `-A.8`.

### 3.2 Результаты запуска тестов

```text
  Определение проектов для восстановления...
  Все проекты обновлены для восстановления.
  ConverterLab1 -> C:\Users\Admin\Desktop\LabsS6\po\build\dotnet\bin\ConverterLab1\Debug\net8.0\ConverterLab1.dll
  ConverterLab1.Tests -> C:\Users\Admin\Desktop\LabsS6\po\build\dotnet\bin\ConverterLab1.Tests\Debug\net8.0\ConverterLab1.Tests.dll
Тестовый запуск для C:\Users\Admin\Desktop\LabsS6\po\build\dotnet\bin\ConverterLab1.Tests\Debug\net8.0\ConverterLab1.Tests.dll (.NETCoreApp,Version=v8.0)
Версия VSTest 17.14.1 (x64)

Запуск выполнения тестов; подождите...
Общее количество тестовых файлов (1), соответствующих указанному шаблону.

Пройден!   : не пройдено     0, пройдено    22, пропущено     0, всего    22, длительность 64 ms. - ConverterLab1.Tests.dll (net8.0)
```

