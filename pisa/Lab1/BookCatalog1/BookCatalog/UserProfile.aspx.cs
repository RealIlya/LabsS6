using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

public partial class UserProfile : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Только авторизованные пользователи
        if (Session["User"] == null)
        {
            Response.Redirect("~/Login.aspx?returnUrl=UserProfile.aspx");
            return;
        }

        if (!IsPostBack)
            LoadProfile();
    }

    private void LoadProfile()
    {
        User user = (User)Session["User"];

        // Информация о пользователе
        lblFullName.Text = Server.HtmlEncode(user.FullName);
        lblEmail.Text = Server.HtmlEncode(user.Email);
        lblCreatedAt.Text = user.CreatedAt.ToString("dd.MM.yyyy HH:mm");

        // Активные брони
        List<BookReservation> active = DataStore.GetActiveBookingsByUser(user.UserID);
        lblActiveCount.Text = active.Count.ToString() + " / " + user.MaxBookings;
        lblBookingsCount.Text = active.Count > 0
            ? $"[OK] АКТИВНЫХ БРОНЕЙ: {active.Count}"
            : "";

        gvBookings.DataSource = active.Count > 0 ? active : null;
        gvBookings.DataBind();

        // История — все брони кроме активных и очереди
        List<BookReservation> history = DataStore
            .GetUserBookings(user.UserID)
            .Where(b => b.Status != "Active" && b.Status != "Queued")
            .OrderByDescending(b => b.BookingDate)
            .ToList();

        gvHistory.DataSource = history.Count > 0 ? history : null;
        gvHistory.DataBind();
    }

    protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "CancelBooking") return;

        if (!int.TryParse(e.CommandArgument?.ToString(), out int bookingId) || bookingId <= 0)
        {
            ShowMessage("[!] Некорректный идентификатор брони", false);
            return;
        }

        User user = (User)Session["User"];
        bool cancelled = DataStore.CancelBooking(bookingId, user.UserID);

        if (cancelled)
        {
            ShowMessage("[OK] Бронирование успешно отменено", true);
            LoadProfile();
        }
        else
        {
            ShowMessage("[!] Не удалось отменить бронирование", false);
        }
    }




    // =========================================================
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ DATABINDING
    // =========================================================

    protected string GetBookTitle(int bookId)
    {
        Book book = DataStore.GetBookById(bookId);
        return book != null
            ? Server.HtmlEncode(book.Title)
            : $"[Книга #{bookId}]";
    }

    protected string GetBookAuthor(int bookId)
    {
        Book book = DataStore.GetBookById(bookId);
        return book != null
            ? Server.HtmlEncode(book.Author)
            : "—";
    }

    protected string GetDaysLeft(DateTime expiry)
    {
        int days = (int)(expiry - DateTime.Now).TotalDays;
        if (days < 0) return "[ИСТЕКЛА]";
        if (days == 0) return "[СЕГОДНЯ]";
        if (days == 1) return "1 день";
        if (days <= 4) return $"{days} дня";
        return $"{days} дней";
    }

    protected string GetStatusRussian(string status)
    {
        switch (status)
        {
            case "Completed": return "ЗАВЕРШЕНА";
            case "Cancelled": return "ОТМЕНЕНА";
            case "Expired": return "ИСТЕКЛА";
            default: return status;
        }
    }

    protected string GetStatusCssClass(string status)
    {
        switch (status)
        {
            case "Completed": return "mono status-available";
            case "Cancelled": return "mono status-unavailable";
            case "Expired": return "mono status-unavailable";
            default: return "mono";
        }
    }

    private void ShowMessage(string text, bool isSuccess)
    {
        lblMessage.Text = Server.HtmlEncode(text);
        lblMessage.CssClass = isSuccess ? "message success" : "message error";
        lblMessage.Visible = true;
    }
}


