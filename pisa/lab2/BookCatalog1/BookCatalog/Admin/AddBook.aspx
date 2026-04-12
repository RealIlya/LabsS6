<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AddBook.aspx.cs" Inherits="Admin_AddBook" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Добавление книги</title>
    <link href="../Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <asp:Panel ID="pnlAddBook" runat="server"
        CssClass="search-panel"
        Style="max-width:600px; margin:30px auto;">

        <h2>▍ ДОБАВЛЕНИЕ НОВОГО ЭКЗЕМПЛЯРА</h2>

        <div style="margin-bottom:12px;">
            <asp:HyperLink runat="server" NavigateUrl="~/Default.aspx"
                CssClass="link">← НАЗАД В КАТАЛОГ</asp:HyperLink>
        </div>

        <%-- Основная форма --%>
        <asp:Panel ID="pnlForm" runat="server">
            <table class="search-form">
                <tr>
                    <td><asp:Label ID="lblTitle" runat="server"
                        Text="Название:" AssociatedControlID="txtTitle" /></td>
                    <td>
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator ID="rfvTitle" runat="server"
                            ControlToValidate="txtTitle"
                            ErrorMessage="Название обязательно"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                    </td>
                </tr>
                <tr>
                    <td><asp:Label ID="lblAuthor" runat="server"
                        Text="Автор:" AssociatedControlID="txtAuthor" /></td>
                    <td>
                        <asp:TextBox ID="txtAuthor" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator ID="rfvAuthor" runat="server"
                            ControlToValidate="txtAuthor"
                            ErrorMessage="Автор обязателен"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                    </td>
                </tr>
                <tr>
                    <td><asp:Label ID="lblISBN" runat="server"
                        Text="ISBN:" AssociatedControlID="txtISBN" /></td>
                    <td>
                        <asp:TextBox ID="txtISBN" runat="server" CssClass="input" />
                        <%-- ISBN обязателен — по нему ищем дубликаты --%>
                        <asp:RequiredFieldValidator ID="rfvISBN" runat="server"
                            ControlToValidate="txtISBN"
                            ErrorMessage="ISBN обязателен"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                        <asp:RegularExpressionValidator ID="revISBN" runat="server"
                            ControlToValidate="txtISBN"
                            ValidationExpression="^(97[89]-)?\d{1,5}-\d{1,7}-\d{1,7}-[\dX]$"
                            ErrorMessage="Неверный формат ISBN, должен содержать 13 цифр в формате 978-X-XX-XXXXXX-X"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                    </td>
                </tr>
                <tr>
                    <td><asp:Label ID="lblPublisher" runat="server"
                        Text="Издательство:" AssociatedControlID="txtPublisher" /></td>
                    <td>
                        <asp:TextBox ID="txtPublisher" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator ID="rfvPublisher" runat="server"
                            ControlToValidate="txtPublisher"
                            ErrorMessage="Издательство обязательно"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                    </td>
                </tr>
                <tr>
                    <td><asp:Label ID="lblYear" runat="server"
                        Text="Год:" AssociatedControlID="txtYear" /></td>
                    <td>
                        <asp:TextBox ID="txtYear" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator ID="rfvYear" runat="server"
                            ControlToValidate="txtYear"
                            ErrorMessage="Год обязателен"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                        <asp:RangeValidator ID="rvYear" runat="server"
                            ControlToValidate="txtYear"
                            Type="Integer"
                            MinimumValue="1000" MaximumValue="2026"
                            ErrorMessage="Год: от 1000 до 2026"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                    </td>
                </tr>
                <tr>
                    <td><asp:Label ID="lblGenre" runat="server"
                        Text="Жанр:" AssociatedControlID="ddlGenre" /></td>
                    <td>
                        <asp:DropDownList ID="ddlGenre" runat="server" CssClass="input">
                            <asp:ListItem Value="fiction"   Text="Художественная" />
                            <asp:ListItem Value="science"   Text="Научная" />
                            <asp:ListItem Value="education" Text="Учебная" />
                            <asp:ListItem Value="children"  Text="Детская" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td><asp:Label ID="lblPages" runat="server"
                        Text="Страниц:" AssociatedControlID="txtPages" /></td>
                    <td>
                        <asp:TextBox ID="txtPages" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator ID="rfvPages" runat="server"
                            ControlToValidate="txtPages"
                            ErrorMessage="Количество страниц обязательно"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                        <asp:RangeValidator ID="rvPages" runat="server"
                            ControlToValidate="txtPages"
                            Type="Integer"
                            MinimumValue="1" MaximumValue="99999"
                            ErrorMessage="Страниц: от 1 до 99999"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                    </td>
                </tr>
                <tr>
                    <td><asp:Label ID="lblCount" runat="server"
                        Text="Экземпляров:" AssociatedControlID="txtCount" /></td>
                    <td>
                        <asp:TextBox ID="txtCount" runat="server"
                            CssClass="input" Text="1" />
                        <asp:RequiredFieldValidator ID="rfvCount" runat="server"
                            ControlToValidate="txtCount"
                            ErrorMessage="Количество экземпляров обязательно"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                        <asp:RangeValidator ID="rvCount" runat="server"
                            ControlToValidate="txtCount"
                            Type="Integer"
                            MinimumValue="1" MaximumValue="100"
                            ErrorMessage="Экземпляров: от 1 до 100"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="AddBookGroup" />
                    </td>
                </tr>
            </table>

            <asp:Button ID="btnAdd" runat="server"
                Text="[>> ДОБАВИТЬ]"
                OnClick="btnAdd_Click"
                CssClass="btn btn-primary"
                ValidationGroup="AddBookGroup" />

            <asp:ValidationSummary ID="vsErrors" runat="server"
                ShowMessageBox="false"
                CssClass="error-summary"
                ValidationGroup="AddBookGroup" />

            <asp:Label ID="lblMessage" runat="server"
                CssClass="message" Visible="false" />
        </asp:Panel>

        <%-- Панель подтверждения дубликата — скрыта по умолчанию --%>
        <asp:Panel ID="pnlDuplicate" runat="server" Visible="false"
            Style="margin-top:20px;">

            <h3 style="color:#c8a96e;">[!] ОБНАРУЖЕНЫ СОВПАДЕНИЯ В КАТАЛОГЕ</h3>

            <p class="mono" style="margin:8px 0;">
                Книга с таким ISBN уже существует в системе:
            </p>

            <%-- Список найденных дубликатов --%>
            <asp:GridView ID="gvDuplicates" runat="server"
                AutoGenerateColumns="false"
                CssClass="books-grid"
                Style="margin:10px 0;">
                <Columns>
                    <asp:BoundField DataField="Title"          HeaderText="Название" />
                    <asp:BoundField DataField="Author"         HeaderText="Автор" />
                    <asp:BoundField DataField="ISBN"           HeaderText="ISBN" />
                    <asp:BoundField DataField="TotalCount"     HeaderText="Всего" />
                    <asp:BoundField DataField="AvailableCount" HeaderText="Доступно" />
                </Columns>
            </asp:GridView>

            <p class="mono" style="margin:12px 0;">
                Добавить
                <asp:Label ID="lblDupCount" runat="server"
                    CssClass="username" />
                экз. к существующей записи?
            </p>

            <asp:Button ID="btnConfirmAdd" runat="server"
                Text="[OK ДОБАВИТЬ ЭКЗЕМПЛЯРЫ]"
                OnClick="btnConfirmAdd_Click"
                CssClass="btn btn-primary"
                CausesValidation="false" />

            <asp:Button ID="btnCancelAdd" runat="server"
                Text="[X ОТМЕНИТЬ]"
                OnClick="btnCancelAdd_Click"
                CssClass="btn btn-secondary"
                CausesValidation="false"
                Style="margin-left:10px;" />
        </asp:Panel>

    </asp:Panel>
</form>
</body>
</html>