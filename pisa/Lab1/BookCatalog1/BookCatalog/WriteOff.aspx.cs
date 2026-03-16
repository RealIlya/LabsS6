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
        lblAvailableHint.Text = "—";
    }

    private void LoadArchive()
    {
        // [FIX 1] Источник архива — записи WriteOffRecord, а не книги со
        // статусом WrittenOff. Только так в архив попадают частичные списания.
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
    // СПИСАНИЕ
    // =========================================================

    protected void lbWriteOff_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        var lb = (LinkButton)sender;

        // CommandArgument: "bookId;availableCount"
        string[] parts = lb.CommandArgument.Split(';');
        if (parts.Length != 2
            || !int.TryParse(parts[0], out int bookId) || bookId <= 0
            || !int.TryParse(parts[1], out int available))
        {
            ShowMessage("[!] Некорректный идентификатор книги", false);
            return;
        }

        if (!int.TryParse(txtWriteOffCount.Text, out int count) || count < 1)
        {
            ShowMessage("[!] Некорректное количество штук", false);
            return;
        }

        // [FIX 2] Было: count > available (т.е. > AvailableCount).
        // Это блокировало любой ввод, превышающий число свободных экземпляров,
        // и путь WriteOffResult.HasActiveBookings становился недостижим.
        // Теперь верхняя граница — TotalCount книги: нельзя списать больше,
        // чем физически существует экземпляров (свободных + забронированных).
        // Если count > AvailableCount, DataStore сам вернёт HasActiveBookings
        // и покажет панель подтверждения отмены броней.
        var bookToCheck = DataStore.GetBookById(bookId);
        if (bookToCheck == null)
        {
            ShowMessage("[!] Книга не найдена в каталоге", false);
            return;
        }

        if (count > bookToCheck.TotalCount)
        {
            lblAvailableHint.Text = available.ToString();
            ShowMessage(
                $"[!] Нельзя списать {count} шт. — всего экземпляров: {bookToCheck.TotalCount}",
                false);
            return;
        }

        lblAvailableHint.Text = available.ToString();

        WriteOffResult result = DataStore.WriteOffBook(
            bookId, ddlReason.SelectedValue, count, out int writtenOff);

        switch (result)
        {
            case WriteOffResult.Success:
                ShowMessage(
                    $"[OK] Списано {writtenOff} шт. Книга перемещена в архив.", true);
                pnlActiveBookings.Visible = false;
                LoadBooks();
                LoadArchive();
                break;

            case WriteOffResult.PartialSuccess:
                ShowMessage(
                    $"[OK] Списано {writtenOff} шт. " +
                    "Оставшиеся экземпляры остаются в каталоге.", true);
                pnlActiveBookings.Visible = false;
                LoadBooks();
                LoadArchive();
                break;

            case WriteOffResult.HasActiveBookings:
                // Свободных не хватает — нужно отменить брони
                hfPendingBookId.Value = bookId.ToString();
                hfPendingCount.Value = count.ToString();
                pnlActiveBookings.Visible = true;
                lblMessage.Visible = false;
                break;

            case WriteOffResult.NotFound:
            default:
                ShowMessage("[!] Книга не найдена в каталоге", false);
                break;
        }
    }

    protected void btnConfirmWriteOff_Click(object sender, EventArgs e)
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

        // Отменяем брони — теперь свободных станет достаточно
        DataStore.CancelBookingsForBook(bookId);

        WriteOffResult result = DataStore.WriteOffBook(
            bookId, reason, count, out int writtenOff);

        pnlActiveBookings.Visible = false;
        hfPendingBookId.Value = "";
        hfPendingCount.Value = "";

        if (result == WriteOffResult.Success)
        {
            ShowMessage(
                $"[OK] Брони отменены. Списано {writtenOff} шт. Книга в архиве.", true);
            LoadBooks();
            LoadArchive();
        }
        else if (result == WriteOffResult.PartialSuccess)
        {
            ShowMessage(
                $"[OK] Брони отменены. Списано {writtenOff} шт. " +
                "Оставшиеся экземпляры в каталоге.", true);
            LoadBooks();
            LoadArchive();
        }
        else
        {
            ShowMessage("[!] Ошибка при списании книги", false);
        }
    }

    protected void btnCancelWriteOff_Click(object sender, EventArgs e)
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

        // [FIX 1] CommandArgument теперь должен передавать RecordID (а не BookID).
        // Обновите привязку в WriteOff.aspx: CommandArgument='<%# Eval("RecordID") %>'
        if (!int.TryParse(lb.CommandArgument, out int recordId) || recordId <= 0)
        {
            ShowMessage("[!] Некорректный идентификатор записи", false);
            return;
        }

        // [FIX 1] Было: DataStore.RestoreBook(bookId) — восстанавливало только
        // полностью списанные книги и не умело работать с частичными записями.
        // Теперь: RestoreFromRecord(recordId) восстанавливает конкретную партию.
        bool restored = DataStore.RestoreFromRecord(recordId);

        if (restored)
        {
            ShowMessage("[OK] Книга успешно восстановлена в каталог", true);
            LoadBooks();
            LoadArchive();
        }
        else
        {
            ShowMessage(
                "[!] Восстановление невозможно: истекло 24 часа после списания",
                false);
            LoadArchive();
        }
    }

    // =========================================================
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // =========================================================

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
