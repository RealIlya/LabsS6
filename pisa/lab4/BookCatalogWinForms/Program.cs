using System;
using System.Windows.Forms;

namespace BookCatalogWinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Сначала показываем форму входа
            var loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK && DataStore.CurrentUser != null)
            {
                // После успешного входа — главная форма
                Application.Run(new MainForm());
            }
        }
    }
}
