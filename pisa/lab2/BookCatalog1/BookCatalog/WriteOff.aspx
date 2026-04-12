<%@ Page Language="C#" AutoEventWireup="true" CodeFile="WriteOff.aspx.cs" Inherits="WriteOff" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Списание книг</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <asp:Panel ID="pnlWriteOff" runat="server"
        CssClass="search-panel"
        Style="max-width:900px; margin:30px auto;">

        <h2>▍ СПИСАНИЕ КНИГ</h2>

        <div style="margin-bottom:16px;">
            <asp:HyperLink ID="hlBack" runat="server"
                NavigateUrl="~/Default.aspx"
                Text="[← В КАТАЛОГ]"
                CssClass="link" />
        </div>

        <%-- Фильтрация --%>
        <asp:Panel ID="pnlFilter" runat="server"
            CssClass="search-panel" Style="margin-bottom:16px;">

            <h3 style="margin-bottom:10px;">▍ ФИЛЬТР КАТАЛОГА</h3>

            <table class="search-form">
                <tr>
                    <td><asp:Label runat="server" Text="Название:"
                        AssociatedControlID="txtFilterTitle" /></td>
                    <td><asp:TextBox ID="txtFilterTitle" runat="server"
                        CssClass="input" placeholder="Введите название..." /></td>
                </tr>
                <tr>
                    <td><asp:Label runat="server" Text="Автор:"
                        AssociatedControlID="txtFilterAuthor" /></td>
                    <td><asp:TextBox ID="txtFilterAuthor" runat="server"
                        CssClass="input" placeholder="Имя автора..." /></td>
                </tr>
                <tr>
                    <td><asp:Label runat="server" Text="Жанр:"
                        AssociatedControlID="ddlFilterGenre" /></td>
                    <td>
                        <asp:DropDownList ID="ddlFilterGenre" runat="server" CssClass="input">
                            <asp:ListItem Value=""          Text="-- Все жанры --" />
                            <asp:ListItem Value="fiction"   Text="Художественная" />
                            <asp:ListItem Value="science"   Text="Научная" />
                            <asp:ListItem Value="education" Text="Учебная" />
                            <asp:ListItem Value="children"  Text="Детская" />
                        </asp:DropDownList>
                    </td>
                </tr>
            </table>

            <asp:Button ID="btnFilter" runat="server"
                Text="[>> ПРИМЕНИТЬ ФИЛЬТР]"
                OnClick="btnFilter_Click"
                CssClass="btn btn-primary"
                CausesValidation="false" />
            <asp:Button ID="btnFilterReset" runat="server"
                Text="[X] СБРОС"
                OnClick="btnFilterReset_Click"
                CssClass="btn btn-secondary"
                CausesValidation="false"
                Style="margin-left:10px;" />
        </asp:Panel>

        <%-- Причина и количество списания --%>
        <table class="search-form" style="margin-bottom:15px;">
            <tr>
                <td><asp:Label runat="server"
                    Text="Причина списания:"
                    AssociatedControlID="ddlReason"
                    CssClass="mono label-key" /></td>
                <td>
                    <asp:DropDownList ID="ddlReason" runat="server" CssClass="input">
                        <asp:ListItem Value=""            Text="-- Выберите причину --" />
                        <asp:ListItem Value="износ"       Text="Износ / повреждение" />
                        <asp:ListItem Value="устаревание" Text="Устаревание" />
                        <asp:ListItem Value="утрата"      Text="Утрата / пропажа" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="rfvReason" runat="server"
                        ControlToValidate="ddlReason"
                        InitialValue=""
                        ErrorMessage="Выберите причину списания"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="WriteOffGroup" />
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server"
                    Text="Количество штук:"
                    AssociatedControlID="txtWriteOffCount"
                    CssClass="mono label-key" /></td>
                <td>
                    <asp:TextBox ID="txtWriteOffCount" runat="server"
                        CssClass="input" Text="1"
                        Style="max-width:100px;" />
                    <asp:RequiredFieldValidator runat="server"
                        ControlToValidate="txtWriteOffCount"
                        ErrorMessage="Укажите количество штук"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="WriteOffGroup" />
                    <asp:RangeValidator runat="server"
                        ControlToValidate="txtWriteOffCount"
                        Type="Integer"
                        MinimumValue="1" MaximumValue="9999"
                        ErrorMessage="Количество: от 1 до 9999"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="WriteOffGroup" />
                    <span class="mono"
                        style="font-size:.75rem; color:var(--nier-pale);
                               margin-left:8px;">
                        (доступно: <asp:Label ID="lblAvailableHint" runat="server"
                            style="color:var(--nier-gold);" Text="—" />)
                    </span>
                </td>
            </tr>
        </table>

        <%-- Счётчик --%>
        <asp:Label ID="lblBooksCount" runat="server"
            CssClass="results-count mono"
            Style="display:block; margin-bottom:8px;" />

        <%-- Список книг для списания --%>
        <asp:GridView ID="gvBooks" runat="server"
            AutoGenerateColumns="false"
            CssClass="books-grid"
            EmptyDataText="[?] Книги по заданным критериям не найдены">
            <Columns>
                <asp:BoundField DataField="Title"  HeaderText="Название" />
                <asp:BoundField DataField="Author" HeaderText="Автор" />
                <asp:BoundField DataField="Year"   HeaderText="Год"
                    ItemStyle-Width="50px" />
                <asp:TemplateField HeaderText="Жанр" ItemStyle-Width="110px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# WriteOff.GenreToRussian(Eval("Genre").ToString()) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Статус" ItemStyle-Width="120px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# (string)Eval("Status") == "Available"
                                ? "В НАЛИЧИИ" : "ЗАБРОНИРОВАНО" %>'
                            CssClass='<%# (string)Eval("Status") == "Available"
                                ? "mono status-available" : "mono status-unavailable" %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Доступно" ItemStyle-Width="80px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# Eval("AvailableCount") %>'
                            CssClass="mono"
                            style="color:var(--nier-gold);" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Действие" ItemStyle-Width="100px">
                    <ItemTemplate>
                        <asp:LinkButton runat="server"
                            CommandArgument='<%# Eval("BookID") + ";" + Eval("AvailableCount") %>'
                            Text="[СПИСАТЬ]"
                            CssClass="action-link"
                            OnClick="lbWriteOff_Click"
                            CausesValidation="true"
                            ValidationGroup="WriteOffGroup"
                            OnClientClick="return confirm('Подтвердить списание этой книги?');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <%-- Панель подтверждения при наличии активных броней --%>
        <asp:Panel ID="pnlActiveBookings" runat="server" Visible="false"
            CssClass="search-panel"
            Style="margin-top:15px; border-color:var(--nier-error);">
            <p class="mono error-text" style="margin-bottom:12px;">
                [!] На эту книгу есть активные брони. Отменить их и списать книгу?
            </p>
            <asp:HiddenField ID="hfPendingBookId" runat="server" />
            <asp:HiddenField ID="hfPendingCount"  runat="server" />
            <asp:Button ID="btnConfirmWriteOff" runat="server"
                Text="[>> ОТМЕНИТЬ БРОНИ И СПИСАТЬ]"
                OnClick="btnConfirmWriteOff_Click"
                CssClass="btn btn-primary"
                CausesValidation="false" />
            <asp:Button ID="btnCancelWriteOff" runat="server"
                Text="[X ОТМЕНА]"
                OnClick="btnCancelWriteOff_Click"
                CssClass="btn btn-secondary"
                Style="margin-left:15px;"
                CausesValidation="false" />
        </asp:Panel>

        <asp:ValidationSummary ID="vsWriteOff" runat="server"
            ShowMessageBox="false"
            CssClass="error-summary"
            ValidationGroup="WriteOffGroup" />

        <asp:Label ID="lblMessage" runat="server"
            CssClass="message" Visible="false"
            Style="margin-top:12px; display:block;" />

        <%-- Архив списанных книг --%>
        <asp:Panel ID="pnlArchive" runat="server"
            Style="margin-top:32px; border-top:1px solid var(--nier-gray);
                   padding-top:20px;">

            <h3 style="margin-bottom:12px;">▍ АРХИВ СПИСАННЫХ КНИГ</h3>

            <asp:Label ID="lblArchiveCount" runat="server"
                CssClass="results-count mono"
                Style="display:block; margin-bottom:8px;" />

            <%-- Источник данных — WriteOffRecord, а не Book.
                 Поля: BookTitle, BookAuthor, BookISBN, Count, Reason,
                       WriteOffDate, RecordID, CanBeRestored --%>
            <asp:GridView ID="gvArchive" runat="server"
                AutoGenerateColumns="false"
                CssClass="books-grid"
                EmptyDataText="Архив пуст">
                <Columns>
                    <%-- BookTitle вместо Title --%>
                    <asp:BoundField DataField="BookTitle"  HeaderText="Название" />
                    <%-- BookAuthor вместо Author --%>
                    <asp:BoundField DataField="BookAuthor" HeaderText="Автор" />
                    <%-- BookISBN вместо Year (года в записи нет) --%>
                    <asp:BoundField DataField="BookISBN"   HeaderText="ISBN"
                        ItemStyle-Width="130px" />
                    <%-- Count — сколько экземпляров было списано в этой записи --%>
                    <asp:TemplateField HeaderText="Кол-во" ItemStyle-Width="60px">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# Eval("Count") %>'
                                CssClass="mono"
                                style="color:var(--nier-gold);" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <%-- Reason вместо WriteOffReason --%>
                    <asp:TemplateField HeaderText="Причина" ItemStyle-Width="130px">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# Server.HtmlEncode(Eval("Reason").ToString()) %>'
                                CssClass="mono"
                                style="font-size:.8rem; color:var(--nier-pale);" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <%-- WriteOffDate — тип DateTime, не nullable, проверка не нужна --%>
                    <asp:TemplateField HeaderText="Дата списания" ItemStyle-Width="130px">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# ((DateTime)Eval("WriteOffDate"))
                                    .ToString("dd.MM.yyyy HH:mm") %>'
                                CssClass="mono"
                                style="font-size:.8rem; color:var(--nier-pale);" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <%-- CommandArgument — RecordID вместо BookID --%>
                    <asp:TemplateField HeaderText="Восстановить" ItemStyle-Width="130px">
                        <ItemTemplate>
                            <asp:LinkButton runat="server"
                                CommandName="Restore"
                                CommandArgument='<%# Eval("RecordID") %>'
                                Text="[ВОССТАНОВИТЬ]"
                                CssClass='<%# (bool)Eval("CanBeRestored")
                                    ? "action-link book-btn" : "action-link" %>'
                                Enabled='<%# (bool)Eval("CanBeRestored") %>'
                                OnClick="lbRestore_Click"
                                CausesValidation="false"
                                OnClientClick="return confirm('Восстановить книгу в каталог?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>

    </asp:Panel>
</form>
</body>
</html>