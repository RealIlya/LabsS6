using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Login
{
    protected Panel pnlLogin;
    protected Panel pnlRegisterSuccess; // новый
    protected Label lblEmail;
    protected TextBox txtEmail;
    protected Label lblPassword;
    protected TextBox txtPassword;
    protected RequiredFieldValidator rfvEmail;
    protected RequiredFieldValidator rfvPassword;
    protected RegularExpressionValidator revEmail;
    protected Button btnLogin;
    protected HyperLink hlRegister;
    protected HyperLink hlForgotPassword; // новый
    protected ValidationSummary vsLogin;
    protected Label lblMessage;
}

