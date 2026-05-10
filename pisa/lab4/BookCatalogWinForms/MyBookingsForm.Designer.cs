namespace BookCatalogWinForms
{
    partial class MyBookingsForm
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
            this.lblInfo = new System.Windows.Forms.Label();
            this.dgvBookings = new System.Windows.Forms.DataGridView();
            this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAuthor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colISBN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)this.dgvBookings).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();

            // === Информация сверху ===
            this.lblInfo.Text = "Загрузка...";
            this.lblInfo.Location = new System.Drawing.Point(12, 12);
            this.lblInfo.AutoSize = true;

            // === Таблица бронирований ===
            this.dgvBookings.AllowUserToAddRows = false;
            this.dgvBookings.AllowUserToDeleteRows = false;
            this.dgvBookings.ReadOnly = true;
            this.dgvBookings.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBookings.MultiSelect = false;
            this.dgvBookings.RowHeadersVisible = false;
            this.dgvBookings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBookings.Location = new System.Drawing.Point(12, 35);
            this.dgvBookings.Size = new System.Drawing.Size(660, 300);
            this.dgvBookings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.dgvBookings.SelectionChanged += new System.EventHandler(this.dgvBookings_SelectionChanged);

            this.colTitle.HeaderText = "Название";
            this.colTitle.DataPropertyName = "BookTitle";
            this.colTitle.FillWeight = 120;
            this.colAuthor.HeaderText = "Автор";
            this.colAuthor.DataPropertyName = "BookAuthor";
            this.colAuthor.FillWeight = 100;
            this.colISBN.HeaderText = "ISBN";
            this.colISBN.DataPropertyName = "BookISBN";
            this.colISBN.FillWeight = 80;
            this.colDate.HeaderText = "Дата бронирования";
            this.colDate.DataPropertyName = "BookingDate";
            this.colDate.FillWeight = 70;
            this.colDate.DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";

            this.dgvBookings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colTitle, this.colAuthor, this.colISBN, this.colDate
            });

            // === Кнопки ===
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlButtons.Location = new System.Drawing.Point(0, 345);
            this.pnlButtons.Size = new System.Drawing.Size(684, 45);
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            this.btnCancel.Text = "Отменить бронирование";
            this.btnCancel.Size = new System.Drawing.Size(170, 30);
            this.btnCancel.Enabled = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            this.btnClose.Text = "Назад";
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Controls.Add(this.btnClose);

            // === Форма ===
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 390);
            this.Controls.Add(this.dgvBookings);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.pnlButtons);
            this.Name = "MyBookingsForm";
            this.Text = "Мои бронирования";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(500, 300);

            ((System.ComponentModel.ISupportInitialize)this.dgvBookings).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.DataGridView dgvBookings;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAuthor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colISBN;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
    }
}
