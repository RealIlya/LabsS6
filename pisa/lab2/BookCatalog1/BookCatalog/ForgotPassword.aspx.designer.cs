using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ForgotPassword
{
    protected Panel pnlMain;
    protected Panel pnlStep1;
    protected TextBox txtEmail;
    protected RequiredFieldValidator rfvEmail;
    protected RegularExpressionValidator revEmail;
    protected Button btnFind;
    protected Label lblStep1Error;
    protected Panel pnlStep2;
    protected Label lblFoundUser;
    protected TextBox txtNewPassword;
    protected TextBox txtConfirmPassword;
    protected Button btnSetPassword;
}


