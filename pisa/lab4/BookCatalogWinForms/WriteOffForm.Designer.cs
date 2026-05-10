namespace BookCatalogWinForms
{
    partial class WriteOffForm
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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnBack = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();

            this.pnlBookList = new System.Windows.Forms.GroupBox();
            this.dgvBooks = new System.Windows.Forms.DataGridView();
            this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAuthor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colYear = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGenre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAvailable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBooked = new System.Windows.Forms.DataGridViewTextBoxColumn();

            this.pnlWriteOff = new System.Windows.Forms.GroupBox();
            this.lblBookInfo = new System.Windows.Forms.Label();
            this.lblReason = new System.Windows.Forms.Label();
            this.cmbReason = new System.Windows.Forms.ComboBox();
            this.lblCount = new System.Windows.Forms.Label();
            this.txtCount = new System.Windows.Forms.TextBox();
            this.btnWriteOff = new System.Windows.Forms.Button();

            this.pnlArchive = new System.Windows.Forms.GroupBox();
            this.lblArchiveInfo = new System.Windows.Forms.Label();
            this.dgvArchive = new System.Windows.Forms.DataGridView();
            this.colRecTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecAuthor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRestore = new System.Windows.Forms.Button();

            this.pnlTop.SuspendLayout();
            this.pnlBookList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.dgvBooks).BeginInit();
            this.pnlWriteOff.SuspendLayout();
            this.pnlArchive.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.dgvArchive).BeginInit();
            this.SuspendLayout();

            // === Верхняя панель ===
            this.pnlTop.Controls.Add(this.btnBack);
            this.pnlTop.Controls.Add(this.lblTitle);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Size = new System.Drawing.Size(900, 45);
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            this.lblTitle.Text = "СПИСАНИЕ КНИГ";
            this.lblTitle.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(10, 12);
            this.lblTitle.AutoSize = true;

            this.btnBack.Text = "Назад";
            this.btnBack.Size = new System.Drawing.Size(80, 28);
            this.btnBack.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnBack.Location = new System.Drawing.Point(810, 8);
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);

            // === Список книг ===
            this.pnlBookList.Text = "Каталог книг";
            this.pnlBookList.Controls.Add(this.dgvBooks);
            this.pnlBookList.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBookList.Location = new System.Drawing.Point(0, 45);
            this.pnlBookList.Size = new System.Drawing.Size(900, 230);
            this.pnlBookList.Padding = new System.Windows.Forms.Padding(5);

            this.dgvBooks.AllowUserToAddRows = false;
            this.dgvBooks.AllowUserToDeleteRows = false;
            this.dgvBooks.ReadOnly = true;
            this.dgvBooks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBooks.MultiSelect = false;
            this.dgvBooks.RowHeadersVisible = false;
            this.dgvBooks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBooks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBooks.SelectionChanged += new System.EventHandler(this.dgvBooks_SelectionChanged);
            this.dgvBooks.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvBooks_CellFormatting);

            this.colTitle.HeaderText = "Название";
            this.colTitle.DataPropertyName = "Title";
            this.colTitle.FillWeight = 100;
            this.colAuthor.HeaderText = "Автор";
            this.colAuthor.DataPropertyName = "Author";
            this.colAuthor.FillWeight = 80;
            this.colYear.HeaderText = "Год";
            this.colYear.DataPropertyName = "Year";
            this.colYear.FillWeight = 40;
            this.colGenre.HeaderText = "Жанр";
            this.colGenre.DataPropertyName = "Genre";
            this.colGenre.FillWeight = 60;
            this.colTotal.HeaderText = "Всего";
            this.colTotal.DataPropertyName = "TotalCount";
            this.colTotal.FillWeight = 35;
            this.colAvailable.HeaderText = "Доступно";
            this.colAvailable.DataPropertyName = "AvailableCount";
            this.colAvailable.FillWeight = 45;
            this.colBooked.HeaderText = "Забр.";
            this.colBooked.FillWeight = 35;

            this.dgvBooks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colTitle, this.colAuthor, this.colYear, this.colGenre,
                this.colTotal, this.colAvailable, this.colBooked
            });

            // === Панель списания ===
            this.pnlWriteOff.Text = "Списание";
            this.pnlWriteOff.Controls.Add(this.btnWriteOff);
            this.pnlWriteOff.Controls.Add(this.txtCount);
            this.pnlWriteOff.Controls.Add(this.lblCount);
            this.pnlWriteOff.Controls.Add(this.cmbReason);
            this.pnlWriteOff.Controls.Add(this.lblReason);
            this.pnlWriteOff.Controls.Add(this.lblBookInfo);
            this.pnlWriteOff.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlWriteOff.Location = new System.Drawing.Point(0, 275);
            this.pnlWriteOff.Size = new System.Drawing.Size(900, 120);
            this.pnlWriteOff.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            this.lblBookInfo.Text = "Выберите книгу в таблице выше";
            this.lblBookInfo.Location = new System.Drawing.Point(13, 22);
            this.lblBookInfo.Size = new System.Drawing.Size(870, 32);
            this.lblBookInfo.AutoSize = false;

            this.lblReason.Text = "Причина:";
            this.lblReason.Location = new System.Drawing.Point(13, 60);
            this.lblReason.AutoSize = true;

            this.cmbReason.Location = new System.Drawing.Point(80, 57);
            this.cmbReason.Size = new System.Drawing.Size(200, 21);
            this.cmbReason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbReason.Items.AddRange(new object[] { "Износ / повреждение", "Устаревание", "Утрата / пропажа" });

            this.lblCount.Text = "Кол-во:";
            this.lblCount.Location = new System.Drawing.Point(300, 60);
            this.lblCount.AutoSize = true;

            this.txtCount.Location = new System.Drawing.Point(355, 57);
            this.txtCount.Size = new System.Drawing.Size(60, 20);
            this.txtCount.Text = "1";

            this.btnWriteOff.Text = "Списать";
            this.btnWriteOff.Size = new System.Drawing.Size(100, 28);
            this.btnWriteOff.Location = new System.Drawing.Point(430, 55);
            this.btnWriteOff.Enabled = false;
            this.btnWriteOff.Click += new System.EventHandler(this.btnWriteOff_Click);

            // === Архив списанных ===
            this.pnlArchive.Text = "Архив списанных книг";
            this.pnlArchive.Controls.Add(this.btnRestore);
            this.pnlArchive.Controls.Add(this.lblArchiveInfo);
            this.pnlArchive.Controls.Add(this.dgvArchive);
            this.pnlArchive.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlArchive.Location = new System.Drawing.Point(0, 395);
            this.pnlArchive.Size = new System.Drawing.Size(900, 200);
            this.pnlArchive.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            this.lblArchiveInfo.Text = "Архив пуст";
            this.lblArchiveInfo.Location = new System.Drawing.Point(13, 20);
            this.lblArchiveInfo.AutoSize = true;

            this.btnRestore.Text = "Восстановить";
            this.btnRestore.Size = new System.Drawing.Size(120, 28);
            this.btnRestore.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnRestore.Location = new System.Drawing.Point(770, 16);
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);

            this.dgvArchive.AllowUserToAddRows = false;
            this.dgvArchive.AllowUserToDeleteRows = false;
            this.dgvArchive.ReadOnly = true;
            this.dgvArchive.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvArchive.MultiSelect = false;
            this.dgvArchive.RowHeadersVisible = false;
            this.dgvArchive.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvArchive.Location = new System.Drawing.Point(10, 42);
            this.dgvArchive.Size = new System.Drawing.Size(880, 150);
            this.dgvArchive.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            this.colRecTitle.HeaderText = "Название";
            this.colRecTitle.DataPropertyName = "BookTitle";
            this.colRecTitle.FillWeight = 100;
            this.colRecAuthor.HeaderText = "Автор";
            this.colRecAuthor.DataPropertyName = "BookAuthor";
            this.colRecAuthor.FillWeight = 80;
            this.colRecCount.HeaderText = "Кол-во";
            this.colRecCount.DataPropertyName = "Count";
            this.colRecCount.FillWeight = 40;
            this.colRecReason.HeaderText = "Причина";
            this.colRecReason.DataPropertyName = "Reason";
            this.colRecReason.FillWeight = 80;
            this.colRecDate.HeaderText = "Дата";
            this.colRecDate.DataPropertyName = "WriteOffDate";
            this.colRecDate.FillWeight = 60;

            this.dgvArchive.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colRecTitle, this.colRecAuthor, this.colRecCount,
                this.colRecReason, this.colRecDate
            });

            // === Форма ===
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.pnlArchive);
            this.Controls.Add(this.pnlWriteOff);
            this.Controls.Add(this.pnlBookList);
            this.Controls.Add(this.pnlTop);
            this.Name = "WriteOffForm";
            this.Text = "Списание книг";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(700, 500);

            this.pnlTop.ResumeLayout(false);
            this.pnlBookList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.dgvBooks).EndInit();
            this.pnlWriteOff.ResumeLayout(false);
            this.pnlWriteOff.PerformLayout();
            this.pnlArchive.ResumeLayout(false);
            this.pnlArchive.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.dgvArchive).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label lblTitle;

        private System.Windows.Forms.GroupBox pnlBookList;
        private System.Windows.Forms.DataGridView dgvBooks;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colYear;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGenre;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTotal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAvailable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBooked;

        private System.Windows.Forms.GroupBox pnlWriteOff;
        private System.Windows.Forms.Label lblBookInfo;
        private System.Windows.Forms.Label lblReason;
        private System.Windows.Forms.ComboBox cmbReason;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.TextBox txtCount;
        private System.Windows.Forms.Button btnWriteOff;

        private System.Windows.Forms.GroupBox pnlArchive;
        private System.Windows.Forms.Label lblArchiveInfo;
        private System.Windows.Forms.DataGridView dgvArchive;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecReason;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecDate;
        private System.Windows.Forms.Button btnRestore;
    }
}
