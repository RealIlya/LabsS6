using System;
using System.Windows.Forms;

namespace BookCatalogWinForms
{
    public partial class WriteOffForm : Form
    {
        private Book _selectedBook;

        public WriteOffForm()
        {
            InitializeComponent();
            Theme.Apply(this);
            LoadBooks();
            LoadArchive();
        }

        // =========================================================
        // ЗАГРУЗКА ДАННЫХ
        // =========================================================

        private void LoadBooks()
        {
            dgvBooks.DataSource = null;
            dgvBooks.DataSource = DataStore.GetAllBooks();
        }

        private void LoadArchive()
        {
            dgvArchive.DataSource = null;
            var records = DataStore.GetWriteOffRecords();
            dgvArchive.DataSource = records;

            if (records.Count == 0)
                lblArchiveInfo.Text = "Архив пуст";
            else
                lblArchiveInfo.Text = $"В архиве: {records.Count} записей";
        }

        // =========================================================
        // ВЫБОР КНИГИ ДЛЯ СПИСАНИЯ
        // =========================================================

        private void dgvBooks_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBooks.CurrentRow == null)
            {
                btnWriteOff.Enabled = false;
                return;
            }

            _selectedBook = (Book)dgvBooks.CurrentRow.DataBoundItem;
            btnWriteOff.Enabled = true;

            // Показываем информацию о книге
            lblBookInfo.Text = $"{_selectedBook.Title} — {_selectedBook.Author}, {_selectedBook.Year}"
                + $"\nВсего: {_selectedBook.TotalCount} | Доступно: {_selectedBook.AvailableCount}"
                + $" | Забронировано: {_selectedBook.TotalCount - _selectedBook.AvailableCount}";
        }

        private void btnWriteOff_Click(object sender, EventArgs e)
        {
            if (_selectedBook == null)
            {
                MessageBox.Show("Выберите книгу для списания", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем причину
            if (cmbReason.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите причину списания", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbReason.Focus();
                return;
            }

            // Проверяем количество
            if (!int.TryParse(txtCount.Text, out int count) || count < 1)
            {
                MessageBox.Show("Количество должно быть положительным числом", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCount.Focus();
                return;
            }

            if (count > _selectedBook.TotalCount)
            {
                MessageBox.Show($"Нельзя списать больше {_selectedBook.TotalCount} шт. (всего экземпляров)",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCount.Focus();
                return;
            }

            // Подтверждение
            string reason = cmbReason.SelectedItem.ToString();
            var result = MessageBox.Show(
                $"Списать {count} шт. книги «{_selectedBook.Title}»?\nПричина: {reason}",
                "Подтверждение списания",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            // Списание
            WriteOffResult writeOffResult = DataStore.WriteOffBook(
                _selectedBook.BookID, reason, count, out int writtenOff);

            switch (writeOffResult)
            {
                case WriteOffResult.Success:
                    MessageBox.Show($"Списано {writtenOff} шт. Книга перемещена в архив.", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case WriteOffResult.PartialSuccess:
                    MessageBox.Show($"Списано {writtenOff} шт. Оставшиеся экземпляры в каталоге.", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case WriteOffResult.HasActiveBookings:
                    // Есть активные брони — спрашиваем, отменить ли
                    var confirmCancel = MessageBox.Show(
                        "На эту книгу есть активные брони. Отменить брони и списать книгу?",
                        "Активные брони",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmCancel == DialogResult.Yes)
                    {
                        DataStore.CancelBookingsForBook(_selectedBook.BookID);
                        WriteOffResult retry = DataStore.WriteOffBook(
                            _selectedBook.BookID, reason, count, out int writtenOff2);

                        MessageBox.Show(
                            retry != WriteOffResult.NotFound
                                ? $"Брони отменены. Списано {writtenOff2} шт."
                                : "Ошибка при списании книги",
                            retry != WriteOffResult.NotFound ? "Успех" : "Ошибка",
                            MessageBoxButtons.OK,
                            retry != WriteOffResult.NotFound ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    }
                    else
                    {
                        return;
                    }
                    break;

                default:
                    MessageBox.Show("Книга не найдена в каталоге", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Сброс формы и обновление
            cmbReason.SelectedIndex = -1;
            txtCount.Text = "1";
            _selectedBook = null;
            LoadBooks();
            LoadArchive();
        }

        // =========================================================
        // ВОССТАНОВЛЕНИЕ ИЗ АРХИВА
        // =========================================================

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (dgvArchive.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для восстановления", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var record = (WriteOffRecord)dgvArchive.CurrentRow.DataBoundItem;

            if (!record.CanBeRestored)
            {
                MessageBox.Show("Восстановление невозможно: прошло более 24 часов после списания",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Восстановить книгу «{record.BookTitle}» ({record.Count} шт.) в каталог?",
                "Подтверждение восстановления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            if (DataStore.RestoreFromRecord(record.RecordID))
            {
                MessageBox.Show("Книга успешно восстановлена в каталог", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBooks();
                LoadArchive();
            }
            else
            {
                MessageBox.Show("Не удалось восстановить книгу", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // НАВИГАЦИЯ
        // =========================================================

        private void btnBack_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void dgvBooks_CellFormatting(object sender, System.Windows.Forms.DataGridViewCellFormattingEventArgs e)
        {
            if (dgvBooks.Columns[e.ColumnIndex].Name == "colBooked" && e.RowIndex >= 0)
            {
                var book = dgvBooks.Rows[e.RowIndex].DataBoundItem as Book;
                if (book != null)
                    e.Value = book.TotalCount - book.AvailableCount;
            }
        }
    }
}
