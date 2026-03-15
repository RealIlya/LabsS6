using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        LoadUserContext();
        DataStore.ExpireOldBookings();
        if (!IsPostBack)
            LoadBooks();
    }

    private void LoadUserContext()
    {
        if (Session["User"] != null)
        {
            User user = (User)Session["User"];
            phAuth.Visible = false;
            phUser.Visible = true;
            lblUserName.Text = user.FullName;
            phAdmin.Visible = user.IsAdmin;
        }
        else
        {
            phAuth.Visible = true;
            phUser.Visible = false;
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        if (Page.IsValid)
            LoadBooks();
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        txtTitle.Text = "";
        txtAuthor.Text = "";
        txtYear.Text = "";
        ddlGenre.SelectedIndex = 0;
        LoadBooks();
    }

    private void LoadBooks()
    {
        string title = txtTitle.Text.Trim();
        string author = txtAuthor.Text.Trim();
        string genre = ddlGenre.SelectedValue;

        int? year = null;
        if (!string.IsNullOrEmpty(txtYear.Text))
            if (int.TryParse(txtYear.Text, out int parsedYear))
                year = parsedYear;

        List<Book> books = DataStore.SearchBooks(title, author, genre, year);

        if (books.Count > 0)
        {
            gvBooks.DataSource = books;
            gvBooks.DataBind();
            lblResultsCount.Text = $"[OK] НАЙДЕНО: {books.Count} ЗАПИСЕЙ";
        }
        else
        {
            // Исправление 3: передаём контекст поиска в EmptyDataTemplate через ViewState,
            // чтобы сообщение знало, были ли введены какие-либо критерии
            bool hasFilters = !string.IsNullOrEmpty(title)
                           || !string.IsNullOrEmpty(author)
                           || !string.IsNullOrEmpty(genre)
                           || year.HasValue;

            ViewState["HasFilters"] = hasFilters;
            gvBooks.DataSource = null;
            gvBooks.DataBind();
            lblResultsCount.Text = "[!] ДАННЫЕ НЕ ОБНАРУЖЕНЫ";
        }
    }

    // Исправление 3: вызывается из EmptyDataTemplate
    public bool GetHasFilters() =>
        ViewState["HasFilters"] != null && (bool)ViewState["HasFilters"];

    // Исправление 2: конвертация кода жанра в русское название
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

    protected void gvBooks_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "ViewDetails")
        {
            Response.Redirect($"BookDetails.aspx?id={e.CommandArgument}");
        }
        else if (e.CommandName == "Book")
        {
            // Исправление 1: посетитель нажал [ЗАБРОНИРОВАТЬ] → редирект на Login.aspx
            // После входа returnUrl вернёт его к странице бронирования
            if (Session["User"] != null)
                Response.Redirect($"Booking.aspx?id={e.CommandArgument}");
            else
                Response.Redirect(
                    $"Login.aspx?returnUrl=Booking.aspx%3Fid%3D{e.CommandArgument}");
        }
    }

    protected void lbLogout_Click(object sender, EventArgs e)
    {
        Session.Abandon();
        Response.Redirect("Default.aspx");
    }
}


