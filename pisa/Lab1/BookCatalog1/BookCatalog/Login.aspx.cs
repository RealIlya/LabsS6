using System;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["User"] != null)
            Response.Redirect("Default.aspx");

        // Показываем баннер если пришли с успешной регистрации
        if (!IsPostBack && Session["RegisterSuccess"] != null)
        {
            pnlRegisterSuccess.Visible = true;
            Session.Remove("RegisterSuccess");
        }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        User user = DataStore.AuthenticateUser(
            txtEmail.Text.Trim(),
            txtPassword.Text);

        if (user == null)
        {
            lblMessage.Text = "[!] Неверный email или пароль";
            lblMessage.Visible = true;
            return;
        }

        // Проверка IsEmailConfirmed убрана — подтверждение не требуется
        if (!user.IsActive)
        {
            lblMessage.Text = "[!] Аккаунт заблокирован. Обратитесь к библиотекарю.";
            lblMessage.Visible = true;
            return;
        }

        Session["User"] = user;

        string returnUrl = Request.QueryString["returnUrl"];
        Response.Redirect(!string.IsNullOrEmpty(returnUrl)
            ? returnUrl
            : "Default.aspx");
    }
}


