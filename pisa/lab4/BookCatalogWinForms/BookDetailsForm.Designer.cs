namespace BookCatalogWinForms
{
    partial class BookDetailsForm
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
            // === Структурный принцип: табличное расположение меток и значений ===
            var pnlInfo = new System.Windows.Forms.TableLayoutPanel();
            var pnlButtons = new System.Windows.Forms.FlowLayoutPanel();

            var lblTitle = new System.Windows.Forms.Label();
            lblTitleValue = new System.Windows.Forms.Label();
            var lblAuthor = new System.Windows.Forms.Label();
            lblAuthorValue = new System.Windows.Forms.Label();
            var lblISBN = new System.Windows.Forms.Label();
            lblISBNValue = new System.Windows.Forms.Label();
            var lblYear = new System.Windows.Forms.Label();
            lblYearValue = new System.Windows.Forms.Label();
            var lblGenre = new System.Windows.Forms.Label();
            lblGenreValue = new System.Windows.Forms.Label();
            var lblPages = new System.Windows.Forms.Label();
            lblPagesValue = new System.Windows.Forms.Label();
            var lblTotal = new System.Windows.Forms.Label();
            lblTotalValue = new System.Windows.Forms.Label();
            var lblAvailable = new System.Windows.Forms.Label();
            lblAvailableValue = new System.Windows.Forms.Label();

            btnReserve = new System.Windows.Forms.Button();
            var btnClose = new System.Windows.Forms.Button();

            SuspendLayout();
            pnlInfo.SuspendLayout();
            pnlButtons.SuspendLayout();

            // === Информация о книге ===
            pnlInfo.ColumnCount = 2;
            pnlInfo.RowCount = 8;
            pnlInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            pnlInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            pnlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlInfo.Padding = new System.Windows.Forms.Padding(15);

            // Принцип видимости: метки слева, значения справа
            lblTitle.Text = "Название:";
            lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            lblTitleValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);

            lblAuthor.Text = "Автор:";
            lblAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            lblAuthorValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);

            lblISBN.Text = "ISBN:";
            lblISBN.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            lblISBNValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);

            lblYear.Text = "Год издания:";
            lblYear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);

            lblGenre.Text = "Жанр:";
            lblGenre.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);

            lblPages.Text = "Страниц:";
            lblPages.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);

            lblTotal.Text = "Всего экземпляров:";
            lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);

            lblAvailable.Text = "Доступно:";
            lblAvailable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            lblAvailableValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);

            // Принцип повторного использования: единый стиль выравнивания
            foreach (var lbl in new[] { lblTitle, lblAuthor, lblISBN, lblYear, lblGenre, lblPages, lblTotal, lblAvailable })
                lbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            foreach (var lbl in new[] { lblTitleValue, lblAuthorValue, lblISBNValue, lblYearValue, lblGenreValue, lblPagesValue, lblTotalValue, lblAvailableValue })
                lbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            pnlInfo.Controls.Add(lblTitle, 0, 0); pnlInfo.Controls.Add(lblTitleValue, 1, 0);
            pnlInfo.Controls.Add(lblAuthor, 0, 1); pnlInfo.Controls.Add(lblAuthorValue, 1, 1);
            pnlInfo.Controls.Add(lblISBN, 0, 2); pnlInfo.Controls.Add(lblISBNValue, 1, 2);
            pnlInfo.Controls.Add(lblYear, 0, 3); pnlInfo.Controls.Add(lblYearValue, 1, 3);
            pnlInfo.Controls.Add(lblGenre, 0, 4); pnlInfo.Controls.Add(lblGenreValue, 1, 4);
            pnlInfo.Controls.Add(lblPages, 0, 5); pnlInfo.Controls.Add(lblPagesValue, 1, 5);
            pnlInfo.Controls.Add(lblTotal, 0, 6); pnlInfo.Controls.Add(lblTotalValue, 1, 6);
            pnlInfo.Controls.Add(lblAvailable, 0, 7); pnlInfo.Controls.Add(lblAvailableValue, 1, 7);

            // === Кнопки ===
            pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            pnlButtons.Size = new System.Drawing.Size(400, 40);
            pnlButtons.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            btnReserve.Text = "Забронировать";
            btnReserve.Size = new System.Drawing.Size(110, 28);
            btnReserve.Click += new System.EventHandler(btnReserve_Click);

            btnClose.Text = "Закрыть";
            btnClose.Size = new System.Drawing.Size(80, 28);
            btnClose.Click += new System.EventHandler(btnClose_Click);

            pnlButtons.Controls.Add(btnReserve);
            pnlButtons.Controls.Add(btnClose);

            // === Форма ===
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(420, 320);
            Controls.Add(pnlInfo);
            Controls.Add(pnlButtons);
            Name = "BookDetailsForm";
            Text = "Информация о книге";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            pnlInfo.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblTitleValue;
        private System.Windows.Forms.Label lblAuthorValue;
        private System.Windows.Forms.Label lblISBNValue;
        private System.Windows.Forms.Label lblYearValue;
        private System.Windows.Forms.Label lblGenreValue;
        private System.Windows.Forms.Label lblPagesValue;
        private System.Windows.Forms.Label lblTotalValue;
        private System.Windows.Forms.Label lblAvailableValue;
        private System.Windows.Forms.Button btnReserve;
    }
}
