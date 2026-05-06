using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default
{
    protected Panel pnlHeader;
    protected PlaceHolder phAuth;
    protected HyperLink hlLogin;
    protected HyperLink hlRegister;
    protected PlaceHolder phUser;
    protected Label lblUserName;
    protected LinkButton lbLogout;
    protected PlaceHolder phAdmin;
    protected HyperLink hlAddBook;
    protected HyperLink hlWriteOff;
    protected HyperLink hlManageUsers;

    protected Panel pnlSearch;
    protected TextBox txtTitle;
    protected TextBox txtAuthor;
    protected DropDownList ddlGenre;
    protected TextBox txtYearFrom;
    protected RangeValidator rvYearFrom;
    protected TextBox txtYearTo;
    protected RangeValidator rvYearTo;
    protected Button btnSearch;
    protected Button btnReset;

    protected Panel pnlResults;
    protected Label lblResultsCount;
    protected GridView gvBooks;

    protected ValidationSummary vsErrors;
    protected Label lblSystemStatus;
}