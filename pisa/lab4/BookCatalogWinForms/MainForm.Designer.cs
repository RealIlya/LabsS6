namespace BookCatalogWinForms
{
    partial class MainForm
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
            // === Структурный принцип: логическая группировка элементов ===

            // Панель поиска (сверху)
            pnlSearch = new System.Windows.Forms.GroupBox();
            lblTitle = new System.Windows.Forms.Label();
            txtTitle = new System.Windows.Forms.TextBox();
            lblAuthor = new System.Windows.Forms.Label();
            txtAuthor = new System.Windows.Forms.TextBox();
            lblGenre = new System.Windows.Forms.Label();
            cmbGenre = new System.Windows.Forms.ComboBox();
            lblYearFrom = new System.Windows.Forms.Label();
            txtYearFrom = new System.Windows.Forms.TextBox();
            lblYearTo = new System.Windows.Forms.Label();
            txtYearTo = new System.Windows.Forms.TextBox();
            btnSearch = new System.Windows.Forms.Button();
            btnReset = new System.Windows.Forms.Button();

            // Таблица результатов (центр)
            dgvBooks = new System.Windows.Forms.DataGridView();

            // Панель действий (снизу)
            pnlActions = new System.Windows.Forms.FlowLayoutPanel();
            btnDetails = new System.Windows.Forms.Button();
            btnReserve = new System.Windows.Forms.Button();
            btnAddBook = new System.Windows.Forms.Button();
            btnWriteOff = new System.Windows.Forms.Button();
            btnLogin = new System.Windows.Forms.Button();
            btnRegister = new System.Windows.Forms.Button();
            btnLogout = new System.Windows.Forms.Button();
            btnMyBookings = new System.Windows.Forms.Button();

            // Строка состояния
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();

            pnlSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBooks).BeginInit();
            pnlActions.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // === Панель поиска ===
            pnlSearch.Controls.Add(lblTitle);
            pnlSearch.Controls.Add(txtTitle);
            pnlSearch.Controls.Add(lblAuthor);
            pnlSearch.Controls.Add(txtAuthor);
            pnlSearch.Controls.Add(lblGenre);
            pnlSearch.Controls.Add(cmbGenre);
            pnlSearch.Controls.Add(lblYearFrom);
            pnlSearch.Controls.Add(txtYearFrom);
            pnlSearch.Controls.Add(lblYearTo);
            pnlSearch.Controls.Add(txtYearTo);
            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(btnReset);
            pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            pnlSearch.Location = new System.Drawing.Point(0, 0);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Padding = new System.Windows.Forms.Padding(10);
            pnlSearch.Size = new System.Drawing.Size(900, 100);
            pnlSearch.TabIndex = 0;
            pnlSearch.Text = "Поиск книг";

            // Название
            lblTitle.AutoSize = true;
            lblTitle.Location = new System.Drawing.Point(13, 25);
            lblTitle.Text = "Название:";
            txtTitle.Location = new System.Drawing.Point(80, 22);
            txtTitle.Size = new System.Drawing.Size(150, 20);

            // Автор
            lblAuthor.AutoSize = true;
            lblAuthor.Location = new System.Drawing.Point(240, 25);
            lblAuthor.Text = "Автор:";
            txtAuthor.Location = new System.Drawing.Point(290, 22);
            txtAuthor.Size = new System.Drawing.Size(130, 20);

            // Жанр
            lblGenre.AutoSize = true;
            lblGenre.Location = new System.Drawing.Point(430, 25);
            lblGenre.Text = "Жанр:";
            cmbGenre.Location = new System.Drawing.Point(475, 22);
            cmbGenre.Size = new System.Drawing.Size(120, 21);
            cmbGenre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbGenre.Items.AddRange(new object[] { "Художественная", "Научная", "Учебная", "Детская" });

            // Год от
            lblYearFrom.AutoSize = true;
            lblYearFrom.Location = new System.Drawing.Point(13, 58);
            lblYearFrom.Text = "Год от:";
            txtYearFrom.Location = new System.Drawing.Point(60, 55);
            txtYearFrom.Size = new System.Drawing.Size(50, 20);

            // Год до
            lblYearTo.AutoSize = true;
            lblYearTo.Location = new System.Drawing.Point(120, 58);
            lblYearTo.Text = "до:";
            txtYearTo.Location = new System.Drawing.Point(142, 55);
            txtYearTo.Size = new System.Drawing.Size(50, 20);

            // Кнопки поиска
            btnSearch.Location = new System.Drawing.Point(250, 52);
            btnSearch.Size = new System.Drawing.Size(100, 28);
            btnSearch.Text = "Поиск";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += new System.EventHandler(btnSearch_Click);

            btnReset.Location = new System.Drawing.Point(360, 52);
            btnReset.Size = new System.Drawing.Size(80, 28);
            btnReset.Text = "Сброс";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += new System.EventHandler(btnReset_Click);

            // === Таблица книг ===
            dgvBooks.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvBooks.AllowUserToAddRows = false;
            dgvBooks.AllowUserToDeleteRows = false;
            dgvBooks.ReadOnly = true;
            dgvBooks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvBooks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvBooks.MultiSelect = false;
            dgvBooks.RowHeadersVisible = false;
            dgvBooks.Name = "dgvBooks";
            dgvBooks.Location = new System.Drawing.Point(0, 100);
            dgvBooks.Size = new System.Drawing.Size(900, 350);
            dgvBooks.TabIndex = 1;
            dgvBooks.SelectionChanged += new System.EventHandler(dgvBooks_SelectionChanged);

            // === Панель действий ===
            pnlActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlActions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            pnlActions.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            pnlActions.Size = new System.Drawing.Size(900, 45);
            pnlActions.Name = "pnlActions";

            // Принцип простоты: понятные названия кнопок
            btnDetails.Text = "Подробнее";
            btnDetails.Size = new System.Drawing.Size(100, 30);
            btnDetails.Enabled = false;
            btnDetails.Click += new System.EventHandler(btnDetails_Click);

            btnReserve.Text = "Забронировать";
            btnReserve.Size = new System.Drawing.Size(120, 30);
            btnReserve.Enabled = false;
            btnReserve.Click += new System.EventHandler(btnReserve_Click);

            btnAddBook.Text = "Добавить книгу";
            btnAddBook.Size = new System.Drawing.Size(120, 30);
            btnAddBook.Click += new System.EventHandler(btnAddBook_Click);

            btnWriteOff.Text = "Списание";
            btnWriteOff.Size = new System.Drawing.Size(90, 30);
            btnWriteOff.Click += new System.EventHandler(btnWriteOff_Click);

            btnLogin.Text = "Войти";
            btnLogin.Size = new System.Drawing.Size(80, 30);
            btnLogin.Click += new System.EventHandler(btnLogin_Click);

            btnRegister.Text = "Регистрация";
            btnRegister.Size = new System.Drawing.Size(100, 30);
            btnRegister.Click += new System.EventHandler(btnRegister_Click);

            btnLogout.Text = "Выйти";
            btnLogout.Size = new System.Drawing.Size(80, 30);
            btnLogout.Click += new System.EventHandler(btnLogout_Click);

            btnMyBookings.Text = "Мои бронирования";
            btnMyBookings.Size = new System.Drawing.Size(130, 30);
            btnMyBookings.Click += new System.EventHandler(btnMyBookings_Click);

            pnlActions.Controls.Add(btnDetails);
            pnlActions.Controls.Add(btnReserve);
            pnlActions.Controls.Add(btnAddBook);
            pnlActions.Controls.Add(btnWriteOff);
            pnlActions.Controls.Add(btnMyBookings);
            pnlActions.Controls.Add(btnLogin);
            pnlActions.Controls.Add(btnRegister);
            pnlActions.Controls.Add(btnLogout);

            // === Строка состояния (принцип обратной связи) ===
            lblStatus.Text = "Готово";
            statusStrip.Items.Add(lblStatus);
            statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;

            // === Форма ===
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(900, 500);
            Controls.Add(dgvBooks);
            Controls.Add(pnlSearch);
            Controls.Add(pnlActions);
            Controls.Add(statusStrip);
            Name = "MainForm";
            Text = "Книжный каталог";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            pnlSearch.ResumeLayout(false);
            pnlSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBooks).EndInit();
            pnlActions.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.GroupBox pnlSearch;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.ComboBox cmbGenre;
        private System.Windows.Forms.Label lblYearFrom;
        private System.Windows.Forms.TextBox txtYearFrom;
        private System.Windows.Forms.Label lblYearTo;
        private System.Windows.Forms.TextBox txtYearTo;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.DataGridView dgvBooks;
        private System.Windows.Forms.FlowLayoutPanel pnlActions;
        private System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.Button btnReserve;
        private System.Windows.Forms.Button btnAddBook;
        private System.Windows.Forms.Button btnWriteOff;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button btnMyBookings;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
