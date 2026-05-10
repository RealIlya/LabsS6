using System.Web.UI;
using System.Web.UI.WebControls;

public partial class WriteOff
{
    protected ScriptManager smMain;
    protected UpdatePanel upWriteOff;
    protected UpdateProgress updProgress;

    protected Panel pnlWriteOff;
    protected HyperLink hlBack;
    protected Panel pnlFilter;
    protected TextBox txtFilterTitle;
    protected TextBox txtFilterAuthor;
    protected DropDownList ddlFilterGenre;
    protected Button btnFilter;
    protected Button btnFilterReset;
    protected Label lblBooksCount;

    // Панель подтверждения списания
    protected Panel pnlWriteOffConfirm;
    protected HiddenField hfSelectedBookId;
    protected HiddenField hfSelectedAvailable;
    protected Label lblSelectedTitle;
    protected Label lblSelectedMeta;
    protected Label lblSelectedTotal;
    protected Label lblSelectedAvailable;
    protected Label lblSelectedBooked;
    protected DropDownList ddlReason;
    protected RequiredFieldValidator rfvReason;
    protected TextBox txtWriteOffCount;
    protected RangeValidator rvWriteOffCount;
    protected Button btnConfirmWriteOff;
    protected Button btnCancelWriteOff;

    // Список книг
    protected GridView gvBooks;

    // Панель подтверждения с отменой броней
    protected Panel pnlActiveBookings;
    protected HiddenField hfPendingBookId;
    protected HiddenField hfPendingCount;
    protected Button btnConfirmWithCancelBookings;
    protected Button btnCancelActiveBookings;

    protected ValidationSummary vsWriteOff;
    protected Label lblMessage;

    // Архив
    protected Panel pnlArchive;
    protected Label lblArchiveCount;
    protected GridView gvArchive;
}
