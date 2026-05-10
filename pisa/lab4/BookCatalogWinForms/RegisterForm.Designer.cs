namespace BookCatalogWinForms
{
    partial class RegisterForm
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
            var pnlFields = new System.Windows.Forms.TableLayoutPanel();
            var pnlButtons = new System.Windows.Forms.FlowLayoutPanel();

            var lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            var lblEmail = new System.Windows.Forms.Label();
            txtEmail = new System.Windows.Forms.TextBox();
            var lblPassword = new System.Windows.Forms.Label();
            txtPassword = new System.Windows.Forms.TextBox();
            var lblConfirm = new System.Windows.Forms.Label();
            txtConfirm = new System.Windows.Forms.TextBox();

            var btnRegister = new System.Windows.Forms.Button();
            var btnCancel = new System.Windows.Forms.Button();

            SuspendLayout();
            pnlFields.SuspendLayout();
            pnlButtons.SuspendLayout();

            pnlFields.ColumnCount = 2;
            pnlFields.RowCount = 4;
            pnlFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            pnlFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            pnlFields.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlFields.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);

            lblName.Text = "Имя:";
            lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtName.Dock = System.Windows.Forms.DockStyle.Fill;

            lblEmail.Text = "Email:";
            lblEmail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtEmail.Dock = System.Windows.Forms.DockStyle.Fill;

            lblPassword.Text = "Пароль:";
            lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            txtPassword.UseSystemPasswordChar = true;

            lblConfirm.Text = "Подтвердите:";
            lblConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            txtConfirm.Dock = System.Windows.Forms.DockStyle.Fill;
            txtConfirm.UseSystemPasswordChar = true;

            pnlFields.Controls.Add(lblName, 0, 0); pnlFields.Controls.Add(txtName, 1, 0);
            pnlFields.Controls.Add(lblEmail, 0, 1); pnlFields.Controls.Add(txtEmail, 1, 1);
            pnlFields.Controls.Add(lblPassword, 0, 2); pnlFields.Controls.Add(txtPassword, 1, 2);
            pnlFields.Controls.Add(lblConfirm, 0, 3); pnlFields.Controls.Add(txtConfirm, 1, 3);

            pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            pnlButtons.Size = new System.Drawing.Size(380, 40);
            pnlButtons.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);

            btnRegister.Text = "Зарегистрироваться";
            btnRegister.Size = new System.Drawing.Size(140, 28);
            btnRegister.Click += new System.EventHandler(btnRegister_Click);

            btnCancel.Text = "Отмена";
            btnCancel.Size = new System.Drawing.Size(80, 28);
            btnCancel.Click += new System.EventHandler(btnCancel_Click);

            pnlButtons.Controls.Add(btnRegister);
            pnlButtons.Controls.Add(btnCancel);

            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(380, 220);
            Controls.Add(pnlFields);
            Controls.Add(pnlButtons);
            Name = "RegisterForm";
            Text = "Регистрация";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            pnlFields.ResumeLayout(false);
            pnlFields.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtConfirm;
    }
}
