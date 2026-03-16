<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Книжный каталог</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">

    <asp:Panel ID="pnlHeader" runat="server" CssClass="header">
        <h1>// КНИЖНЫЙ КАТАЛОГ</h1>
        <asp:PlaceHolder ID="phAuth" runat="server">
            <asp:HyperLink ID="hlLogin" runat="server"
                NavigateUrl="~/Login.aspx" CssClass="link">Войти</asp:HyperLink> |
            <asp:HyperLink ID="hlRegister" runat="server"
                NavigateUrl="~/Register.aspx" CssClass="link">Регистрация</asp:HyperLink>
        </asp:PlaceHolder>
        <asp:PlaceHolder ID="phUser" runat="server" Visible="false">
    Привет, <asp:Label ID="lblUserName" runat="server" CssClass="username" /> |
    <asp:HyperLink runat="server" NavigateUrl="~/UserProfile.aspx"
        CssClass="link">[МОЙ ПРОФИЛЬ]</asp:HyperLink> |
    <asp:LinkButton ID="lbLogout" runat="server"
        OnClick="lbLogout_Click" CssClass="link"
        CausesValidation="false">Выйти</asp:LinkButton>
    <asp:PlaceHolder ID="phAdmin" runat="server" Visible="false">
        | <asp:HyperLink ID="hlAddBook" runat="server"
            NavigateUrl="~/Admin/AddBook.aspx"
            CssClass="link admin">[ДОБАВИТЬ КНИГУ]</asp:HyperLink>
        | <asp:HyperLink ID="hlWriteOff" runat="server"
            NavigateUrl="~/WriteOff.aspx"
            CssClass="link admin">[СПИСАНИЕ]</asp:HyperLink>
        | <asp:HyperLink ID="hlManageUsers" runat="server"
            NavigateUrl="~/Admin/ManageUsers.aspx"
            CssClass="link admin">[ПОЛЬЗОВАТЕЛИ]</asp:HyperLink>
    </asp:PlaceHolder>
</asp:PlaceHolder>






        </asp:PlaceHolder>
    </asp:Panel>

    <asp:Panel ID="pnlSearch" runat="server" CssClass="search-panel">
        <h2>>> ПОИСК ДАННЫХ</h2>
        <table class="search-form">
            <tr>
                <td><asp:Label ID="lblTitle" runat="server"
                    Text="Название:" AssociatedControlID="txtTitle" /></td>
                <td><asp:TextBox ID="txtTitle" runat="server"
                    CssClass="input" placeholder="Введите название..." /></td>
            </tr>
            <tr>
                <td><asp:Label ID="lblAuthor" runat="server"
                    Text="Автор:" AssociatedControlID="txtAuthor" /></td>
                <td><asp:TextBox ID="txtAuthor" runat="server"
                    CssClass="input" placeholder="Имя автора..." /></td>
            </tr>
            <tr>
                <td><asp:Label ID="lblGenre" runat="server"
                    Text="Жанр:" AssociatedControlID="ddlGenre" /></td>
                <td>
                    <asp:DropDownList ID="ddlGenre" runat="server" CssClass="input">
                        <asp:ListItem Value="" Text="-- Все жанры --" />
                        <asp:ListItem Value="fiction" Text="Художественная" />
                        <asp:ListItem Value="science" Text="Научная" />
                        <asp:ListItem Value="education" Text="Учебная" />
                        <asp:ListItem Value="children" Text="Детская" />
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="lblYear" runat="server"
                    Text="Год:" AssociatedControlID="txtYear" /></td>
                <td>
                    <asp:TextBox ID="txtYear" runat="server"
                        CssClass="input" placeholder="Год издания..." />
                    <asp:RangeValidator ID="rvYear" runat="server"
                        ControlToValidate="txtYear"
                        Type="Integer"
                        MinimumValue="1000" MaximumValue="2100"
                        ErrorMessage="Год должен быть в диапазоне 1000–2100"
                        Display="Dynamic"
                        EnableClientScript="true"
                        CssClass="error"
                        ValidationGroup="SearchGroup" />
                </td>
            </tr>
        </table>
        <asp:Button ID="btnSearch" runat="server"
            Text=">> ИНИЦИИРОВАТЬ ПОИСК"
            OnClick="btnSearch_Click"
            CssClass="btn btn-primary"
            ValidationGroup="SearchGroup" />
        <asp:Button ID="btnReset" runat="server"
            Text="[X] СБРОС"
            OnClick="btnReset_Click"
            CssClass="btn btn-secondary"
            CausesValidation="false" />
    </asp:Panel>

    <asp:Panel ID="pnlResults" runat="server" CssClass="results-panel">
        <asp:Label ID="lblResultsCount" runat="server" CssClass="results-count mono" />

        <asp:GridView ID="gvBooks" runat="server"
            AutoGenerateColumns="false"
            OnRowCommand="gvBooks_RowCommand"
            CssClass="books-grid"
            EmptyDataText="">
            <EmptyDataTemplate>
                <%-- Исправление 3: динамическое сообщение при пустом результате --%>
                <asp:Label ID="lblEmpty" runat="server"
                    CssClass="mono" Style="color:#aaa; display:block; padding:20px 0;">
                </asp:Label>
                <asp:Panel runat="server" CssClass="mono" Style="margin-top:8px;">
                    [?] Попробуйте:
                    изменить поисковый запрос,
                    выбрать другой жанр или
                    сбросить фильтры кнопкой [X] СБРОС
                </asp:Panel>
            </EmptyDataTemplate>
            <Columns>
                <asp:BoundField DataField="Title"  HeaderText="Название"
                    ItemStyle-CssClass="col-title" />
                <asp:BoundField DataField="Author" HeaderText="Автор"
                    ItemStyle-CssClass="col-author" />
                <asp:BoundField DataField="Year"   HeaderText="Год"
                    ItemStyle-CssClass="col-year" />
                <%-- Исправление 2: жанр через TemplateField с конвертацией в русский --%>
                <asp:TemplateField HeaderText="Жанр" ItemStyle-CssClass="col-genre">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# _Default.GenreToRussian(Eval("Genre").ToString()) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Доступно" ItemStyle-CssClass="col-available">
                    <ItemTemplate>
                        <asp:Label ID="lblAvailable" runat="server"
                            Text='<%# Eval("AvailableCount") + " шт." %>'
                            CssClass='<%# (int)Eval("AvailableCount") > 0 ? "status-available" : "status-unavailable" %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Действия" ItemStyle-CssClass="col-actions">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbDetails" runat="server"
                            CommandName="ViewDetails"
                            CommandArgument='<%# Eval("BookID") %>'
                            Text="[ПОДРОБНЕЕ]"
                            CssClass="action-link mono"
                            CausesValidation="false" />
                        <%-- Исправление 1: кнопка видна всем если книга доступна.
                             Посетитель нажимает → gvBooks_RowCommand редиректит на Login.aspx --%>
                        <asp:LinkButton ID="lbBook" runat="server"
                            CommandName="Book"
                            CommandArgument='<%# Eval("BookID") %>'
                            Text="[ЗАБРОНИРОВАТЬ]"
                            CssClass="action-link book-btn mono"
                            CausesValidation="false"
                            Visible='<%# (int)Eval("AvailableCount") > 0 %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </asp:Panel>

    <asp:ValidationSummary ID="vsErrors" runat="server"
        ShowMessageBox="false"
        CssClass="error-summary"
        ValidationGroup="SearchGroup" />


</form>
</body>
</html>