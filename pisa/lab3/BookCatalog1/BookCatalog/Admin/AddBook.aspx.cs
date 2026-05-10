using System;
using System.Collections.Generic;
using System.Web.Services;

public partial class Admin_AddBook : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["User"] == null || !((User)Session["User"]).IsAdmin)
        {
            Response.Redirect("~/Default.aspx");
            return;
        }
    }

    /// <summary>
    /// AJAX PageMethod: проверка существования ISBN в каталоге.
    /// Возвращает объект с полями exists (bool) и count (int).
    /// Вызывается клиентским JavaScript через PageMethods.CheckISBNExists().
    /// </summary>
    [WebMethod]
    public static object CheckISBNExists(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return new { exists = false, count = 0 };

        isbn = isbn.Trim();

        List<Book> duplicates = DataStore.FindDuplicatesByISBN(isbn);

        return new
        {
            exists = duplicates.Count > 0,
            count = duplicates.Count
        };
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        if (!int.TryParse(txtYear.Text, out int year))
        {
            ShowMessage("[!] Некорректный год издания", false);
            return;
        }

        if (!int.TryParse(txtPages.Text, out int pageCount) || pageCount < 1)
        {
            ShowMessage("[!] Некорректное количество страниц", false);
            return;
        }

        if (!int.TryParse(txtCount.Text, out int count))
        {
            ShowMessage("[!] Некорректное количество экземпляров", false);
            return;
        }

        string isbn = txtISBN.Text.Trim();

        // Проверяем дубликаты по ISBN до добавления
        List<Book> duplicates = DataStore.FindDuplicatesByISBN(isbn);

        if (duplicates.Count > 0)
        {
            // Сохраняем введённые данные в ViewState для шага подтверждения
            ViewState["PendingTitle"] = txtTitle.Text.Trim();
            ViewState["PendingAuthor"] = txtAuthor.Text.Trim();
            ViewState["PendingISBN"] = isbn;
            ViewState["PendingPublisher"] = txtPublisher.Text.Trim();
            ViewState["PendingYear"] = year;
            ViewState["PendingGenre"] = ddlGenre.SelectedValue;
            ViewState["PendingPages"] = pageCount;
            ViewState["PendingCount"] = count;

            // Показываем панель с дубликатами
            gvDuplicates.DataSource = duplicates;
            gvDuplicates.DataBind();
            lblDupCount.Text = count.ToString();

            pnlForm.Visible = false;
            pnlDuplicate.Visible = true;
            return;
        }

        // Дубликатов нет — добавляем сразу
        AddBookFromFields(
            txtTitle.Text.Trim(), txtAuthor.Text.Trim(), isbn,
            txtPublisher.Text.Trim(), year,
            ddlGenre.SelectedValue, pageCount, count);
    }

    // Пользователь подтвердил добавление экземпляров к дубликату
    protected void btnConfirmAdd_Click(object sender, EventArgs e)
    {
        if (ViewState["PendingISBN"] == null)
        {
            Response.Redirect("AddBook.aspx");
            return;
        }

        AddBookFromFields(
            (string)ViewState["PendingTitle"],
            (string)ViewState["PendingAuthor"],
            (string)ViewState["PendingISBN"],
            (string)ViewState["PendingPublisher"],
            (int)ViewState["PendingYear"],
            (string)ViewState["PendingGenre"],
            (int)ViewState["PendingPages"],
            (int)ViewState["PendingCount"]);
    }

    // Пользователь отменил добавление — возвращаем форму
    protected void btnCancelAdd_Click(object sender, EventArgs e)
    {
        ClearViewState();
        pnlDuplicate.Visible = false;
        pnlForm.Visible = true;
        ShowMessage("[X] Добавление отменено", false);
    }

    private void AddBookFromFields(string title, string author, string isbn,
        string publisher, int year, string genre, int pageCount, int count)
    {
        var book = new Book
        {
            Title = title,
            Author = author,
            ISBN = isbn,
            Publisher = publisher,
            Year = year,
            Genre = genre,
            PageCount = pageCount,
            AvailableCount = count
        };

        DataStore.AddBook(book);
        ClearViewState();

        // Сбрасываем форму
        txtTitle.Text = "";
        txtAuthor.Text = "";
        txtISBN.Text = "";
        txtPublisher.Text = "";
        txtYear.Text = "";
        txtPages.Text = "";
        txtCount.Text = "1";
        ddlGenre.SelectedIndex = 0;

        pnlDuplicate.Visible = false;
        pnlForm.Visible = true;
        ShowMessage("[OK] Книга успешно добавлена в каталог", true);
    }

    private void ClearViewState()
    {
        ViewState.Remove("PendingTitle");
        ViewState.Remove("PendingAuthor");
        ViewState.Remove("PendingISBN");
        ViewState.Remove("PendingPublisher");
        ViewState.Remove("PendingYear");
        ViewState.Remove("PendingGenre");
        ViewState.Remove("PendingPages");
        ViewState.Remove("PendingCount");
    }

    private void ShowMessage(string text, bool isSuccess)
    {
        lblMessage.Text = Server.HtmlEncode(text);
        lblMessage.CssClass = isSuccess ? "message success" : "message error";
        lblMessage.Visible = true;
    }
}
