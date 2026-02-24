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

        if (ctl.St != Control_.State.Преобразовано)
        {
            labelOutput.Text = "0";
            return;
        }

        try
        {
            labelOutput.Text = ctl.RecalculateWithoutHistory();
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
