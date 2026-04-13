using System.Web.UI;
using System.Web.UI.WebControls;

public partial class WriteOff
{
    protected Panel pnlWriteOff;
    protected HyperLink hlBack;
    protected Panel pnlFilter;
    protected TextBox txtFilterTitle;
    protected TextBox txtFilterAuthor;
    protected DropDownList ddlFilterGenre;
    protected Button btnFilter;
    protected Button btnFilterReset;
    protected Label lblReason;
    protected DropDownList ddlReason;
    protected RequiredFieldValidator rfvReason;
    protected TextBox txtWriteOffCount;  // новый
    protected Label lblAvailableHint;  // новый
    protected Label lblBooksCount;
    protected GridView gvBooks;
    protected Panel pnlActiveBookings;
    protected HiddenField hfPendingBookId;
    protected HiddenField hfPendingCount;    // новый
    protected Button btnConfirmWriteOff;
    protected Button btnCancelWriteOff;
    protected ValidationSummary vsWriteOff;
    protected Label lblMessage;
    protected Panel pnlArchive;
    protected Label lblArchiveCount;
    protected GridView gvArchive;
}


