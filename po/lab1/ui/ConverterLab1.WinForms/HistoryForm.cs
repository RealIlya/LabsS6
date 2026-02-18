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
