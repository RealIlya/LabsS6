using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Register
{
    protected Panel pnlRegister;
    protected Label lblName;
    protected TextBox txtName;
    protected Label lblEmail;
    protected TextBox txtEmail;
    protected Label lblPassword;
    protected TextBox txtPassword;
    protected Label lblConfirm;
    protected TextBox txtConfirm;
    protected RequiredFieldValidator rfvName;
    protected RequiredFieldValidator rfvEmail;
    protected RequiredFieldValidator rfvPassword;
    protected RequiredFieldValidator rfvConfirm;
    protected RegularExpressionValidator revEmail;
    protected RegularExpressionValidator revPassword;
    protected CompareValidator cvPassword;
    protected Button btnRegister;
    protected HyperLink hlLogin;
    protected ValidationSummary vsErrors;
    protected Label lblMessage;
    protected Panel pnlEmailExists;   // новый
    protected HyperLink hlForgotPassword; // новый
}


