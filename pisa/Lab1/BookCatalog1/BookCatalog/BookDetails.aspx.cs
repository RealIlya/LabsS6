using System;
using System.Web;

public partial class BookDetails : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            LoadBookDetails();
    }

    private void LoadBookDetails()
    {
        // Исправление #3: TryParse вместо Parse
        if (!int.TryParse(Request.QueryString["id"], out int bookId) || bookId <= 0)
        {
            ShowNotFound();
            return;
        }

        Book book = DataStore.GetBookById(bookId);

        if (book == null || book.Status == "WrittenOff")
        {
            ShowNotFound();
            return;
        }

        // Исправление #2: HtmlEncode защищает от XSS
        lblTitle.Text = Server.HtmlEncode(book.Title);
        lblAuthor.Text = Server.HtmlEncode(book.Author);
        lblISBN.Text = Server.HtmlEncode(book.ISBN ?? "—");
        lblPublisher.Text = Server.HtmlEncode(book.Publisher ?? "—");
        lblYear.Text = book.Year.ToString();
        // Исправление #6: читаемые названия жанров
        lblGenre.Text = MapGenre(book.Genre);
        lblPages.Text = book.PageCount > 0 ? book.PageCount.ToString() : "—";

        if (book.AvailableCount > 0)
        {
            lblAvailable.Text = book.AvailableCount + " экз.";
            lblAvailable.CssClass = "status-available";
        }
        else
        {
            lblAvailable.Text = "0 экз.";
            lblAvailable.CssClass = "status-unavailable";
        }

        lblStatus.Text = book.Status == "Available" ? "В НАЛИЧИИ" : "ЗАБРОНИРОВАНО";

        bool isLoggedIn = Session["User"] != null;

        // Исправление #4: показываем нужную кнопку в зависимости от доступности
        if (book.IsAvailable && isLoggedIn)
        {
            lbBook.Visible = true;
            lbQueue.Visible = false;
        }
        else if (!book.IsAvailable && isLoggedIn)
        {
            // Сценарий 4, шаг 5а.2: книга недоступна — предлагаем очередь
            lbBook.Visible = false;
            lbQueue.Visible = true;
        }
        else
        {
            // Гость — кнопок нет, нужно войти
            lbBook.Visible = false;
            lbQueue.Visible = false;
        }

        pnlBookInfo.Visible = true;
        pnlNotFound.Visible = false;
    }

    protected void lbBook_Click(object sender, EventArgs e)
    {
        // Исправление #5: перепроверяем сессию перед действием
        if (Session["User"] == null)
        {
            Response.Redirect($"Login.aspx?returnUrl=BookDetails.aspx%3Fid%3D{Request.QueryString["id"]}");
            return;
        }
        Response.Redirect($"Booking.aspx?id={Request.QueryString["id"]}");
    }

    protected void lbQueue_Click(object sender, EventArgs e)
    {
        // Исправление #5: перепроверяем сессию
        if (Session["User"] == null)
        {
            Response.Redirect($"Login.aspx?returnUrl=BookDetails.aspx%3Fid%3D{Request.QueryString["id"]}");
            return;
        }

        if (!int.TryParse(Request.QueryString["id"], out int bookId)) return;

        User user = (User)Session["User"];
        bool added = DataStore.AddToQueue(user.UserID, bookId);

        lblActionMessage.Visible = true;
        if (added)
        {
            lblActionMessage.Text = "[OK] Вы добавлены в очередь на эту книгу.";
            lblActionMessage.CssClass = "message success";
            lbQueue.Visible = false;
        }
        else
        {
            lblActionMessage.Text = "[!] Вы уже стоите в очереди на эту книгу.";
            lblActionMessage.CssClass = "message error";
        }
    }

    private void ShowNotFound()
    {
        pnlBookInfo.Visible = false;
        pnlNotFound.Visible = true;
    }

    // Исправление #6: маппинг кода жанра → читаемое название
    private string MapGenre(string genre)
    {
        switch (genre)
        {
            case "fiction": return "Художественная";
            case "science": return "Научная";
            case "education": return "Учебная";
            case "children": return "Детская";
            default: return Server.HtmlEncode(genre ?? "—");
        }
    }
}

