using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

public partial class Admin_ManageUsers : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["User"] == null || !((User)Session["User"]).IsAdmin)
        {
            Response.Redirect("~/Default.aspx");
            return;
        }

        if (!IsPostBack)
            LoadUsers();
    }

    private void LoadUsers()
    {
        User currentAdmin = (User)Session["User"];
        List<User> users = DataStore.GetAllUsers(currentAdmin.UserID);

        gvUsers.DataSource = users;
        gvUsers.DataBind();

        lblUsersCount.Text = $"[OK] ПОЛЬЗОВАТЕЛЕЙ В СИСТЕМЕ: {users.Count}";
    }

    protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (!int.TryParse(e.CommandArgument?.ToString(), out int userId) || userId <= 0)
        {
            ShowMessage("[!] Некорректный идентификатор пользователя", false);
            return;
        }

        switch (e.CommandName)
        {
            case "ViewInfo": HandleViewInfo(userId); break;
            case "ToggleActive": HandleToggleActive(userId); break;
            case "ToggleAdmin": HandleToggleAdmin(userId); break;
            case "ViewBookings": HandleViewBookings(userId); break;
        }

        LoadUsers();
    }

    // --- Просмотр информации ---
    private void HandleViewInfo(int userId)
    {
        User target = DataStore.GetUserById(userId);
        if (target == null) { ShowMessage("[!] Пользователь не найден", false); return; }

        lblInfoName.Text = Server.HtmlEncode(target.FullName);
        lblInfoEmail.Text = Server.HtmlEncode(target.Email);
        lblInfoCreated.Text = target.CreatedAt.ToString("dd.MM.yyyy HH:mm");
        lblInfoRole.Text = target.Role == "Admin" ? "Администратор" : "Читатель";
        lblInfoStatus.Text = target.IsActive ? "Активен" : "Заблокирован";
        lblInfoStatus.CssClass = target.IsActive
            ? "mono status-available" : "mono status-unavailable";
        lblInfoBookings.Text = DataStore.GetUserActiveBookingsCount(userId).ToString();

        pnlUserInfo.Visible = true;
        pnlUserBookings.Visible = false;
        lblMessage.Visible = false;
    }

    // --- Блокировка / разблокировка ---
    private void HandleToggleActive(int userId)
    {
        User currentAdmin = (User)Session["User"];
        if (userId == currentAdmin.UserID)
        {
            ShowMessage("[!] Невозможно деактивировать собственный аккаунт", false);
            return;
        }

        User target = DataStore.GetUserById(userId);
        if (target == null) { ShowMessage("[!] Пользователь не найден", false); return; }

        if (target.IsAdmin)
        {
            ShowMessage("[!] Невозможно деактивировать аккаунт администратора", false);
            return;
        }

        DataStore.ToggleUserActive(userId);

        bool nowActive = DataStore.GetUserById(userId).IsActive;
        string action = nowActive ? "активирован" : "деактивирован";
        ShowMessage($"[OK] Пользователь {Server.HtmlEncode(target.FullName)} {action}", true);

        if (pnlUserInfo.Visible) HandleViewInfo(userId);
    }

    // --- Выдача / снятие прав администратора ---
    private void HandleToggleAdmin(int userId)
    {
        User currentAdmin = (User)Session["User"];
        if (userId == currentAdmin.UserID)
        {
            ShowMessage("[!] Невозможно изменить роль собственного аккаунта", false);
            return;
        }

        User target = DataStore.GetUserById(userId);
        if (target == null) { ShowMessage("[!] Пользователь не найден", false); return; }

        DataStore.ToggleAdminRole(userId);

        // Обновляем сессию если пользователь сам себя смотрит (не актуально, но на всякий случай)
        bool nowAdmin = DataStore.GetUserById(userId).Role == "Admin";
        string action = nowAdmin ? "назначен администратором" : "снят с роли администратора";
        ShowMessage(
            $"[OK] Пользователь {Server.HtmlEncode(target.FullName)} {action}", true);

        if (pnlUserInfo.Visible) HandleViewInfo(userId);
    }

    // --- Просмотр броней ---
    private void HandleViewBookings(int userId)
    {
        User target = DataStore.GetUserById(userId);
        if (target == null) { ShowMessage("[!] Пользователь не найден", false); return; }

        List<BookReservation> bookings = DataStore.GetActiveBookingsByUser(userId);

        gvBookings.DataSource = bookings;
        gvBookings.DataBind();
        lblSelectedUser.Text = Server.HtmlEncode(target.FullName)
                                + "  (" + Server.HtmlEncode(target.Email) + ")";
        pnlUserBookings.Visible = true;
        pnlUserInfo.Visible = false;
        lblMessage.Visible = false;
    }

    protected void btnCloseInfo_Click(object sender, EventArgs e)
    {
        pnlUserInfo.Visible = false;
        LoadUsers();
    }

    protected void btnCloseBookings_Click(object sender, EventArgs e)
    {
        pnlUserBookings.Visible = false;
        gvBookings.DataSource = null;
        gvBookings.DataBind();
        LoadUsers();
    }

    protected string GetBookTitle(int bookId)
    {
        Book book = DataStore.GetBookById(bookId);
        return book != null
            ? Server.HtmlEncode(book.Title)
            : $"[Книга #{bookId} не найдена]";
    }

    private void ShowMessage(string text, bool isSuccess)
    {
        lblMessage.Text = text;
        lblMessage.CssClass = isSuccess ? "message success" : "message error";
        lblMessage.Visible = true;

        pnlUserBookings.Visible = false;
        pnlUserInfo.Visible = false;
    }
}


