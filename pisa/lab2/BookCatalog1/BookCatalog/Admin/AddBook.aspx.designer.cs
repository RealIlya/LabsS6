using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Admin_AddBook
{
    protected Panel pnlAddBook;
    protected Panel pnlForm;
    protected Label lblTitle;
    protected TextBox txtTitle;
    protected Label lblAuthor;
    protected TextBox txtAuthor;
    protected Label lblISBN;
    protected TextBox txtISBN;
    protected Label lblPublisher;
    protected TextBox txtPublisher;
    protected Label lblYear;
    protected TextBox txtYear;
    protected Label lblGenre;
    protected DropDownList ddlGenre;
    protected Label lblPages;
    protected TextBox txtPages;
    protected Label lblCount;
    protected TextBox txtCount;
    protected RequiredFieldValidator rfvTitle;
    protected RequiredFieldValidator rfvAuthor;
    protected RequiredFieldValidator rfvISBN;
    protected RequiredFieldValidator rfvPublisher;
    protected RequiredFieldValidator rfvYear;
    protected RequiredFieldValidator rfvPages;
    protected RequiredFieldValidator rfvCount;
    protected RegularExpressionValidator revISBN;
    protected RangeValidator rvYear;
    protected RangeValidator rvPages;
    protected RangeValidator rvCount;
    protected Button btnAdd;
    protected ValidationSummary vsErrors;
    protected Label lblMessage;
    protected Panel pnlDuplicate;
    protected GridView gvDuplicates;
    protected Label lblDupCount;
    protected Button btnConfirmAdd;
    protected Button btnCancelAdd;
}
