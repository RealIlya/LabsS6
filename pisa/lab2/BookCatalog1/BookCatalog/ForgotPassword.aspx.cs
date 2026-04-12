using System;

public partial class ForgotPassword : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["User"] != null)
            Response.Redirect("Default.aspx");

        // Автозаполнение email если пришли с Register.aspx
        if (!IsPostBack)
        {
            string emailParam = Request.QueryString["email"];
            if (!string.IsNullOrEmpty(emailParam))
                txtEmail.Text = emailParam;
        }
    }

    protected void btnFind_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string email = txtEmail.Text.Trim();
        User user = DataStore.GetUserByEmail(email);

        if (user == null)
        {
            lblStep1Error.Text = "[!] Аккаунт с таким email не найден.";
            lblStep1Error.Visible = true;
            return;
        }

        // Сохраняем ID пользователя в сессии для шага 2
        Session["RestoreUserId"] = user.UserID;

        lblFoundUser.Text = $"[OK] Аккаунт найден: {Server.HtmlEncode(user.FullName)}. Введите новый пароль.";
        pnlStep1.Visible = false;
        pnlStep2.Visible = true;
    }

    protected void btnSetPassword_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        if (Session["RestoreUserId"] == null)
        {
            Response.Redirect("ForgotPassword.aspx");
            return;
        }

        int userId = (int)Session["RestoreUserId"];
        DataStore.ResetPassword(userId, txtNewPassword.Text);
        Session.Remove("RestoreUserId");

        Response.Redirect("Login.aspx?msg=password_changed");
    }
}


