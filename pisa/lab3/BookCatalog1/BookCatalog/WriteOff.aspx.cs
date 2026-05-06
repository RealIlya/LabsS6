using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

public partial class WriteOff : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["User"] == null || !((User)Session["User"]).IsAdmin)
        {
            Response.Redirect("~/Default.aspx");
            return;
        }

        if (!IsPostBack)
        {
            LoadBooks();
            LoadArchive();
        }
    }

    // =========================================================
    // ЗАГРУЗКА ДАННЫХ
    // =========================================================

    private void LoadBooks()
    {
        string title = txtFilterTitle.Text.Trim();
        string author = txtFilterAuthor.Text.Trim();
        string genre = ddlFilterGenre.SelectedValue;

        List<Book> books = DataStore.SearchBooks(title, author, genre, null);

        gvBooks.DataSource = books.Count > 0 ? books : null;
        gvBooks.DataBind();

        lblBooksCount.Text = books.Count > 0
            ? $"[OK] НАЙДЕНО: {books.Count} ЗАПИСЕЙ" : "";
    }

    private void LoadArchive()
    {
        List<WriteOffRecord> records = DataStore.GetWriteOffRecords();

        gvArchive.DataSource = records.Count > 0 ? records : null;
        gvArchive.DataBind();

        lblArchiveCount.Text = records.Count > 0
            ? $"[OK] В АРХИВЕ: {records.Count} ЗАПИСЕЙ"
            : "[OK] АРХИВ ПУСТ";
    }

    // =========================================================
    // ФИЛЬТРАЦИЯ
    // =========================================================

    protected void btnFilter_Click(object sender, EventArgs e) => LoadBooks();

    protected void btnFilterReset_Click(object sender, EventArgs e)
    {
        txtFilterTitle.Text = "";
        txtFilterAuthor.Text = "";
        ddlFilterGenre.SelectedIndex = 0;
        LoadBooks();
    }

    // =========================================================
    // ВЫБОР КНИГИ ДЛЯ СПИСАНИЯ (шаг 1)
    // =========================================================

    protected void lbSelectForWriteOff_Click(object sender, EventArgs e)
    {
        var lb = (LinkButton)sender;
        string[] parts = lb.CommandArgument.Split(';');

        if (parts.Length != 2
            || !int.TryParse(parts[0], out int bookId) || bookId <= 0
            || !int.TryParse(parts[1], out int available))
        {
            ShowMessage("[!] Некорректный идентификатор книги", false);
            return;
        }

        // Загружаем данные книги для отображения
        Book book = DataStore.GetBookById(bookId);
        if (book == null)
        {
            ShowMessage("[!] Книга не найдена в каталоге", false);
            return;
        }

        // Заполняем панель подтверждения
        hfSelectedBookId.Value = bookId.ToString();
        hfSelectedAvailable.Value = available.ToString();

        lblSelectedTitle.Text = Server.HtmlEncode(book.Title);
        lblSelectedMeta.Text = $"{Server.HtmlEncode(book.Author)}, {book.Year}, {GenreToRussian(book.Genre)}";
        lblSelectedTotal.Text = book.TotalCount.ToString();
        lblSelectedAvailable.Text = available.ToString();
        lblSelectedBooked.Text = (book.TotalCount - available).ToString();

        // Сбрасываем поля формы
        ddlReason.SelectedIndex = 0;
        txtWriteOffCount.Text = "1";

        // Показываем панель подтверждения, скрываем другие
        pnlWriteOffConfirm.Visible = true;
        pnlActiveBookings.Visible = false;
        lblMessage.Visible = false;

        // Обновляем RangeValidator — максимум = общее количество
        rvWriteOffCount.MaximumValue = book.TotalCount.ToString();
        rvWriteOffCount.ErrorMessage = $"Нельзя списать больше {book.TotalCount} шт. (всего экземпляров)";
    }

    // =========================================================
    // ПОДТВЕРЖДЕНИЕ СПИСАНИЯ (шаг 2)
    // =========================================================

    protected void btnConfirmWriteOff_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        if (!int.TryParse(hfSelectedBookId.Value, out int bookId) || bookId <= 0)
        {
            ShowMessage("[!] Ошибка: книга не выбрана", false);
            return;
        }

        if (!int.TryParse(txtWriteOffCount.Text, out int count) || count < 1)
        {
            ShowMessage("[!] Некорректное количество штук", false);
            return;
        }

        Book bookToCheck = DataStore.GetBookById(bookId);
        if (bookToCheck == null)
        {
            ShowMessage("[!] Книга не найдена в каталоге", false);
            return;
        }

        if (count > bookToCheck.TotalCount)
        {
            ShowMessage(
                $"[!] Нельзя списать {count} шт. — всего экземпляров: {bookToCheck.TotalCount}",
                false);
            return;
        }

        int adminID = ((User)Session["User"]).UserID;
        WriteOffResult result = DataStore.WriteOffBook(
            bookId, ddlReason.SelectedValue, count, out int writtenOff, adminID);

        switch (result)
        {
            case WriteOffResult.Success:
                ShowMessage($"[OK] Списано {writtenOff} шт. Книга перемещена в архив.", true);
                HideConfirmPanel();
                LoadBooks();
                LoadArchive();
                break;

            case WriteOffResult.PartialSuccess:
                ShowMessage(
                    $"[OK] Списано {writtenOff} шт. Оставшиеся экземпляры остаются в каталоге.", true);
                HideConfirmPanel();
                LoadBooks();
                LoadArchive();
                break;

            case WriteOffResult.HasActiveBookings:
                // Сохраняем данные для подтверждения с отменой броней
                hfPendingBookId.Value = bookId.ToString();
                hfPendingCount.Value = count.ToString();
                pnlWriteOffConfirm.Visible = false;
                pnlActiveBookings.Visible = true;
                lblMessage.Visible = false;
                break;

            case WriteOffResult.NotFound:
            default:
                ShowMessage("[!] Книга не найдена в каталоге", false);
                break;
        }
    }

    protected void btnCancelWriteOff_Click(object sender, EventArgs e)
    {
        HideConfirmPanel();
        ShowMessage("[OK] Списание отменено.", true);
    }

    // =========================================================
    // ПОДТВЕРЖДЕНИЕ С ОТМЕНОЙ БРОНЕЙ
    // =========================================================

    protected void btnConfirmWithCancelBookings_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(hfPendingBookId.Value, out int bookId) || bookId <= 0)
        {
            ShowMessage("[!] Ошибка: идентификатор книги не найден", false);
            return;
        }

        if (!int.TryParse(hfPendingCount.Value, out int count) || count < 1)
        {
            ShowMessage("[!] Ошибка: количество не указано", false);
            return;
        }

        string reason = ddlReason.SelectedValue;
        if (string.IsNullOrEmpty(reason))
        {
            ShowMessage("[!] Причина списания не указана", false);
            return;
        }

        DataStore.CancelBookingsForBook(bookId);

        int adminID = ((User)Session["User"]).UserID;
        WriteOffResult result = DataStore.WriteOffBook(
            bookId, reason, count, out int writtenOff, adminID);

        pnlActiveBookings.Visible = false;
        hfPendingBookId.Value = "";
        hfPendingCount.Value = "";

        if (result == WriteOffResult.Success)
        {
            ShowMessage($"[OK] Брони отменены. Списано {writtenOff} шт. Книга в архиве.", true);
            HideConfirmPanel();
            LoadBooks();
            LoadArchive();
        }
        else if (result == WriteOffResult.PartialSuccess)
        {
            ShowMessage(
                $"[OK] Брони отменены. Списано {writtenOff} шт. Оставшиеся экземпляры в каталоге.", true);
            HideConfirmPanel();
            LoadBooks();
            LoadArchive();
        }
        else
        {
            ShowMessage("[!] Ошибка при списании книги", false);
        }
    }

    protected void btnCancelActiveBookings_Click(object sender, EventArgs e)
    {
        pnlActiveBookings.Visible = false;
        hfPendingBookId.Value = "";
        hfPendingCount.Value = "";
        ShowMessage("[OK] Списание отменено. Книга остаётся в каталоге.", true);
    }

    // =========================================================
    // ВОССТАНОВЛЕНИЕ ИЗ АРХИВА
    // =========================================================

    protected void lbRestore_Click(object sender, EventArgs e)
    {
        var lb = (LinkButton)sender;

        if (!int.TryParse(lb.CommandArgument, out int recordId) || recordId <= 0)
        {
            ShowMessage("[!] Некорректный идентификатор записи", false);
            return;
        }

        bool restored = DataStore.RestoreFromRecord(recordId);

        if (restored)
        {
            ShowMessage("[OK] Книга успешно восстановлена в каталог", true);
            LoadBooks();
            LoadArchive();
        }
        else
        {
            ShowMessage("[!] Восстановление невозможно: истекло 24 часа после списания", false);
            LoadArchive();
        }
    }

    // =========================================================
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // =========================================================

    private void HideConfirmPanel()
    {
        pnlWriteOffConfirm.Visible = false;
        hfSelectedBookId.Value = "";
        hfSelectedAvailable.Value = "";
    }

    public static string GenreToRussian(string genre)
    {
        switch (genre)
        {
            case "fiction": return "Художественная";
            case "science": return "Научная";
            case "education": return "Учебная";
            case "children": return "Детская";
            default: return genre;
        }
    }

    private void ShowMessage(string text, bool isSuccess)
    {
        lblMessage.Text = Server.HtmlEncode(text);
        lblMessage.CssClass = isSuccess ? "message success" : "message error";
        lblMessage.Visible = true;
    }
}
