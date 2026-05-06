using System;
using System.Web.Services;

public partial class Register : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["User"] != null)
            Response.Redirect("Default.aspx");
    }

    /// <summary>
    /// AJAX PageMethod: проверка доступности email без перезагрузки страницы.
    /// Вызывается клиентским JavaScript через PageMethods.CheckEmailAvailability().
    /// </summary>
    [WebMethod]
    public static bool CheckEmailAvailability(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        email = email.Trim();

        // Проверяем формат на сервере
        if (!System.Text.RegularExpressions.Regex.IsMatch(email,
            @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            return false;

        // Проверяем существование email в базе данных
        return !DataStore.UserExists(email);
    }

    protected void btnRegister_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string fullName = txtName.Text.Trim();
        string email = txtEmail.Text.Trim();
        string password = txtPassword.Text;

        // Email уже занят → показываем панель с ссылкой на восстановление пароля
        if (DataStore.UserExists(email))
        {
            pnlEmailExists.Visible = true;
            // Передаём email в ForgotPassword.aspx, чтобы поле заполнилось автоматически
            hlForgotPassword.NavigateUrl =
                "~/ForgotPassword.aspx?email=" + Uri.EscapeDataString(email);
            return;
        }

        bool success = DataStore.RegisterUser(fullName, email, password);

        if (!success)
        {
            lblMessage.Text = "[!] Ошибка регистрации. Попробуйте позже.";
            lblMessage.CssClass = "message error";
            lblMessage.Visible = true;
            return;
        }

        // Регистрация прошла успешно — сразу входим, без подтверждения email
        User newUser = DataStore.GetUserByEmail(email);
        Session["User"] = newUser;
        Response.Redirect("Default.aspx");
    }
}
