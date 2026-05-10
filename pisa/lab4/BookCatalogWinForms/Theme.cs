using System.Drawing;
using System.Windows.Forms;

namespace BookCatalogWinForms
{
    /// <summary>
    /// Тема NieR: Automata — Light/Ruins Edition.
    /// Точные цвета из CSS: --nier-void: #c9c5b8, --nier-dark: #b8b4a7,
    /// --nier-shadow: #a7a396, --nier-white: #0a0a0a, --nier-cream: #141410
    /// </summary>
    public static class Theme
    {
        // Фоны (светлые)
        public static readonly Color Background = Color.FromArgb(201, 197, 184);    // --nier-void #c9c5b8
        public static readonly Color PanelBack = Color.FromArgb(184, 180, 167);     // --nier-dark #b8b4a7
        public static readonly Color InputBack = Color.FromArgb(220, 218, 210);     // чуть светлее фона
        public static readonly Color HeaderBack = Color.FromArgb(167, 163, 150);    // --nier-shadow #a7a396
        public static readonly Color CellBack = Color.FromArgb(190, 186, 173);      // чуть темнее фона
        public static readonly Color CellSelected = Color.FromArgb(200, 169, 110);  // --nier-gold #c8a96e

        // Текст (тёмный)
        public static readonly Color TextPrimary = Color.FromArgb(10, 10, 10);      // --nier-white #0a0a0a
        public static readonly Color TextSecondary = Color.FromArgb(20, 20, 16);    // --nier-cream #141410
        public static readonly Color TextMuted = Color.FromArgb(42, 38, 32);        // --nier-pale #2a2620
        public static readonly Color TextGray = Color.FromArgb(138, 134, 121);      // --nier-gray #8a8679

        // Акцент
        public static readonly Color Gold = Color.FromArgb(200, 169, 110);          // --nier-gold #c8a96e
        public static readonly Color GoldDark = Color.FromArgb(160, 130, 80);       // темнее золота
        public static readonly Color Success = Color.FromArgb(42, 90, 42);
        public static readonly Color Error = Color.FromArgb(106, 26, 26);

        // Шрифт (моноширинный — как в оригинале)
        public static readonly Font DefaultFont = new Font("Consolas", 9F);
        public static readonly Font HeaderFont = new Font("Consolas", 9F, FontStyle.Bold);
        public static readonly Font TitleFont = new Font("Consolas", 12F, FontStyle.Bold);

        public static void Apply(Form form)
        {
            form.BackColor = Background;
            form.ForeColor = TextPrimary;
            form.Font = DefaultFont;
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is TextBox tb)
                {
                    tb.BackColor = InputBack;
                    tb.ForeColor = TextPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.Font = DefaultFont;
                }
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = TextPrimary;
                    lbl.Font = DefaultFont;
                    lbl.BackColor = Color.Transparent;
                }
                else if (ctrl is Button btn)
                {
                    btn.BackColor = PanelBack;
                    btn.ForeColor = TextPrimary;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = TextGray;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.Font = HeaderFont;
                    btn.Cursor = Cursors.Hand;
                }
                else if (ctrl is GroupBox gb)
                {
                    gb.BackColor = Background;
                    gb.ForeColor = TextSecondary;
                    gb.Font = HeaderFont;
                }
                else if (ctrl is ComboBox cmb)
                {
                    cmb.BackColor = InputBack;
                    cmb.ForeColor = TextPrimary;
                    cmb.Font = DefaultFont;
                    cmb.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is DataGridView dgv)
                {
                    dgv.BackgroundColor = Background;
                    dgv.ForeColor = TextPrimary;
                    dgv.GridColor = TextGray;
                    dgv.Font = DefaultFont;
                    dgv.BorderStyle = BorderStyle.FixedSingle;
                    dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

                    // Ячейки
                    dgv.DefaultCellStyle.BackColor = CellBack;
                    dgv.DefaultCellStyle.ForeColor = TextPrimary;
                    dgv.DefaultCellStyle.SelectionBackColor = CellSelected;
                    dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
                    dgv.DefaultCellStyle.Font = DefaultFont;

                    // Заголовки столбцов
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = HeaderBack;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
                    dgv.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
                    dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                    dgv.EnableHeadersVisualStyles = false;

                    // Строка-заголовок
                    dgv.RowHeadersDefaultCellStyle.BackColor = HeaderBack;
                    dgv.RowHeadersDefaultCellStyle.ForeColor = TextSecondary;

                    dgv.AlternatingRowsDefaultCellStyle.BackColor = PanelBack;
                }
                else if (ctrl is StatusStrip ss)
                {
                    ss.BackColor = PanelBack;
                    ss.ForeColor = TextSecondary;
                    ss.Font = DefaultFont;
                    ss.SizingGrip = false;
                    foreach (ToolStripItem item in ss.Items)
                    {
                        item.ForeColor = TextSecondary;
                        item.Font = DefaultFont;
                    }
                }
                else if (ctrl is FlowLayoutPanel flp)
                {
                    flp.BackColor = Background;
                }
                else if (ctrl is TableLayoutPanel tlp)
                {
                    tlp.BackColor = Background;
                }

                if (ctrl.Controls.Count > 0)
                    ApplyToControls(ctrl.Controls);
            }
        }
    }
}
