using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BookCatalogWinForms
{
    public partial class MainForm : Form
    {
        // Сохранённые параметры последнего поиска
        private string _lastTitle = "";
        private string _lastAuthor = "";
        private string _lastGenre = "";
        private int? _lastYearFrom;
        private int? _lastYearTo;
        private bool _hasActiveSearch;

        public MainForm()
        {
            InitializeComponent();
            Theme.Apply(this);
            ApplyRolePermissions();
            LoadBooks();
        }

        /// <summary>
        /// Принцип видимости: показываем только доступные действия
        /// </summary>
        private void ApplyRolePermissions()
        {
            var user = DataStore.CurrentUser;
            if (user == null) return;

            bool isGuest = user.Role == UserRole.Guest;

            // Обновляем заголовок с именем пользователя
            this.Text = $"Книжный каталог — {user.Name} [{user.Role}]";

            // Принцип видимости: скрываем недоступные кнопки
            btnReserve.Visible = user.CanReserve;
            btnAddBook.Visible = user.CanAddBook;
            btnWriteOff.Visible = user.CanDeleteBook;

            // Кнопки авторизации: видны только гостю
            btnLogin.Visible = isGuest;
            btnRegister.Visible = isGuest;

            // Кнопка выхода: видна всем авторизованным (не гостям)
            btnLogout.Visible = !isGuest;

            // Мои бронирования: видна всем, кто может бронировать (User/Admin)
            btnMyBookings.Visible = user.CanReserve;

            // Статус-бар показывает роль
            lblStatus.Text = $"Пользователь: {user.Name} | Роль: {user.Role}";
        }

        private void LoadBooks()
        {
            dgvBooks.DataSource = null;
            dgvBooks.DataSource = DataStore.GetAllBooks();
            UpdateStatusBar();
        }

        /// <summary>
        /// Обновляет таблицу: если есть активный поиск — повторяет его, иначе грузит все книги
        /// </summary>
        private void RefreshBooks()
        {
            if (_hasActiveSearch)
                ApplySearch();
            else
                LoadBooks();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string author = txtAuthor.Text.Trim();
            string genre = cmbGenre.SelectedItem?.ToString() ?? "";
            int? yearFrom = null, yearTo = null;

            if (int.TryParse(txtYearFrom.Text, out int yf)) yearFrom = yf;
            if (int.TryParse(txtYearTo.Text, out int yt)) yearTo = yt;

            if (txtYearFrom.Text.Length > 0 && !yearFrom.HasValue)
            {
                ShowError("Год «от» должен быть числом");
                return;
            }
            if (txtYearTo.Text.Length > 0 && !yearTo.HasValue)
            {
                ShowError("Год «до» должен быть числом");
                return;
            }

            // Сохраняем параметры поиска
            _lastTitle = title;
            _lastAuthor = author;
            _lastGenre = genre;
            _lastYearFrom = yearFrom;
            _lastYearTo = yearTo;
            _hasActiveSearch = true;

            ApplySearch();
        }

        /// <summary>
        /// Выполняет поиск по сохранённым параметрам
        /// </summary>
        private void ApplySearch()
        {
            var results = DataStore.SearchBooks(_lastTitle, _lastAuthor, _lastGenre, _lastYearFrom, _lastYearTo);
            dgvBooks.DataSource = null;
            dgvBooks.DataSource = results;

            if (results.Count == 0)
                lblStatus.Text = "Ничего не найдено";
            else
                lblStatus.Text = $"Найдено: {results.Count} книг";
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtTitle.Text = "";
            txtAuthor.Text = "";
            txtYearFrom.Text = "";
            txtYearTo.Text = "";
            cmbGenre.SelectedIndex = -1;
            _hasActiveSearch = false;
            LoadBooks();
        }

        private void btnAddBook_Click(object sender, EventArgs e)
        {
            var form = new AddBookForm();
            this.Hide();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                this.Show();
                RefreshBooks();
                ShowInfo("Книга успешно добавлена");
            }
            else
            {
                this.Show();
            }
        }

        private void btnWriteOff_Click(object sender, EventArgs e)
        {
            var form = new WriteOffForm();
            this.Hide();
            form.ShowDialog(this);
            this.Show();
            ApplyRolePermissions();
            RefreshBooks();
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (dgvBooks.CurrentRow == null)
            {
                ShowError("Выберите книгу");
                return;
            }

            var book = (Book)dgvBooks.CurrentRow.DataBoundItem;
            var form = new BookDetailsForm(book);
            this.Hide();
            form.ShowDialog(this);
            this.Show();
            // После закрытия деталей — обновляем всё (пользователь мог смениться через логин внутри деталей)
            ApplyRolePermissions();
            RefreshBooks();
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
                    return;
                }
                this.Show();

                // После входа обновляем интерфейс
                ApplyRolePermissions();
                RefreshBooks();
            }

            if (dgvBooks.CurrentRow == null)
            {
                ShowError("Выберите книгу");
                return;
            }

            var book = (Book)dgvBooks.CurrentRow.DataBoundItem;

            if (!book.IsAvailable)
            {
                ShowError("Книга недоступна для бронирования");
                return;
            }

            var result = MessageBox.Show(
                $"Забронировать книгу «{book.Title}»?",
                "Подтверждение бронирования",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (DataStore.ReserveBook(book.BookID))
                {
                    ShowInfo($"Книга «{book.Title}» забронирована");
                    RefreshBooks();
                }
                else
                {
                    ShowError("Не удалось забронировать книгу");
                }
            }
        }

        private void dgvBooks_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvBooks.CurrentRow != null;
            btnDetails.Enabled = hasSelection;
            btnReserve.Enabled = hasSelection && ((Book)dgvBooks.CurrentRow.DataBoundItem).IsAvailable;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var loginForm = new LoginForm();
            this.Hide();
            if (loginForm.ShowDialog(this) == DialogResult.OK)
            {
                this.Show();
                ApplyRolePermissions();
                RefreshBooks();
            }
            else
            {
                this.Show();
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm();
            this.Hide();
            if (registerForm.ShowDialog(this) == DialogResult.OK)
            {
                this.Show();
                ApplyRolePermissions();
                RefreshBooks();
            }
            else
            {
                this.Show();
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                $"Выйти из аккаунта «{DataStore.CurrentUser?.Name}»?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DataStore.Logout();
                this.Close();
            }
        }

        private void btnMyBookings_Click(object sender, EventArgs e)
        {
            var form = new MyBookingsForm();
            this.Hide();
            form.ShowDialog(this);
            this.Show();
            ApplyRolePermissions();
            RefreshBooks();
        }

        private void UpdateStatusBar()
        {
            var books = DataStore.GetAllBooks();
            int available = 0;
            foreach (var b in books)
                if (b.IsAvailable) available++;
            lblStatus.Text = $"Пользователь: {DataStore.CurrentUser?.Name} | Всего: {books.Count} | Доступно: {available}";
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowInfo(string msg)
        {
            MessageBox.Show(msg, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
