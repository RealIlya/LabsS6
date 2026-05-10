<%@ Page Language="C#" AutoEventWireup="true" CodeFile="WriteOff.aspx.cs" Inherits="WriteOff" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Списание книг</title>
    <link href="Styles/nier-theme.css?v=2" rel="stylesheet" />
    <style>
        .writeoff-confirm {
            background: var(--nier-dark);
            border: 2px solid var(--nier-gold);
            padding: 20px;
            margin: 15px 0;
            border-radius: 2px;
        }
        .writeoff-confirm h3 {
            color: var(--nier-cream);
            margin-bottom: 12px;
        }
        .writeoff-book-info {
            background: var(--nier-void);
            border: 1px solid var(--nier-gray);
            padding: 12px;
            margin-bottom: 15px;
            font-family: var(--font-mono);
        }
        .writeoff-book-info .book-title {
            font-size: 1rem;
            font-weight: bold;
            color: var(--nier-white);
            margin-bottom: 4px;
        }
        .writeoff-book-info .book-meta {
            font-size: 0.8rem;
            color: var(--nier-pale);
        }
        .writeoff-actions {
            display: flex;
            gap: 12px;
            align-items: center;
            margin-top: 15px;
        }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="smMain" runat="server" EnablePageMethods="true" />

    <asp:UpdatePanel ID="upWriteOff" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
    <asp:UpdateProgress ID="updProgress" runat="server" DisplayAfter="200">
        <ProgressTemplate>
            <div style="text-align:center; padding:8px; color:var(--nier-gold);
                        font-family:var(--font-mono); font-size:0.85rem;">
                [~] Загрузка...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

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

        <asp:Label ID="lblMessage" runat="server"
            CssClass="message" Visible="false"
            Style="margin-bottom:12px; display:block;" />

        <%-- Панель подтверждения при наличии активных броней --%>
        <asp:Panel ID="pnlActiveBookings" runat="server" Visible="false"
            CssClass="search-panel"
            Style="margin-bottom:15px; border-color:var(--nier-error);">
            <p class="mono error-text" style="margin-bottom:12px;">
                [!] На эту книгу есть активные брони. Отменить их и списать книгу?
            </p>
            <asp:HiddenField ID="hfPendingBookId" runat="server" />
            <asp:HiddenField ID="hfPendingCount"  runat="server" />
            <asp:Button ID="btnConfirmWithCancelBookings" runat="server"
                Text="[>> ОТМЕНИТЬ БРОНИ И СПИСАТЬ]"
                OnClick="btnConfirmWithCancelBookings_Click"
                CssClass="btn btn-primary"
                OnClientClick="saveScroll()"
                CausesValidation="false" />
            <asp:Button ID="btnCancelActiveBookings" runat="server"
                Text="[X ОТМЕНА]"
                OnClick="btnCancelActiveBookings_Click"
                CssClass="btn btn-secondary"
                OnClientClick="saveScroll()"
                Style="margin-left:15px;"
                CausesValidation="false" />
        </asp:Panel>

        <%-- Панель подтверждения списания (появляется при клике на [СПИСАТЬ]) --%>
        <asp:Panel ID="pnlWriteOffConfirm" runat="server" Visible="false"
            CssClass="writeoff-confirm">
            <h3>▍ ПОДТВЕРЖДЕНИЕ СПИСАНИЯ</h3>

            <asp:HiddenField ID="hfSelectedBookId" runat="server" />
            <asp:HiddenField ID="hfSelectedAvailable" runat="server" />

            <div class="writeoff-book-info">
                <div class="book-title">
                    <asp:Label ID="lblSelectedTitle" runat="server" />
                </div>
                <div class="book-meta">
                    <asp:Label ID="lblSelectedMeta" runat="server" />
                    &nbsp;|&nbsp; Всего:
                    <asp:Label ID="lblSelectedTotal" runat="server"
                        style="color:var(--nier-white); font-weight:bold;" />
                    &nbsp;|&nbsp; Доступно:
                    <asp:Label ID="lblSelectedAvailable" runat="server"
                        style="color:var(--nier-gold); font-weight:bold;" />
                    &nbsp;|&nbsp; Забронировано:
                    <asp:Label ID="lblSelectedBooked" runat="server"
                        style="color:var(--nier-error); font-weight:bold;" />
                </div>
            </div>

            <table class="search-form">
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
                        Text="Количество:"
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
                        <asp:RangeValidator ID="rvWriteOffCount" runat="server"
                            ControlToValidate="txtWriteOffCount"
                            Type="Integer"
                            MinimumValue="1" MaximumValue="9999"
                            ErrorMessage="Количество: от 1 до 9999"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="WriteOffGroup" />
                    </td>
                </tr>
            </table>

            <div class="writeoff-actions">
                <asp:Button ID="btnConfirmWriteOff" runat="server"
                    Text="[>> ПОДТВЕРДИТЬ СПИСАНИЕ]"
                    OnClick="btnConfirmWriteOff_Click"
                    CssClass="btn btn-primary"
                    OnClientClick="saveScroll()"
                    ValidationGroup="WriteOffGroup" />
                <asp:Button ID="btnCancelWriteOff" runat="server"
                    Text="[X] ОТМЕНА"
                    OnClick="btnCancelWriteOff_Click"
                    CssClass="btn btn-secondary"
                    OnClientClick="saveScroll()"
                    CausesValidation="false" />
            </div>
        </asp:Panel>

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
                OnClientClick="saveScroll()"
                CausesValidation="false" />
            <asp:Button ID="btnFilterReset" runat="server"
                Text="[X] СБРОС"
                OnClick="btnFilterReset_Click"
                CssClass="btn btn-secondary"
                OnClientClick="saveScroll()"
                CausesValidation="false"
                Style="margin-left:10px;" />
        </asp:Panel>

        <%-- Счётчик --%>
        <asp:Label ID="lblBooksCount" runat="server"
            CssClass="results-count mono"
            Style="display:block; margin-bottom:8px;" />

        <%-- Список книг для списания --%>
        <div style="overflow-x:auto;">
        <asp:GridView ID="gvBooks" runat="server"
            AutoGenerateColumns="false"
            CssClass="books-grid"
            EmptyDataText="[?] Книги по заданным критериям не найдены">
            <Columns>
                <asp:BoundField DataField="Title"  HeaderText="Название" />
                <asp:BoundField DataField="Author" HeaderText="Автор" />
                <asp:BoundField DataField="Year"   HeaderText="Год"
                    ItemStyle-Width="50px" />
                <asp:TemplateField HeaderText="Жанр" ItemStyle-Width="90px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# WriteOff.GenreToRussian(Eval("Genre").ToString()) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Статус" ItemStyle-Width="100px">
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
                <asp:TemplateField HeaderText="Забр." ItemStyle-Width="60px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# (int)Eval("TotalCount") - (int)Eval("AvailableCount") %>'
                            CssClass="mono"
                            style="color:var(--nier-error);" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Действие" ItemStyle-Width="100px">
                    <ItemTemplate>
                        <asp:LinkButton runat="server"
                            CommandArgument='<%# Eval("BookID") + ";" + Eval("AvailableCount") %>'
                            Text="[СПИСАТЬ]"
                            CssClass="action-link"
                            OnClick="lbSelectForWriteOff_Click"
                            OnClientClick="sessionStorage.setItem('writeoff_scroll','0');"
                            CausesValidation="false" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        </div>

        <asp:ValidationSummary ID="vsWriteOff" runat="server"
            ShowMessageBox="false"
            CssClass="error-summary"
            ValidationGroup="WriteOffGroup" />

        <%-- Архив списанных книг --%>
        <asp:Panel ID="pnlArchive" runat="server"
            Style="margin-top:32px; border-top:1px solid var(--nier-gray);
                   padding-top:20px;">

            <h3 style="margin-bottom:12px;">▍ АРХИВ СПИСАННЫХ КНИГ</h3>

            <asp:Label ID="lblArchiveCount" runat="server"
                CssClass="results-count mono"
                Style="display:block; margin-bottom:8px;" />

            <asp:GridView ID="gvArchive" runat="server"
                AutoGenerateColumns="false"
                CssClass="books-grid"
                EmptyDataText="Архив пуст">
                <Columns>
                    <asp:BoundField DataField="BookTitle"  HeaderText="Название" />
                    <asp:BoundField DataField="BookAuthor" HeaderText="Автор" />
                    <asp:BoundField DataField="BookISBN"   HeaderText="ISBN"
                        ItemStyle-Width="130px" />
                    <asp:TemplateField HeaderText="Кол-во" ItemStyle-Width="60px">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# Eval("Count") %>'
                                CssClass="mono"
                                style="color:var(--nier-gold);" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Причина" ItemStyle-Width="130px">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# Server.HtmlEncode(Eval("Reason").ToString()) %>'
                                CssClass="mono"
                                style="font-size:.8rem; color:var(--nier-pale);" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Дата списания" ItemStyle-Width="130px">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# ((DateTime)Eval("WriteOffDate"))
                                    .ToString("dd.MM.yyyy HH:mm") %>'
                                CssClass="mono"
                                style="font-size:.8rem; color:var(--nier-pale);" />
                        </ItemTemplate>
                    </asp:TemplateField>
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
    </ContentTemplate>
    </asp:UpdatePanel>

    <script type="text/javascript">
        function saveScroll() {
            sessionStorage.setItem('writeoff_scroll', window.scrollY);
        }
        function restoreScroll() {
            var y = sessionStorage.getItem('writeoff_scroll');
            if (y !== null) {
                window.scrollTo(0, parseInt(y));
                sessionStorage.removeItem('writeoff_scroll');
            }
        }
        // Восстановление при первой загрузке (full postback)
        restoreScroll();
        // Восстановление после AJAX-постбэка (UpdatePanel)
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setTimeout(restoreScroll, 50);
            });
        }
    </script>
</form>
</body>
</html>
