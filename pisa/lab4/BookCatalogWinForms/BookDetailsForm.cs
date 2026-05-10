using System;
using System.Windows.Forms;

namespace BookCatalogWinForms
{
    public partial class BookDetailsForm : Form
    {
        private Book _book;

        public BookDetailsForm(Book book)
        {
            _book = book;
            InitializeComponent();
            Theme.Apply(this);
            DisplayBook();
        }

        private void DisplayBook()
        {
            // Принцип видимости: вся информация о книге на одном экране
            lblTitleValue.Text = _book.Title;
            lblAuthorValue.Text = _book.Author;
            lblISBNValue.Text = _book.ISBN;
            lblYearValue.Text = _book.Year.ToString();
            lblGenreValue.Text = _book.Genre;
            lblPagesValue.Text = _book.PageCount.ToString();
            lblTotalValue.Text = _book.TotalCount.ToString();
            lblAvailableValue.Text = _book.AvailableCount.ToString();

            // Принцип обратной связи: цветовая индикация доступности
            if (_book.IsAvailable)
            {
                lblAvailableValue.ForeColor = System.Drawing.Color.Green;
                btnReserve.Enabled = true;
            }
            else
            {
                lblAvailableValue.ForeColor = System.Drawing.Color.Red;
                btnReserve.Enabled = false;
                btnReserve.Text = "Недоступно";
            }
        }

        private void btnReserve_Click(object sender, EventArgs e)
        {
            // Если гость — предлагаем войти/зарегистрироваться
            if (DataStore.CurrentUser == null || DataStore.CurrentUser.Role == UserRole.Guest)
            {
                var loginForm = new LoginForm();
                this.Hide();
                if (loginForm.ShowDialog(this) != DialogResult.OK)
                {
                    this.Show();
                    return; // Пользователь отменил вход
                }
                this.Show();

                // После успешного входа обновляем отображение
                DisplayBook();
            }

            // Принцип толерантности: подтверждение действия
            var result = MessageBox.Show(
                $"Забронировать книгу «{_book.Title}»?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (DataStore.ReserveBook(_book.BookID))
                {
                    _book = DataStore.GetBookById(_book.BookID);
                    DisplayBook();
                    // Принцип обратной связи: сообщение об успехе
                    MessageBox.Show("Книга успешно забронирована", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось забронировать книгу", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
