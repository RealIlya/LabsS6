namespace BookCatalogWinForms
{
    partial class LoginForm
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
            var pnlMain = new System.Windows.Forms.TableLayoutPanel();
            var pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            var lblHeader = new System.Windows.Forms.Label();
            var lblEmail = new System.Windows.Forms.Label();
            txtEmail = new System.Windows.Forms.TextBox();
            var lblPassword = new System.Windows.Forms.Label();
            txtPassword = new System.Windows.Forms.TextBox();
            var btnLogin = new System.Windows.Forms.Button();
            var btnGuest = new System.Windows.Forms.Button();
            var btnRegister = new System.Windows.Forms.Button();
            var btnCancel = new System.Windows.Forms.Button();

            SuspendLayout();
            pnlMain.SuspendLayout();
            pnlButtons.SuspendLayout();

            // Заголовок
            lblHeader.Text = "// ВХОД В СИСТЕМУ";
            lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            lblHeader.Height = 40;
            lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblHeader.Font = new System.Drawing.Font("Consolas", 14F, System.Drawing.FontStyle.Bold);

            // Поля
            pnlMain.ColumnCount = 2;
            pnlMain.RowCount = 2;
            pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlMain.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);

            lblEmail.Text = "Email:";
            lblEmail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtEmail.Dock = System.Windows.Forms.DockStyle.Fill;

            lblPassword.Text = "Пароль:";
            lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            txtPassword.UseSystemPasswordChar = true;

            pnlMain.Controls.Add(lblEmail, 0, 0);
            pnlMain.Controls.Add(txtEmail, 1, 0);
            pnlMain.Controls.Add(lblPassword, 0, 1);
            pnlMain.Controls.Add(txtPassword, 1, 1);

            // Кнопки
            pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlButtons.Height = 45;
            pnlButtons.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            btnLogin.Text = "Войти";
            btnLogin.Size = new System.Drawing.Size(80, 30);
            btnLogin.Click += new System.EventHandler(btnLogin_Click);

            btnGuest.Text = "Как гость";
            btnGuest.Size = new System.Drawing.Size(90, 30);
            btnGuest.Click += new System.EventHandler(btnGuest_Click);

            btnRegister.Text = "Регистрация";
            btnRegister.Size = new System.Drawing.Size(100, 30);
            btnRegister.Click += new System.EventHandler(btnRegister_Click);

            btnCancel.Text = "Выход";
            btnCancel.Size = new System.Drawing.Size(80, 30);
            btnCancel.Click += new System.EventHandler(btnCancel_Click);

            pnlButtons.Controls.Add(btnLogin);
            pnlButtons.Controls.Add(btnGuest);
            pnlButtons.Controls.Add(btnRegister);
            pnlButtons.Controls.Add(btnCancel);

            // Форма
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(380, 180);
            Controls.Add(pnlMain);
            Controls.Add(lblHeader);
            Controls.Add(pnlButtons);
            Name = "LoginForm";
            Text = "Книжный каталог — Вход";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtPassword;
    }
}
