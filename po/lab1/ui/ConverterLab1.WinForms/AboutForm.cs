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
