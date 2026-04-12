using System;
using System.Data.SqlClient;
using System.Web;

public partial class Booking : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            LoadBookingInfo();
    }

    private void LoadBookingInfo()
    {
        // Исправление #6: проверка сессии
        if (Session["User"] == null)
        {
            Response.Redirect(
                $"Login.aspx?returnUrl=Booking.aspx%3Fid%3D{Request.QueryString["id"]}");
            return;
        }

        // Исправление #2: TryParse вместо Parse
        if (!int.TryParse(Request.QueryString["id"], out int bookId) || bookId <= 0)
        {
            ShowError("[!] Некорректный идентификатор книги");
            return;
        }

        Book book = DataStore.GetBookById(bookId);
        User user = (User)Session["User"];

        if (book == null || book.Status == "WrittenOff")
        {
            ShowError("[!] Книга не найдена или списана");
            return;
        }

        if (!book.IsAvailable)
        {
            ShowError("[!] Книга недоступна для бронирования. " +
                      "Вернитесь на страницу книги, чтобы встать в очередь.");
            return;
        }

        // Исправление #5: используем user.MaxBookings вместо хардкода 3
        if (DataStore.GetUserActiveBookingsCount(user.UserID) >= user.MaxBookings)
        {
            ShowError($"[!] Превышен лимит бронирований (макс. {user.MaxBookings})");
            return;
        }

        // Исправление XSS: HtmlEncode для данных книги
        lblBookTitle.Text = Server.HtmlEncode(book.Title);
        lblBookAuthor.Text = Server.HtmlEncode(book.Author);
        lblAvailable.Text = book.AvailableCount + " экз.";

        pnlConfirm.Visible = true;
        pnlSuccess.Visible = false;
        pnlError.Visible = false;
    }

    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        // Исправление #6: сессия могла истечь между загрузкой и нажатием
        if (Session["User"] == null)
        {
            Response.Redirect(
                $"Login.aspx?returnUrl=Booking.aspx%3Fid%3D{Request.QueryString["id"]}");
            return;
        }

        // Исправление #2: TryParse
        if (!int.TryParse(Request.QueryString["id"], out int bookId) || bookId <= 0)
        {
            ShowError("[!] Некорректный идентификатор книги");
            return;
        }

        User user = (User)Session["User"];

        // Исправления #3 и #4: правильная сигнатура и BookingResult вместо bool
        BookingResult result = DataStore.CreateBooking(user.UserID, bookId);

        switch (result)
        {
            case BookingResult.Success:
                pnlConfirm.Visible = false;
                pnlSuccess.Visible = true;
                // Дата берётся из логики DataStore (3 дня), отображаем для пользователя
                lblExpiryDate.Text = DateTime.Now.AddDays(3).ToString("dd.MM.yyyy");
                break;

            case BookingResult.NotAvailable:
                // Сценарий 4, шаг 5а: книга уже забрали пока пользователь думал
                ShowError("[!] Книга только что была забронирована другим пользователем.");
                break;

            case BookingResult.LimitExceeded:
                // Сценарий 4, шаг 6а
                ShowError($"[!] Превышен лимит бронирований (макс. {user.MaxBookings}).");
                break;

            case BookingResult.NotFound:
            default:
                ShowError("[!] Не удалось оформить бронь. Попробуйте позже.");
                break;
        }
    }

    private void ShowError(string message)
    {
        pnlConfirm.Visible = false;
        pnlSuccess.Visible = false;
        pnlError.Visible = true;
        lblError.Text = Server.HtmlEncode(message);
    }
}

