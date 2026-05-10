using System;
using System.Windows.Forms;

namespace BookCatalogWinForms
{
    public partial class AddBookForm : Form
    {
        public AddBookForm()
        {
            InitializeComponent();
            Theme.Apply(this);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Принцип толерантности: проверка всех полей до сохранения
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowFieldError(txtTitle, "Введите название книги");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtAuthor.Text))
            {
                ShowFieldError(txtAuthor, "Введите автора");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtISBN.Text))
            {
                ShowFieldError(txtISBN, "Введите ISBN");
                return;
            }
            if (!IsValidISBN(txtISBN.Text.Trim()))
            {
                ShowFieldError(txtISBN, "ISBN должен быть в формате ISBN-10 или ISBN-13\nПример: 978-5-17-090521-7 или 5-17-090521-4");
                return;
            }
            if (!int.TryParse(txtYear.Text, out int year) || year < 1000 || year > DateTime.Now.Year)
            {
                ShowFieldError(txtYear, $"Год должен быть от 1000 до {DateTime.Now.Year}");
                return;
            }
            if (!int.TryParse(txtPages.Text, out int pages) || pages < 1)
            {
                ShowFieldError(txtPages, "Количество страниц должно быть положительным числом");
                return;
            }
            if (!int.TryParse(txtCount.Text, out int count) || count < 1)
            {
                ShowFieldError(txtCount, "Количество экземпляров должно быть положительным числом");
                return;
            }

            // Принцип обратной связи: проверка уникальности ISBN
            if (DataStore.BookExists(txtISBN.Text.Trim()))
            {
                MessageBox.Show("Книга с таким ISBN уже существует", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtISBN.Focus();
                return;
            }

            var book = new Book
            {
                Title = txtTitle.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                ISBN = txtISBN.Text.Trim(),
                Year = year,
                Genre = cmbGenre.SelectedItem?.ToString() ?? "Художественная",
                PageCount = pages,
                TotalCount = count,
                AvailableCount = count
            };

            DataStore.AddBook(book);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ShowFieldError(Control control, string message)
        {
            MessageBox.Show(message, "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Focus();
        }

        /// <summary>
        /// Проверяет формат ISBN-10 или ISBN-13 (с дефисами или без).
        /// ISBN-10: контрольная сумма по модулю 11, последний символ может быть X.
        /// ISBN-13: контрольная сумма по модулю 10 (EAN-13).
        /// </summary>
        private bool IsValidISBN(string isbn)
        {
            // Убираем дефисы и пробелы
            string digits = isbn.Replace("-", "").Replace(" ", "");

            if (digits.Length == 10)
            {
                // ISBN-10
                int sum = 0;
                for (int i = 0; i < 9; i++)
                {
                    if (!char.IsDigit(digits[i])) return false;
                    sum += (digits[i] - '0') * (10 - i);
                }
                char last = digits[9];
                if (last == 'X' || last == 'x')
                    sum += 10;
                else if (char.IsDigit(last))
                    sum += (last - '0');
                else
                    return false;

                return sum % 11 == 0;
            }

            if (digits.Length == 13)
            {
                // ISBN-13
                int sum = 0;
                for (int i = 0; i < 12; i++)
                {
                    if (!char.IsDigit(digits[i])) return false;
                    sum += (digits[i] - '0') * (i % 2 == 0 ? 1 : 3);
                }
                char last = digits[12];
                if (!char.IsDigit(last)) return false;
                int check = (10 - (sum % 10)) % 10;

                return (last - '0') == check;
            }

            return false;
        }
    }
}
