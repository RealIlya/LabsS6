namespace BookCatalogWinForms
{
    partial class AddBookForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // === Структурный принцип: группировка по смыслу ===
            var pnlFields = new System.Windows.Forms.TableLayoutPanel();
            var pnlButtons = new System.Windows.Forms.FlowLayoutPanel();

            var lblTitle = new System.Windows.Forms.Label();
            txtTitle = new System.Windows.Forms.TextBox();
            var lblAuthor = new System.Windows.Forms.Label();
            txtAuthor = new System.Windows.Forms.TextBox();
            var lblISBN = new System.Windows.Forms.Label();
            txtISBN = new System.Windows.Forms.TextBox();
            var lblYear = new System.Windows.Forms.Label();
            txtYear = new System.Windows.Forms.TextBox();
            var lblGenre = new System.Windows.Forms.Label();
            cmbGenre = new System.Windows.Forms.ComboBox();
            var lblPages = new System.Windows.Forms.Label();
            txtPages = new System.Windows.Forms.TextBox();
            var lblCount = new System.Windows.Forms.Label();
            txtCount = new System.Windows.Forms.TextBox();

            var btnSave = new System.Windows.Forms.Button();
            var btnCancel = new System.Windows.Forms.Button();

            SuspendLayout();
            pnlFields.SuspendLayout();
            pnlButtons.SuspendLayout();

            // === Таблица полей (принцип видимости: все поля видны сразу) ===
            pnlFields.ColumnCount = 2;
            pnlFields.RowCount = 7;
            pnlFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            pnlFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            for (int i = 0; i < 7; i++)
                pnlFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            pnlFields.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlFields.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);

            // Название
            lblTitle.Text = "Название:";
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtTitle.Dock = System.Windows.Forms.DockStyle.Fill;

            // Автор
            lblAuthor.Text = "Автор:";
            lblAuthor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtAuthor.Dock = System.Windows.Forms.DockStyle.Fill;

            // ISBN
            lblISBN.Text = "ISBN:";
            lblISBN.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtISBN.Dock = System.Windows.Forms.DockStyle.Fill;

            // Год
            lblYear.Text = "Год издания:";
            lblYear.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtYear.Dock = System.Windows.Forms.DockStyle.Fill;

            // Жанр
            lblGenre.Text = "Жанр:";
            lblGenre.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cmbGenre.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbGenre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbGenre.Items.AddRange(new object[] { "Художественная", "Научная", "Учебная", "Детская" });
            cmbGenre.SelectedIndex = 0;

            // Страницы
            lblPages.Text = "Страниц:";
            lblPages.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtPages.Dock = System.Windows.Forms.DockStyle.Fill;

            // Количество
            lblCount.Text = "Количество:";
            lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtCount.Dock = System.Windows.Forms.DockStyle.Fill;
            txtCount.Text = "1";

            pnlFields.Controls.Add(lblTitle, 0, 0); pnlFields.Controls.Add(txtTitle, 1, 0);
            pnlFields.Controls.Add(lblAuthor, 0, 1); pnlFields.Controls.Add(txtAuthor, 1, 1);
            pnlFields.Controls.Add(lblISBN, 0, 2); pnlFields.Controls.Add(txtISBN, 1, 2);
            pnlFields.Controls.Add(lblYear, 0, 3); pnlFields.Controls.Add(txtYear, 1, 3);
            pnlFields.Controls.Add(lblGenre, 0, 4); pnlFields.Controls.Add(cmbGenre, 1, 4);
            pnlFields.Controls.Add(lblPages, 0, 5); pnlFields.Controls.Add(txtPages, 1, 5);
            pnlFields.Controls.Add(lblCount, 0, 6); pnlFields.Controls.Add(txtCount, 1, 6);

            // === Кнопки (принцип простоты) ===
            pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            pnlButtons.Size = new System.Drawing.Size(400, 40);
            pnlButtons.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            btnSave.Text = "Сохранить";
            btnSave.Size = new System.Drawing.Size(90, 28);
            btnSave.Click += new System.EventHandler(btnSave_Click);

            btnCancel.Text = "Отмена";
            btnCancel.Size = new System.Drawing.Size(80, 28);
            btnCancel.Click += new System.EventHandler(btnCancel_Click);

            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Controls.Add(btnCancel);

            // === Форма ===
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(430, 380);
            Controls.Add(pnlFields);
            Controls.Add(pnlButtons);
            Name = "AddBookForm";
            Text = "Добавление книги";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            pnlFields.ResumeLayout(false);
            pnlFields.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.TextBox txtISBN;
        private System.Windows.Forms.TextBox txtYear;
        private System.Windows.Forms.ComboBox cmbGenre;
        private System.Windows.Forms.TextBox txtPages;
        private System.Windows.Forms.TextBox txtCount;
    }
}
