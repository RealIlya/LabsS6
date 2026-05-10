using System;
using System.Windows.Forms;

namespace BookCatalogWinForms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            Theme.Apply(this);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Введите email", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            var user = DataStore.Login(email, password);
            if (user == null)
            {
                MessageBox.Show("Неверный email или пароль", "Ошибка входа",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Text = "";
                txtPassword.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnGuest_Click(object sender, EventArgs e)
        {
            // Вход как гость — без логина
            DataStore.Login("guest@test.com", "guest");
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            var form = new RegisterForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
